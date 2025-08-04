using System.Security;

namespace RunnethOverStudio.AppToolkit.Modules.UserAccess;

/// <summary>
/// Defines methods for user authentication and credential management,
/// including secure credential creation and password verification.
/// </summary>
public interface IUserAuthenticator
{
    /// <summary>
    /// Creates a new <see cref="UserCredentials"/> instance for an application user,
    /// generating a random salt and hash for the provided password.
    /// </summary>
    /// <param name="password">The password to use for credential creation, as a <see cref="SecureString"/>.</param>
    /// <returns>
    /// A new <see cref="UserCredentials"/> object containing the generated salt, hash, and work factor.
    /// </returns>
    public UserCredentials CreateUserCredentials(SecureString password);

    /// <summary>
    /// Verifies a user's password against the stored credentials.
    /// </summary>
    /// <param name="credential">The user's stored credentials.</param>
    /// <param name="password">The password to verify, as a <see cref="SecureString"/>.</param>
    /// <returns>
    /// <c>true</c> if the password is valid; otherwise, <c>false</c>.
    /// </returns>
    public bool VerifyCredentials(UserCredentials credential, SecureString password);
}
