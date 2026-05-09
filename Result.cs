using System;

namespace ResultPattern;

/// <summary>
/// Facade (fachada) para crear resultados sin exponer Unit en el uso comun.
/// </summary>
public partial class Result
{
    // Exito para operaciones con valor.
    public static Result<T> Ok<T>(T value)      
        => Result<T>.Ok(value);
    
    // Fallo para operaciones con valor.
    public static Result<T> Failure<T>(Error error)     
        => Result<T>.Failure(error);
}

public sealed partial class Result
{
    private Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    // factory methods
    public static Result Ok() 
        => new(true, Error.None);
    public static Result Failure(Error error)
        => new(false, error);

    // implicit operators
    // Error -> Result
    public static implicit operator Result(Error error) 
        => Failure(error);

    public override string ToString() 
        => IsSuccess ? "Result(Success)" : $"Result(Failure: {Error.Message})";
}

/// <summary>
/// Resultado de una operacion que puede terminar en exito o error esperado.
/// </summary>
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
            throw new InvalidOperationException("Un resultado fallido debe tener un error.");

        IsSuccess = false;
        Error = error;
        _value = default;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public T Value
        => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Un resultado fallido no tiene valor.");

    // factory methods
    public static Result<T> Ok(T value)
        => new(value);
    public static Result<T> Failure(Error error)
        => new(error);

    // T -> Result Permite devolver un valor directamente desde metodos Result<T>.
    public static implicit operator Result<T>(T value)
        => Ok(value);

    // Error -> Result Permite devolver un Error directamente desde metodos Result<T>.
    public static implicit operator Result<T>(Error error)
        => Failure(error);

    // Result<T> -> Result
    public static implicit operator Result(Result<T> result)
        => result.IsSuccess
            ? Result.Ok() 
            : Result.Failure(result.Error);

    public override string ToString()
        => IsSuccess ? $"Result(Success: {Value})" : $"Result(Failure: {Error.Message})";
}