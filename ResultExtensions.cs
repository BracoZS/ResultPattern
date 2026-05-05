namespace ResultPattern;

public static class ResultExtensions
{
    #region Result<T> 
    // Match<T, TOut>
    // Convierte el resultado en un valor final, cierra el flujo.
    public static TOut Match<T, TOut>(
        this Result<T> result,
        Func<T, TOut> onSuccess,
        Func<Error, TOut> onFailure)
        => result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);

    // Match<T>
    // Ejecuta una accion segun el estado del resultado.
    public static void Match<T>(
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
    public static Result<TOut> Map<T, TOut>(
        this Result<T> result,
        Func<T, TOut> mapper)
        => result.IsSuccess
            ? Result.Ok(mapper(result.Value))
            : Result.Failure<TOut>(result.Error);

    // Bind<T, TOut>
    // Encadena otra operacion que tambien puede fallar.
    public static Result<TOut> Bind<T, TOut>(
        this Result<T> result,
        Func<T, Result<TOut>> next)
        => result.IsSuccess
            ? next(result.Value)
            : Result.Failure<TOut>(result.Error);

    // Bind<T>
    // Result<T> -> Result
    public static Result Bind<T>(
        this Result<T> result,
        Func<T, Result> next)
        => result.IsSuccess
            ? next(result.Value)
            : Result.Failure(result.Error);

    // Tap
    // Ejecuta una accion lateral si el resultado fue exitoso.
    public static Result<T> Tap<T>(
        this Result<T> result,
        Action<T> action)
    {
        if(result.IsSuccess)
            action(result.Value);

        return result;
    }

    // TapFailure
    // Ejecuta una accion lateral si el resultado fallo.
    public static Result<T> TapFailure<T>(
        this Result<T> result,
        Action<Error> action)
    {
        if(result.IsFailure)
            action(result.Error);

        return result;
    }

    // Ensure
    // Valida el valor exitoso sin salir del flujo.
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
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
            => result.IsSuccess
                ? onSuccess()
                : onFailure(result.Error);

    // Match
    // Ejecuta una accion segun el estado de un Result<Unit>.
    public static void Match(
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
    // 
    public static Result Bind(
        this Result result,
        Func<Result> next)
            => result.IsSuccess
                ? next()
                : Result.Failure(result.Error);

    // Bind <T>
    // 
    public static Result<T> Bind<T>(
        this Result result,
        Func<Result<T>> next)
            => result.IsSuccess
                ? next()
                : Result.Failure<T>(result.Error);

    // Tap
    // Ejecuta una accion lateral si el Result fue exitoso.
    public static Result Tap(
        this Result result,
        Action action)
    {
        if(result.IsSuccess)
            action();

        return result;
    }

    // TapFailure
    // Ejecuta una accion lateral si el Result fallo.
    public static Result TapFailure(
        this Result result,
        Action<Error> action)
    {
        if(result.IsFailure)
            action(result.Error);

        return result;
    }

    // Ensure
    // Valida el Result sin salir del flujo.
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
