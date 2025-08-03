using System;

namespace RunnethOverStudio.AppToolkit.Core.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="string"/> instances.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Returns a substring from the start of the string up to, but not including, the first occurrence of the specified character.
    /// If the character is not found, returns the entire string.
    /// </summary>
    /// <param name="that">The source string to search.</param>
    /// <param name="char">The character to search for.</param>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/68096186">this answer</see> by Dariusz Woźniak.
    /// </remarks>
    public static string GetUntil(this string that, char @char)
    {
        int IndexOf() => that.IndexOf(@char);

        return that[..(IndexOf() == -1 ? that.Length : IndexOf())];
    }
}
