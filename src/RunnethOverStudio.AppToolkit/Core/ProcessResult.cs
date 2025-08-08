using RunnethOverStudio.AppToolkit.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static RunnethOverStudio.AppToolkit.Core.Enums;

namespace RunnethOverStudio.AppToolkit.Core;

/// <summary>
/// Represents the result of a process, including any exceptions that occurred, overall status, and content value.
/// </summary>
/// <typeparam name="T">
/// The type of the content or result value produced by the process.
/// </typeparam>
[Serializable]
public class ProcessResult<T>
{
    /// <summary>
    /// Indicates whether the process completed successfully.
    /// </summary>
    public bool IsValid => Errors.IsEmpty && ((int)StatusCode >= 200) && ((int)StatusCode <= 299);

    /// <summary>
    /// General status code representing the outcome of the process.
    /// </summary>
    public StatusCodes StatusCode { get; set; }

    /// <summary>
    /// Collection of exceptions that occurred during the process.
    /// </summary>
    public ConcurrentBag<Exception> Errors { get; }

    /// <summary>
    /// The content or result value produced by the process.
    /// </summary>
    public T Content { get; set; }

    /// <summary>
    /// Creates a new <see cref="ProcessResult{T}"/> from a content value.
    /// </summary>
    /// <param name="content">The content or result value produced by the process.</param>
    /// <param name="statusCode">The status code representing the outcome of the process.</param>
    public ProcessResult(T content, StatusCodes statusCode = StatusCodes.OK)
    {
        Content = content;
        StatusCode = statusCode;
        Errors = [];
    }

    /// <summary>
    /// Creates a new <see cref="ProcessResult{T}"/> from a collection of exceptions.
    /// </summary>
    /// <param name="content">The content or result value produced by the process.</param>
    /// <param name="exceptions">The exceptions that occurred during the process.</param>
    /// <param name="statusCode">The status code representing the outcome of the process.</param>
    public ProcessResult(T content, IEnumerable<Exception> exceptions, StatusCodes statusCode = StatusCodes.OK)
    {
        Content = content;
        StatusCode = statusCode;
        Errors = [.. exceptions];
    }

    /// <summary>
    /// Generates a JSON representation of the <see cref="ProcessResult{T}"/>
    /// </summary>
    public override string ToString()
    {
        return JsonSerializer.Serialize(new
        {
            statusCode = (int)StatusCode,
            statusPhrase = StatusCode.GetDisplayName(),
            errorMessages = Errors.Select(e => e.Message).ToArray(),
            content = Content?.ToString()
        });
    }

    /// <summary>
    /// Generates a string representation of the error messages, concatenating using the specified separator between each error.
    /// </summary>
    public string ErrorsSeparated(string separator)
    {
        return string.Join(separator, Errors.Select(ex => ex.Message));
    }

    /// <summary>
    /// If there is more than one error, generates a string representation of the error messages separated by new lines and bulleted by the specified character.
    /// Else, returns the single error message.
    /// </summary>
    public string ErrorsBulleted(char bullet)
    {
        return Errors.Count == 1
            ? Errors.First().Message
            : Environment.NewLine + string.Join(Environment.NewLine, Errors.Select(e => $" {bullet} {e.Message}"));
    }

    /// <summary>
    /// Creates a <see cref="ProcessResult{T}"/> representing a successful process with the specified content.
    /// </summary>
    /// <param name="content">The content or result value produced by the process.</param>
    /// <returns>A <see cref="ProcessResult{T}"/> instance with the provided content and no errors.</returns>
    public static ProcessResult<T> Success(T content) => new(content, StatusCodes.OK);

    /// <summary>
    /// Creates a <see cref="ProcessResult{T}"/> representing a failed process with the specified status code and exceptions.
    /// </summary>
    /// <param name="content">The content or result value produced by the process.</param>
    /// <param name="statusCode">The <see cref="StatusCodes"/> value representing the outcome of the process.</param>
    /// <param name="exceptions">One or more exceptions that describe the failure.</param>
    /// <returns>
    /// A <see cref="ProcessResult{T}"/> instance containing the provided exceptions and status code.
    /// </returns>
    public static ProcessResult<T> Failure(T content, StatusCodes statusCode, params Exception[] exceptions) => new(content, exceptions, statusCode);
}
