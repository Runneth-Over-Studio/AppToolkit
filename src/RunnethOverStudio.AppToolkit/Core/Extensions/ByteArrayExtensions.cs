using System;

namespace RunnethOverStudio.AppToolkit.Core.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Array"/> instances of type <see cref="byte"/>.
/// </summary>
public static class ByteArrayExtensions
{
    /// <summary>
    /// Determines whether two byte arrays are equal by comparing their elements.
    /// </summary>
    /// <param name="array1">The first byte array to compare.</param>
    /// <param name="array2">The second byte array to compare.</param>
    /// <returns>
    /// <c>true</c> if the arrays are the same length and all corresponding elements are equal; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/2138588">this answer</see> by blowdart.
    /// </remarks>
    public static bool EqualsByteArray(this byte[] array1, byte[] array2)
    {
        if (array1.Length != array2.Length)
        {
            return false;
        }

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
            {
                return false;
            }
        }

        return true;
    }
}
