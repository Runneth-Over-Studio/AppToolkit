using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace RunnethOverStudio.AppToolkit.Core;

/// <summary>
/// Represents the result of an operation that either produced a value or intentionally captured an exception.
/// </summary>
/// <typeparam name="T">The type of the value stored in the result.</typeparam>
/// <remarks>
/// <para>
/// Use <see cref="ProcessResult{T}"/> when an exception is part of the operation's explicit result contract, such as
/// when a boundary deliberately captures an exception so the caller can inspect, forward, or defer it.
/// </para>
/// <para>
/// Expected application or domain outcomes should not be converted into exceptions merely to fit this type.
/// When callers are expected to branch on a known set of non-exceptional failure states, use
/// <see cref="ProcessResult{T,TError}"/> instead. Unexpected or exceptional failures should normally continue to
/// propagate as exceptions unless the boundary has a concrete reason to capture them.
/// </para>
/// <para>
/// Heavily inspired by the <see href="https://dotnet.github.io/dotNext/features/core/result.html">Result type</see>
/// from .NEXT (dotNext).
/// </para>
/// </remarks>
[Serializable]
public class ProcessResult<T>
{
    private readonly T _value;
    private readonly ExceptionDispatchInfo? _exception;

    /// <summary>
    /// Initializes a new successful result.
    /// </summary>
    /// <param name="value">The value to be stored as result.</param>
    public ProcessResult(T value) => this._value = value;

    /// <summary>
    /// Initializes a new unsuccessful result containing a captured exception.
    /// </summary>
    /// <param name="error">The exception representing the failure. Cannot be <see langword="null"/>.</param>
    /// <remarks>
    /// This constructor is intended for failures that are exceptional in nature. Prefer
    /// <see cref="ProcessResult{T,TError}"/> for expected failure states that are part of normal application flow.
    /// </remarks>
    public ProcessResult(Exception error) : this(ExceptionDispatchInfo.Capture(error)) { }

    /// <summary>
    /// Extracts the successful value or rethrows the captured exception.
    /// </summary>
    /// <exception cref="Exception">This result contains a captured exception.</exception>
    public T Value
    {
        get
        {
            Validate();
            return _value;
        }
    }

    /// <summary>
    /// Gets the value if present; otherwise returns the default value.
    /// </summary>
    /// <value>The value, if present; otherwise, <c>default</c>.</value>
    public T? ValueOrDefault => _value;

    /// <summary>
    /// Gets the exception associated with this result, or <see langword="null"/> when successful.
    /// </summary>
    public Exception? Error => _exception?.SourceException;

    /// <summary>
    /// Indicates whether the result contains a successful value rather than a captured exception.
    /// </summary>
    /// <value><see langword="true"/> when successful; otherwise, <see langword="false"/>.</value>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccessful => _exception is null;

    /// <summary>
    /// Returns a string that represents the current result, indicating success or failure and the associated value or error.
    /// </summary>
    public override string ToString() => IsSuccessful ? $"Success({_value})" : $"Failure({Error})";

    /// <summary>
    /// Creates a successful <see cref="ProcessResult{T}"/> containing the specified value.
    /// </summary>
    /// <param name="value">The value to store in the successful result.</param>
    /// <returns>A <see cref="ProcessResult{T}"/> representing a successful operation.</returns>
    public static ProcessResult<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed <see cref="ProcessResult{T}"/> containing the specified exception.
    /// </summary>
    /// <param name="error">The exception representing the failure. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="ProcessResult{T}"/> representing an exceptional failure.</returns>
    /// <remarks>
    /// Prefer <see cref="ProcessResult{T,TError}.Failure(TError)"/> when the failure is an expected outcome that callers
    /// should handle without exception semantics.
    /// </remarks>
    public static ProcessResult<T> Failure(Exception error) => new(error);

    /// <summary>
    /// Logs a failure message and exception using the specified logger and log level, so long as logging is enabled.
    /// Then returns a failed <see cref="ProcessResult{T}"/> containing a new exception with the provided message and
    /// the original exception as its inner exception.
    /// </summary>
    /// <param name="message">The message to log and to use as the new exception's message.</param>
    /// <param name="error">The original exception to be wrapped and logged.</param>
    /// <param name="logLevel">The severity level at which to log the message.</param>
    /// <param name="logger">The logger to use for logging the failure.</param>
    /// <returns>
    /// A failed <see cref="ProcessResult{T}"/> containing a new exception with the specified message and the original exception as its inner exception.
    /// </returns>
    public static ProcessResult<T> LogAndForwardException(string message, Exception error, ILogger logger, LogLevel logLevel = LogLevel.Error)
    {
        if (logger.IsEnabled(logLevel))
        {
            logger.Log(logLevel, error, "{Message}", message);
        }

        return Failure(new Exception(message, innerException: error));
    }

    /// <summary>
    /// Defines an implicit conversion from <see cref="ProcessResult{T}"/> to <see cref="bool"/>.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if the result is successful; otherwise, <see langword="false"/>.
    /// </returns>
    public static implicit operator bool(ProcessResult<T> result) => result.IsSuccessful;

    /// <summary>
    /// Defines an explicit conversion from <see cref="ProcessResult{T}"/> to the underlying value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">The result to extract the value from.</param>
    /// <returns>The value contained in the result if it is successful.</returns>
    /// <exception cref="Exception">
    /// Thrown if the result is not successful and an attempt is made to extract the value.
    /// </exception>
    public static explicit operator T(ProcessResult<T> result) => result.Value;

    [StackTraceHidden]
    private void Validate() => _exception?.Throw();

    private ProcessResult(ExceptionDispatchInfo dispatchInfo)
    {
        Unsafe.SkipInit(out _value);
        _exception = dispatchInfo;
    }
}
