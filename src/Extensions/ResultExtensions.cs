namespace ResultPattern;

public static class ResultExtensions
{
    #region Result<T> 
    // Match<T, TOut>
    // Convierte el resultado en un valor final, cierra el flujo.
    /// <summary>Converts the result into a single value by applying one of two functions.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="onSuccess">Function to apply when the result is successful.</param>
    /// <param name="onFailure">Function to apply when the result is a failure.</param>
    /// <returns>The result of <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    public static TOut Match<T, TOut>(
        this Result<T> result,
        Func<T, TOut> onSuccess,
        Func<Error, TOut> onFailure)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    // Switch<T>
    // Ejecuta una accion segun el estado del resultado. Terminal.
    /// <summary>Executes an action depending on the result state. Terminal operation.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="onSuccess">Action to execute when successful.</param>
    /// <param name="onFailure">Action to execute when failed.</param>
    public static void Switch<T>(
        this Result<T> result,
        Action<T> onSuccess,
        Action<Error> onFailure)
    {
        if(result.IsSuccess)
            onSuccess(result.Value);
        else
            onFailure(result.Error);
    }

    // Map
    // Transforma el valor si el resultado fue exitoso.
    /// <summary>Transforms the success value if the result is successful.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="mapper">Function to transform the value.</param>
    /// <returns>A new <see cref="Result{T}"/> with the transformed value, or the original error.</returns>
    public static Result<TOut> Map<T, TOut>(
        this Result<T> result,
        Func<T, TOut> mapper)
        => result.IsSuccess
            ? Result.Ok(mapper(result.Value))
            : Result.Failure<TOut>(result.Error);

    // Bind<T, TOut>
    // Encadena otra operacion que tambien puede fallar.
    /// <summary>Chains another operation that may also fail.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="next">Function that returns a new <see cref="Result{T}"/>.</param>
    /// <returns>The result of <paramref name="next"/> on success, or the original error.</returns>
    public static Result<TOut> Bind<T, TOut>(
        this Result<T> result,
        Func<T, Result<TOut>> next)
        => result.IsSuccess
            ? next(result.Value)
            : Result.Failure<TOut>(result.Error);

    // Bind<T>
    // Result<T> -> Result
    /// <summary>Chains another operation that returns a void <see cref="Result"/>.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="next">Function that returns a void <see cref="Result"/>.</param>
    /// <returns>The result of <paramref name="next"/> on success, or a void failure.</returns>
    public static Result Bind<T>(
        this Result<T> result,
        Func<T, Result> next)
        => result.IsSuccess
            ? next(result.Value)
            : Result.Failure(result.Error);

    // OnSuccess
    // Ejecuta una accion lateral si el resultado fue exitoso.
    /// <summary>Executes a side effect when the result is successful. Returns the same result.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="action">Action to execute on the success value.</param>
    /// <returns>The original <see cref="Result{T}"/> unchanged.</returns>
    public static Result<T> OnSuccess<T>(
        this Result<T> result,
        Action<T> action)
    {
        if(result.IsSuccess)
            action(result.Value);

        return result;
    }

    // OnFailure
    // Ejecuta una accion lateral si el resultado fallo.
    /// <summary>Executes a side effect when the result is a failure. Returns the same result.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="action">Action to execute on the error.</param>
    /// <returns>The original <see cref="Result{T}"/> unchanged.</returns>
    public static Result<T> OnFailure<T>(
        this Result<T> result,
        Action<Error> action)
    {
        if(result.IsFailure)
            action(result.Error);

        return result;
    }

    // Ensure
    // Valida el valor exitoso sin salir del flujo.
    /// <summary>Validates the success value without breaking the chain.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="predicate">Validation predicate on the success value.</param>
    /// <param name="error">Error to return if validation fails.</param>
    /// <returns>The original result if valid, or a new failure with <paramref name="error"/>.</returns>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if(result.IsFailure)
            return result;

        return predicate(result.Value)
            ? result
            : Result.Failure<T>(error);
    }
    #endregion

    #region Result
    // Match<T>
    // Convierte un Result en un valor final.
    /// <summary>Converts a void <see cref="Result"/> into a single value.</summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="onSuccess">Function to call when successful.</param>
    /// <param name="onFailure">Function to call when failed.</param>
    /// <returns>The result of <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
            => result.IsSuccess
                ? onSuccess()
                : onFailure(result.Error);

    // Switch
    // Ejecuta una accion segun el estado de un Result. Terminal.
    /// <summary>Executes an action depending on the void result state. Terminal operation.</summary>
    /// <param name="onSuccess">Action to execute when successful.</param>
    /// <param name="onFailure">Action to execute when failed.</param>
    public static void Switch(
        this Result result,
        Action onSuccess,
        Action<Error> onFailure)
    {
        if(result.IsSuccess)
            onSuccess();
        else
            onFailure(result.Error);
    }

    // Bind
    // Encadena otra operación que también puede fallar.
    /// <summary>Chains another void operation that may also fail.</summary>
    /// <param name="next">Function that returns a void <see cref="Result"/>.</param>
    /// <returns>The result of <paramref name="next"/> on success, or a failure.</returns>
    public static Result Bind(
        this Result result,
        Func<Result> next)
            => result.IsSuccess
                ? next()
                : Result.Failure(result.Error);

    // Bind <T>
    // Encadena otra operación que devuelve Result<T>.
    /// <summary>Chains a value-returning operation from a void result.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="next">Function that returns a <see cref="Result{T}"/>.</param>
    /// <returns>The result of <paramref name="next"/> on success, or a failure.</returns>
    public static Result<T> Bind<T>(
        this Result result,
        Func<Result<T>> next)
            => result.IsSuccess
                ? next()
                : Result.Failure<T>(result.Error);

    // OnSuccess
    // Ejecuta una accion lateral si el Result fue exitoso.
    /// <summary>Executes a side effect when the void result is successful. Returns the same result.</summary>
    /// <param name="action">Action to execute on success.</param>
    /// <returns>The original <see cref="Result"/> unchanged.</returns>
    public static Result OnSuccess(
        this Result result,
        Action action)
    {
        if(result.IsSuccess)
            action();

        return result;
    }

    // OnFailure
    // Ejecuta una accion lateral si el Result fallo.
    /// <summary>Executes a side effect when the void result is a failure. Returns the same result.</summary>
    /// <param name="action">Action to execute on the error.</param>
    /// <returns>The original <see cref="Result"/> unchanged.</returns>
    public static Result OnFailure(
        this Result result,
        Action<Error> action)
    {
        if(result.IsFailure)
            action(result.Error);

        return result;
    }

    // Ensure
    // Valida el Result sin salir del flujo.
    /// <summary>Validates the void result without breaking the chain.</summary>
    /// <param name="predicate">Validation predicate.</param>
    /// <param name="error">Error to return if validation fails.</param>
    /// <returns>The original result if valid, or a new failure with <paramref name="error"/>.</returns>
    public static Result Ensure(
        this Result result,
        Func<bool> predicate,
        Error error)
    {
        if(result.IsFailure)
            return result;

        return predicate()
            ? result
            : Result.Failure(error);
    }
    #endregion
}
