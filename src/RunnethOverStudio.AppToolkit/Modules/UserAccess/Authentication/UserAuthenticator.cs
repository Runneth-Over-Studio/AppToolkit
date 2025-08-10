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
    private const int SALT_LENGTH = 32;
    private const int DEFAULT_NEW_USER_WORK_FACTOR = 10000;

    private readonly int _newUserWorkFactor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAuthenticator"/> class with an optional work factor for hashing new passwords.
    /// </summary>
    /// <param name="newUserWorkFactor">The work factor (number of iterations) to use for new user password hashes. If not specified, a default value is used.</param>
    public UserAuthenticator(int? newUserWorkFactor = null)
    {
        _newUserWorkFactor = newUserWorkFactor ?? DEFAULT_NEW_USER_WORK_FACTOR;
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
                LoginHash = GenerateHash(pBytes, loginSalt, _newUserWorkFactor),
                LoginWorkFactor = _newUserWorkFactor
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

    private static byte[] GenerateSalt()
    {
        byte[] bytes = new byte[SALT_LENGTH];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return bytes;
    }

    private static byte[] GenerateHash(byte[] password, byte[] salt, int workFactor)
    {
        using Rfc2898DeriveBytes deriveBytes = new(password, salt, workFactor, HashAlgorithmName.SHA256);
        return deriveBytes.GetBytes(SALT_LENGTH);
    }
}
