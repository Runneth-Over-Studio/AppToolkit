using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace RunnethOverStudio.AppToolkit.Core.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Enum"/> types.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Retrieves a custom attribute of the specified type that is applied to the given enum value.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to retrieve.</typeparam>
    /// <param name="enumValue">The enum value to inspect for the attribute.</param>
    /// <returns>
    /// The custom attribute of type <typeparamref name="TAttribute"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
    {
        return enumValue
            .GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<TAttribute>();
    }

    /// <summary>
    /// Gets the display name for the specified enum value using the <see cref="DisplayAttribute"/> if present;
    /// otherwise, returns the enum value's name as a string.
    /// </summary>
    /// <param name="enumValue">The enum value to get the display name for.</param>
    /// <returns>
    /// The display name defined by the <see cref="DisplayAttribute"/>, or the enum value's name if no attribute is present.
    /// </returns>
    public static string GetDisplayName(this Enum enumValue)
    {
        DisplayAttribute? displayAttribute = enumValue.GetAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? enumValue.ToString();
    }
}
