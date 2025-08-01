using System;

namespace RunnethOverStudio.AppToolkit.Modules.UserAccess;

/// <summary>
/// Represents a data transfer object containing credential information for a user,
/// including identifiers, cryptographic salt and hash, and password hashing parameters.
/// </summary>
public record UserCredentials
{
    /// <summary>
    /// Unique identifier for the user credential entry.
    /// </summary>
    public int UserCredentialId { get; init; }

    /// <summary>
    /// Identifier of the end user associated with these credentials.
    /// </summary>
    public int EndUserId { get; init; }

    /// <summary>
    /// Cryptographic salt used for hashing the user's login credentials.
    /// </summary>
    public required byte[] LoginSalt { get; init; }

    /// <summary>
    /// Hashed value of the user's login credentials.
    /// </summary>
    public required byte[] LoginHash { get; init; }

    /// <summary>
    /// Work factor (e.g., cost parameter) used in the password hashing algorithm.
    /// </summary>
    public required int LoginWorkFactor { get; init; }
}
