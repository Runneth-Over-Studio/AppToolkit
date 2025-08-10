using RunnethOverStudio.AppToolkit.Core.Extensions;
using System.Security;
using System.Security.Cryptography;

namespace RunnethOverStudio.AppToolkit.Modules.UserAccess;

/// <summary>
/// Provides a simple implementation of <see cref="IUserAuthenticator"/> for user authentication and credential management,
/// including login verification and secure credential creation. This implementation is intended for straightforward 
/// scenarios and is not designed to provide advanced features or the robustness of enterprise solutions.
/// </summary>
/// <remarks>
/// Inspired by this <see href="https://www.mking.net/blog/password-security-best-practices-with-examples-in-csharp">article</see> by Matthew King.
/// </remarks>
public sealed class UserAuthenticator : IUserAuthenticator
{
    private readonly CryptographyConfig _cryptographyConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAuthenticator"/> class with optional cryptographic configuration settings.
    /// </summary>
    /// <param name="cryptographyConfig">
    /// The configuration settings for cryptographic operations, such as salt length, work factor, and hash algorithm.
    /// If <c>null</c>, default settings will be used.
    /// </param>
    public UserAuthenticator(CryptographyConfig? cryptographyConfig = null)
    {
        _cryptographyConfig = cryptographyConfig ?? new CryptographyConfig();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Does not enforce any password policy.
    /// </remarks>
    public CryptographyCredential CreateUserCredentials(SecureString password)
    {
        byte[] loginSalt = GenerateSalt();
        byte[] pBytes = password.ToBytes();

        try
        {
            return new CryptographyCredential()
            {
                LoginSalt = loginSalt,
                LoginHash = GenerateHash(pBytes, loginSalt, _cryptographyConfig.NewUserWorkFactor),
                LoginWorkFactor = _cryptographyConfig.NewUserWorkFactor
            };
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pBytes);
        }
    }

    /// <inheritdoc/>
    public bool VerifyCredentials(CryptographyCredential credential, SecureString password)
    {
        byte[] pBytes = password.ToBytes();

        try
        {
            byte[] checkHash = GenerateHash(pBytes, credential.LoginSalt, credential.LoginWorkFactor);

            return CryptographicOperations.FixedTimeEquals(credential.LoginHash, checkHash);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pBytes);
        }
    }

    private byte[] GenerateSalt()
    {
        byte[] bytes = new byte[_cryptographyConfig.SaltLength];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return bytes;
    }

    private byte[] GenerateHash(byte[] password, byte[] salt, int workFactor)
    {
        using Rfc2898DeriveBytes deriveBytes = new(password, salt, workFactor, _cryptographyConfig.AlgorithmName);
        return deriveBytes.GetBytes(_cryptographyConfig.SaltLength);
    }
}
