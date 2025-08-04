using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace RunnethOverStudio.AppToolkit.Core.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="SecureString"/> instances.
/// </summary>
public static class SecureStringExtensions
{
    /// <summary>
    /// Converts the specified <see cref="SecureString"/> to a byte array using ASCII encoding.
    /// </summary>
    /// <remarks>
    /// Note that SecureStrings are not actually secure and are not intended to be used for long-term storage of sensitive data.
    /// They merely prevent someone looking into a memory dump from finding plain text strings too easily.
    /// </remarks>
    /// <returns>
    /// Byte array containing sensitive data that should be cleared with <see cref="CryptographicOperations.ZeroMemory"/> after use.
    /// </returns>
    public static byte[] ToBytes(this SecureString secureString)
    {
        return Encoding.ASCII.GetBytes(secureString.ToPlainString());
    }

    /// <summary>
    /// Converts the specified <see cref="SecureString"/> to its plain text string representation.
    /// </summary>
    /// <remarks>
    /// Note that SecureStrings are not actually secure and are not intended to be used for long-term storage of sensitive data.
    /// They merely prevent someone looking into a memory dump from finding plain text strings too easily.
    /// </remarks>
    public static string ToPlainString(this SecureString secureString)
    {
        // ref: https://stackoverflow.com/a/38016279

        return new NetworkCredential(string.Empty, secureString).Password;
    }
}
