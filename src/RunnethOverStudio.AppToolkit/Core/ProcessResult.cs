using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace RunnethOverStudio.AppToolkit.Core;

/// <summary>
/// Represents a result of operation which can be the actual result or exception.
/// </summary>
/// <typeparam name="T">The type of the value stored in the Result.</typeparam>
/// <remarks>
/// Heavily inspired by the <see href="https://dotnet.github.io/dotNext/features/core/result.html">Result type</see> from .NEXT (dotNext).
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
    /// Initializes a new unsuccessful result.
    /// </summary>
    /// <param name="error">The exception representing error. Cannot be <see langword="null"/>.</param>
    public ProcessResult(Exception error) : this(ExceptionDispatchInfo.Capture(error)) { }

    /// <summary>
    /// Extracts the actual result.
    /// </summary>
    /// <exception cref="Exception">This result is not successful.</exception>
    public T Value
    {
        get
        {
            Validate();
            return _value;
        }
    }

    /// <summary>
    /// Gets the value if present; otherwise return default value.
    /// </summary>
    /// <value>The value, if present, otherwise <c>default</c>.</value>
    public T? ValueOrDefault => _value;

    /// <summary>
    /// Gets exception associated with this result.
    /// </summary>
    public Exception? Error => _exception?.SourceException;

    /// <summary>
    /// Indicates that the result is successful.
    /// </summary>
    /// <value><see langword="true"/> if this result is successful; <see langword="false"/> if this result represents exception.</value>
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
    /// <returns>A <see cref="ProcessResult{T}"/> representing a failed operation.</returns>
    public static ProcessResult<T> Failure(Exception error) => new(error);

    /// <summary>
    /// Defines an implicit conversion from <see cref="ProcessResult{T}"/> to <see cref="bool"/>.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the result is successful; otherwise, <c>false</c>.
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

