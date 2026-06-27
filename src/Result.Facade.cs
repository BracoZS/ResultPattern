namespace ResultPattern;

/// <summary>
/// Facade para crear resultados sin exponer Unit en el uso comun.
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
