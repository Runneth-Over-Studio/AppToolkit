using System;

namespace RunnethOverStudio.AppToolkit.Modules.UserAccess;

/// <summary>
/// Represents a data transfer object containing credential material used in cryptographic operations.
/// </summary>
public record CryptographyCredential
{
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
