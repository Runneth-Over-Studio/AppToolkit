using System;

namespace RunnethOverStudio.AppToolkit.Core.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Random"/> instances.
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    /// Returns a random <see cref="decimal"/> value (with decimal-place precision of a double) within the specified range.
    /// </summary>
    /// <param name="random">The <see cref="Random"/> instance to use for generating the value.</param>
    /// <param name="minValue">The inclusive lower bound of the random number returned. Defaults to <see cref="decimal.MinValue"/>.</param>
    /// <param name="maxValue">The inclusive upper bound of the random number returned. Defaults to <see cref="decimal.MaxValue"/>.</param>
    /// <returns>A random decimal value, with decimal-place precision of a double, between <paramref name="minValue"/> and <paramref name="maxValue"/>.</returns>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/28860710">this answer</see> by Cassandra Grace.
    /// </remarks>
    public static decimal NextDecimal(this Random random, decimal minValue = decimal.MinValue, decimal maxValue = decimal.MaxValue)
    {
        decimal randomDecimal = new(random.NextDouble());

        return maxValue * randomDecimal + minValue * (1 - randomDecimal);
    }
}
