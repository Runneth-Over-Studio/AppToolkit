using System.Security.Cryptography;

namespace RunnethOverStudio.AppToolkit.Modules.UserAccess;

/// <summary>
/// Represents configuration settings for cryptographic operations used in user authentication.
/// </summary>
public record CryptographyConfig
{
    /// <summary>
    /// The length, in bytes, of the cryptographic salt to be generated for password hashing.
    /// </summary>
    /// <remarks>
    /// 32 bytes (256 bits) exceeds the minimum recommended by NIST and OWASP and is suitable for PBKDF2.
    /// </remarks>
    public int SaltLength { get; init; } = 32;

    /// <summary>
    /// The work factor to use for key derivation functions such as PBKDF2 when creating new hashes.
    /// Higher values increase computational cost and security.
    /// </summary>
    /// <remarks>
    /// OWASP recommends at least 10,000 iterations, but the ideal value depends on your hardware and performance requirements.
    /// </remarks>
    public int NewUserWorkFactor { get; init; } = 10000;

    /// <summary>
    /// The name of the hash algorithm to use for password hashing (e.g., "SHA256").
    /// </summary>
    /// <remarks>
    /// SHA-256 is a widely supported choice for PBKDF2.
    /// </remarks>
    public HashAlgorithmName AlgorithmName { get; init; } = HashAlgorithmName.SHA256;
}
