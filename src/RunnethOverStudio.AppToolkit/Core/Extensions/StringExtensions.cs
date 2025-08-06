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
    /// <param name="s">The source string to search.</param>
    /// <param name="char">The character to search for.</param>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/68096186">this answer</see> by Dariusz Woźniak.
    /// </remarks>
    public static string GetUntil(this string s, char @char)
    {
        int IndexOf() => s.IndexOf(@char);

        return s[..(IndexOf() == -1 ? s.Length : IndexOf())];
    }

    /// <summary>
    /// Returns a substring after the last occurrence of the specified character.
    /// If the character is not found, returns an empty string.
    /// </summary>
    /// <param name="s">The source string to search.</param>
    /// <param name="char">The character to search for.</param>
    public static string GetAfterLast(this string s, char @char)
    {
        int index = s.LastIndexOf(@char);

        return index == -1 || index == s.Length - 1
            ? string.Empty
            : s[(index + 1)..];
    }
}
