using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace RunnethOverStudio.AppToolkit.Core;

/// <summary>
/// Represents the result of an operation that either produced a value or an expected, explicitly modeled error.
/// </summary>
/// <typeparam name="T">The type of the successful value stored in the result.</typeparam>
/// <typeparam name="TError">
/// An enumeration describing expected failure states. The default enumeration value is reserved to represent success.
/// </typeparam>
/// <remarks>
/// <para>
/// Use <see cref="ProcessResult{T,TError}"/> when failure is an anticipated part of normal application or domain flow
/// and callers are expected to branch on a known error state. Examples include a requested resource not being found,
/// a destination no longer existing, or another explicitly modeled business outcome.
/// </para>
/// <para>
/// This type is intentionally not an exception container. Unexpected or exceptional failures should normally propagate
/// as exceptions. When a boundary deliberately needs to capture an exception as part of its result contract, use
/// <see cref="ProcessResult{T}"/> instead.
/// </para>
/// <para>
/// The default value of <typeparamref name="TError"/> is reserved for success. Define a neutral member such as
/// <c>None = 0</c> when naming that state improves readability, and use non-default values for actual errors.
/// </para>
/// <para>
/// Heavily inspired by the <see href="https://dotnet.github.io/dotNext/features/core/result.html">Result type</see>
/// from .NEXT (dotNext).
/// </para>
/// </remarks>
[Serializable]
public class ProcessResult<T, TError> where TError : struct, Enum
{
    private readonly T _value;
    private readonly TError _error;

    /// <summary>
    /// Initializes a new successful result.
    /// </summary>
    /// <param name="value">The value to be stored as the successful result.</param>
    public ProcessResult(T value)
    {
        _value = value;
        _error = default;
    }

    /// <summary>
    /// Initializes a new unsuccessful result containing an expected error.
    /// </summary>
    /// <param name="error">The expected error describing why the operation was unsuccessful.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="error"/> is the default value of <typeparamref name="TError"/>, which is reserved for success.
    /// </exception>
    public ProcessResult(TError error)
    {
        if (EqualityComparer<TError>.Default.Equals(error, default))
        {
            throw new ArgumentOutOfRangeException(nameof(error), "The default error value is reserved for successful results.");
        }

        _value = default!;
        _error = error;
    }

    /// <summary>
    /// Extracts the successful value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This result contains an expected error rather than a successful value.
    /// </exception>
    public T Value
    {
        get
        {
            Validate();
            return _value;
        }
    }

    /// <summary>
    /// Gets the successful value when present; otherwise returns the default value.
    /// </summary>
    /// <value>The successful value, if present; otherwise, <c>default</c>.</value>
    public T? ValueOrDefault => IsSuccessful ? _value : default;

    /// <summary>
    /// Gets the expected error associated with the result.
    /// </summary>
    /// <remarks>
    /// The default value represents success and is not an error. Check <see cref="IsSuccessful"/> before interpreting
    /// this property as a failure state.
    /// </remarks>
    public TError Error => _error;

    /// <summary>
    /// Indicates whether the result contains a successful value rather than an expected error.
    /// </summary>
    /// <value><see langword="true"/> when successful; otherwise, <see langword="false"/>.</value>
    public bool IsSuccessful => EqualityComparer<TError>.Default.Equals(_error, default);

    /// <summary>
    /// Returns a string that represents the current result, indicating success or failure and the associated value or error.
    /// </summary>
    public override string ToString() => IsSuccessful ? $"Success({_value})" : $"Failure({_error})";

    /// <summary>
    /// Creates a successful <see cref="ProcessResult{T,TError}"/> containing the specified value.
    /// </summary>
    /// <param name="value">The value to store in the successful result.</param>
    /// <returns>A <see cref="ProcessResult{T,TError}"/> representing a successful operation.</returns>
    public static ProcessResult<T, TError> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed <see cref="ProcessResult{T,TError}"/> containing the specified expected error.
    /// </summary>
    /// <param name="error">The expected error describing why the operation was unsuccessful.</param>
    /// <returns>A <see cref="ProcessResult{T,TError}"/> representing an expected failure.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="error"/> is the default value of <typeparamref name="TError"/>, which is reserved for success.
    /// </exception>
    public static ProcessResult<T, TError> Failure(TError error) => new(error);

    /// <summary>
    /// Attempts to extract the successful value.
    /// </summary>
    /// <param name="value">The successful value when present; otherwise, the default value.</param>
    /// <returns><see langword="true"/> when successful; otherwise, <see langword="false"/>.</returns>
    public bool TryGet([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return IsSuccessful;
    }

    /// <summary>
    /// Defines an implicit conversion from <see cref="ProcessResult{T,TError}"/> to <see cref="bool"/>.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <returns>
    /// <see langword="true"/> if the result is successful; otherwise, <see langword="false"/>.
    /// </returns>
    public static implicit operator bool(ProcessResult<T, TError> result) => result.IsSuccessful;

    /// <summary>
    /// Defines an explicit conversion from <see cref="ProcessResult{T,TError}"/> to the underlying value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">The result to extract the value from.</param>
    /// <returns>The value contained in the result if it is successful.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the result is not successful and an attempt is made to extract the value.
    /// </exception>
    public static explicit operator T(ProcessResult<T, TError> result) => result.Value;

    [StackTraceHidden]
    private void Validate()
    {
        if (!IsSuccessful)
        {
            throw new InvalidOperationException($"The process result is unsuccessful with error '{_error}'.");
        }
    }
}
