using System;
using System.Collections.Generic;

namespace RunnethOverStudio.AppToolkit.Core;

/// <summary>
/// Represents the result of validating a business or domain object, including property-level validation messages.
/// </summary>
/// <typeparam name="T">The type of the validated object.</typeparam>
[Serializable]
public class ValidationResult<T>
{
    /// <summary>
    /// Gets the validated object.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid => ValidationMessages.Count == 0 && GeneralMessages.Count == 0;

    /// <summary>
    /// Gets the validation messages, keyed by property name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> ValidationMessages { get; }

    /// <summary>
    /// Gets the general (object-level) validation messages not associated with a specific property.
    /// </summary>
    public IReadOnlyList<string> GeneralMessages { get; }

    /// <summary>
    /// Initializes a successful validation result.
    /// </summary>
    /// <param name="value">The validated object.</param>
    public ValidationResult(T value) : this(value, new Dictionary<string, string[]>(), []) { }

    /// <summary>
    /// Initializes a failed validation result with property and/or general messages.
    /// </summary>
    /// <param name="value">The validated object.</param>
    /// <param name="validationMessages">Validation messages keyed by property name.</param>
    /// <param name="generalMessages">General validation messages not tied to a property.</param>
    public ValidationResult(T value, IDictionary<string, string[]>? validationMessages, IList<string>? generalMessages = null)
    {
        Value = value;
        ValidationMessages = validationMessages is not null ? new Dictionary<string, string[]>(validationMessages) : [];
        GeneralMessages = generalMessages is not null ? new List<string>(generalMessages) : [];
    }

    /// <summary>
    /// Returns a string that represents the current validation result, including general and property-level messages.
    /// </summary>
    public override string ToString()
    {
        List<string> lines = [ .. GeneralMessages ];

        foreach (KeyValuePair<string, string[]> kvp in ValidationMessages)
        {
            string property = kvp.Key;
            foreach (var message in kvp.Value)
            {
                lines.Add($"{property} - {message}");
            }
        }

        return string.Join('\n', lines);
    }
}
