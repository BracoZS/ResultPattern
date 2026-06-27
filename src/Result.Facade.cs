namespace ResultPattern;

/// <summary>
/// Convenience facade for creating <see cref="Result{T}"/> instances
/// without exposing the underlying <c>Unit</c> type in common usage.
/// </summary>
public partial class Result
{
    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> wrapping the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing <paramref name="value"/>.</returns>
    public static Result<T> Ok<T>(T value)
        => Result<T>.Ok(value);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the value that would have been returned on success.</typeparam>
    /// <param name="error">The error describing the failure.</param>
    /// <returns>A failed <see cref="Result{T}"/> containing <paramref name="error"/>.</returns>
    public static Result<T> Failure<T>(Error error)
        => Result<T>.Failure(error);
}
