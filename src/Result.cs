namespace ResultPattern;

/// <summary>
/// Represents the result of an operation that can succeed with a value or fail with an error.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        IsSuccess = true;
        Error = Error.None;
        _value = value;
    }

    private Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.Type == ErrorType.None)
            throw new InvalidOperationException("A failed result must have a non-none error.");

        IsSuccess = false;
        Error = error;
        _value = default;
    }

    /// <summary>Gets a value indicating whether the result represents a success.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the result represents a failure.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error associated with the result, if any.</summary>
    public Error Error { get; }

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public T Value
        => IsSuccess
            ? _value!
            : throw new InvalidOperationException("A failed result has no value.");

    // factory methods
    /// <summary>Creates a successful <see cref="Result{T}"/> with the specified value.</summary>
    /// <param name="value">The success value.</param>
    public static Result<T> Ok(T value)
        => new(value);

    /// <summary>Creates a failed <see cref="Result{T}"/> with the specified error.</summary>
    /// <param name="error">The error describing the failure.</param>
    public static Result<T> Failure(Error error)
        => new(error);

    // T -> Result Permite devolver un valor directamente desde metodos Result<T>.
    /// <summary>
    /// Implicitly converts a value to a successful <see cref="Result{T}"/>.
    /// </summary>
    public static implicit operator Result<T>(T value)
        => Ok(value);

    // Error -> Result Permite devolver un Error directamente desde metodos Result<T>.
    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed <see cref="Result{T}"/>.
    /// </summary>
    public static implicit operator Result<T>(Error error)
        => Failure(error);

    // Result<T> -> Result
    /// <summary>
    /// Implicitly converts a <see cref="Result{T}"/> to a void <see cref="Result"/>,
    /// preserving the success or failure state.
    /// </summary>
    public static implicit operator Result(Result<T> result)
        => result.IsSuccess
            ? Result.Ok()
            : Result.Failure(result.Error);

    /// <summary>Returns a string representation of the result.</summary>
    public override string ToString()
        => IsSuccess ? $"Result(Success: {Value})" : $"Result(Failure: {Error.Message})";
}

/// <summary>
/// Represents the result of a void operation that can succeed or fail with an error.
/// </summary>
public sealed partial class Result
{
    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Gets a value indicating whether the result represents a success.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the result represents a failure.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error associated with the result, if any.</summary>
    public Error Error { get; }

    // factory methods
    /// <summary>Creates a successful void <see cref="Result"/>.</summary>
    public static Result Ok()
        => new(true, Error.None);

    /// <summary>Creates a failed void <see cref="Result"/> with the specified error.</summary>
    /// <param name="error">The error describing the failure.</param>
    public static Result Failure(Error error)
        => new(false, error);

    // implicit operators
    // Error -> Result
    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a failed void <see cref="Result"/>.
    /// </summary>
    public static implicit operator Result(Error error)
        => Failure(error);

    /// <summary>Returns a string representation of the result.</summary>
    public override string ToString()
        => IsSuccess ? "Result(Success)" : $"Result(Failure: {Error.Message})";
}