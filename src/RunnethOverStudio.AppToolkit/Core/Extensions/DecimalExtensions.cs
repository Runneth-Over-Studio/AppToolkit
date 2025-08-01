using System;

namespace RunnethOverStudio.AppToolkit.Core.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="decimal"/> instances.
/// </summary>
public static class DecimalExtensions
{
    /// <summary>
    /// Truncates the specified decimal value to the given number of decimal places without rounding up.
    /// </summary>
    /// <param name="source">The decimal value to truncate.</param>
    /// <param name="precision">The number of decimal places to keep.</param>
    /// <returns>
    /// The truncated decimal value with the specified precision.
    /// </returns>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/43639947">this answer</see> by D. Nesterov.
    /// </remarks>
    public static decimal PrecisionTruncate(this decimal source, byte precision)
    {
        decimal r = Math.Round(source, precision);

        if (source > 0 && r > source)
        {
            return r - new decimal(1, 0, 0, false, precision);
        }
        else if (source < 0 && r < source)
        {
            return r + new decimal(1, 0, 0, false, precision);
        }

        return r;
    }

    /// <summary>
    /// Rounds the decimal value to the nearest multiple of the specified snap value.
    /// </summary>
    /// <param name="source">The decimal value to round.</param>
    /// <param name="snap">The interval to which the value should be snapped. If zero, rounds to the nearest integer.</param>
    /// <returns>
    /// The value of <paramref name="source"/> rounded to the nearest multiple of <paramref name="snap"/>.
    /// </returns>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/66598268">this answer</see> by Botanic.
    /// </remarks>
    public static decimal SnapToNearest(this decimal source, decimal snap)
    {
        if (snap == 0.0M)
        {
            return Math.Round(source, MidpointRounding.AwayFromZero);
        }
        else if (snap <= 1.0M)
        {
            return Math.Floor(source) + (Math.Round((source - Math.Floor(source)) * (1.0M / snap)) * snap);
        }
        else
        {
            return Math.Round(source / snap) * snap;
        }
    }
}
