using RunnethOverStudio.AppToolkit.Core.Extensions;
using System.Security;
using System.Security.Cryptography;

namespace RunnethOverStudio.AppToolkit.Modules.UserAccess;

/// <summary>
/// Provides basic methods for user authentication and credential management,
/// including login verification and secure credential creation.
/// </summary>
/// <remarks>
/// Inspired by this <see href="https://www.mking.net/blog/password-security-best-practices-with-examples-in-csharp">article</see> by Matthew King.
/// </remarks>
public static class Authorizer
{
    private const int SALT_LENGTH = 32;
    private const int NEW_USER_WORK_FACTOR = 10000;

    /// <summary>
    /// Verifies a user's password against the stored credentials.
    /// </summary>
    /// <param name="credential">The user's stored credentials.</param>
    /// <param name="password">The password to verify, as a <see cref="SecureString"/>.</param>
    /// <returns><c>true</c> if the password is valid; otherwise, <c>false</c>.</returns>
    public static bool Login(UserCredentials credential, SecureString password)
    {
        return credential.LoginHash.EqualsByteArray(GenerateHash(
            password.ToBytes(),
            credential.LoginSalt,
            credential.LoginWorkFactor));
    }

    /// <summary>
    /// Creates a new <see cref="UserCredentials"/> instance for an application user,
    /// generating a random salt and hash for the provided password.
    /// </summary>
    /// <param name="password">The password to use for credential creation, as a <see cref="SecureString"/>.</param>
    /// <returns>A new <see cref="UserCredentials"/> object containing the generated salt, hash, and work factor.</returns>
    public static UserCredentials CreateAppCredential(SecureString password)
    {
        byte[] loginSalt = GenerateSalt();

        return new UserCredentials()
        {
            LoginSalt = loginSalt,
            LoginHash = GenerateHash(password.ToBytes(), loginSalt, NEW_USER_WORK_FACTOR),
            LoginWorkFactor = NEW_USER_WORK_FACTOR
        };
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
