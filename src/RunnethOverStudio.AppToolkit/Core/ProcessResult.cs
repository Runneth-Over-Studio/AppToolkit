using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RunnethOverStudio.AppToolkit.Core;

/// <summary>
/// Represents the result of a process, including any exceptions that occurred.
/// Provides methods to aggregate errors and determine overall process validity.
/// </summary>
[Serializable]
public class ProcessResult
{
    /// <summary>
    /// Whether the process succeeded.
    /// </summary>
    public virtual bool IsValid => Errors.IsEmpty;

    /// <summary>
    /// Collection of exceptions that occurred during the process.
    /// </summary>
    public ConcurrentBag<Exception> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessResult"/> class.
    /// </summary>
    public ProcessResult()
    {
        Errors = [];
    }

    /// <summary>
    /// Creates a new <see cref="ProcessResult"/> from a collection of exceptions.
    /// </summary>
    public ProcessResult(IEnumerable<Exception> exceptions)
    {
        Errors = [.. exceptions];
    }

    /// <summary>
    /// Creates a new <see cref="ProcessResult"/> by combining several others.
    /// </summary>
    public ProcessResult(IEnumerable<ProcessResult> otherResults)
    {
        Errors = [.. otherResults.SelectMany(x => x.Errors)];
    }

    /// <summary>
    /// Creates a <see cref="ProcessResult"/> representing a successful process with no errors.
    /// </summary>
    /// <returns>A <see cref="ProcessResult"/> instance with an empty error collection.</returns>
    public static ProcessResult Success() => new();

    /// <summary>
    /// Creates a <see cref="ProcessResult"/> representing a failed process with the specified exceptions.
    /// </summary>
    /// <param name="exceptions">One or more exceptions that describe the failure.</param>
    /// <returns>A <see cref="ProcessResult"/> instance containing the provided exceptions.</returns>
    public static ProcessResult Failure(params Exception[] exceptions) => new(exceptions);

    /// <summary>
    /// Generates a string representation of the error messages separated by new lines.
    /// </summary>
    public override string ToString()
    {
        return ToString(Environment.NewLine);
    }

    /// <summary>
    /// Generates a string representation of the error messages, concatenating using the specified separator between each error.
    /// </summary>
    public string ToString(string separator)
    {
        return string.Join(separator, Errors.Select(ex => ex.Message));
    }

    /// <summary>
    /// If there is more than one error, generates a string representation of the error messages separated by new lines and bulleted by the specified character.
    /// Else, returns the single error message.
    /// </summary>
    public string ToStringBulleted(char bullet)
    {
        return Errors.Count == 1
            ? Errors.First().Message
            : Environment.NewLine + string.Join(Environment.NewLine, Errors.Select(e => $" {bullet} {e.Message}"));
    }
}
