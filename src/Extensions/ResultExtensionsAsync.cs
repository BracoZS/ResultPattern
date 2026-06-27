namespace ResultPattern;

public static class ResultExtensionsAsync
{
    #region Result<T>
    // MatchAsync<T, TOut>
    // Espera el resultado y lo convierte en un valor final.
    /// <summary>Awaits the task and converts the result into a single value.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="onSuccess">Function to apply when successful.</param>
    /// <param name="onFailure">Function to apply when failed.</param>
    /// <returns>The result of <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    public static async Task<TOut> MatchAsync<T, TOut>(
        this Task<Result<T>> task,
        Func<T, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        var result = await task;

        return result.Match(onSuccess, onFailure);
    }

    // SwitchAsync<T>
    // Espera el resultado y ejecuta una accion segun su estado. Terminal.
    /// <summary>Awaits the result and executes an action based on its state. Terminal operation.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="onSuccess">Action to execute when successful.</param>
    /// <param name="onFailure">Action to execute when failed.</param>
    public static async Task SwitchAsync<T>(
        this Task<Result<T>> task,
        Action<T> onSuccess,
        Action<Error> onFailure)
    {
        var result = await task;

        result.Switch(onSuccess, onFailure);
    }

    // MapAsync
    // Espera el resultado y transforma su valor si fue exitoso.
    /// <summary>Awaits the result and transforms its value if successful.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="mapper">Function to transform the value.</param>
    /// <returns>A new <see cref="Result{T}"/> with the transformed value, or the original error.</returns>
    public static async Task<Result<TOut>> MapAsync<T, TOut>(
        this Task<Result<T>> task,
        Func<T, TOut> mapper)
    {
        var result = await task;

        return result.Map(mapper);
    }

    // BindAsync<T, TOut>
    // Espera el resultado y encadena otra operacion async.
    /// <summary>Awaits the result and chains an async operation that may also fail.</summary>
    /// <typeparam name="T">The input value type.</typeparam>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="next">Async function that returns a new <see cref="Result{T}"/>.</param>
    /// <returns>The result of <paramref name="next"/> on success, or the original error.</returns>
    public static async Task<Result<TOut>> BindAsync<T, TOut>(
        this Task<Result<T>> task,
        Func<T, Task<Result<TOut>>> next)
    {
        var result = await task;

        return result.IsSuccess
            ? await next(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

    // BindAsync<T>
    // Result<T> -> Result
    /// <summary>Awaits the result and chains an async operation returning a void <see cref="Result"/>.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="next">Async function returning a void <see cref="Result"/>.</param>
    /// <returns>The result of <paramref name="next"/> on success, or a void failure.</returns>
    public static async Task<Result> BindAsync<T>(
        this Task<Result<T>> task,
        Func<T, Task<Result>> next)
    {
        var result = await task;
        return result.IsSuccess
            ? await next(result.Value)
            : Result.Failure(result.Error);
    }

    // OnSuccessAsync
    // Espera el resultado y ejecuta una accion lateral si fue exitoso.
    /// <summary>Awaits the result and executes a side effect if successful. Returns the same result.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="action">Action to execute on the success value.</param>
    /// <returns>The original <see cref="Result{T}"/> unchanged.</returns>
    public static async Task<Result<T>> OnSuccessAsync<T>(
        this Task<Result<T>> task,
        Action<T> action)
    {
        var result = await task;

        return result.OnSuccess(action);
    }

    // OnFailureAsync
    // Espera el resultado y ejecuta una accion lateral si fallo.
    /// <summary>Awaits the result and executes a side effect if failed. Returns the same result.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="action">Action to execute on the error.</param>
    /// <returns>The original <see cref="Result{T}"/> unchanged.</returns>
    public static async Task<Result<T>> OnFailureAsync<T>(
        this Task<Result<T>> task,
        Action<Error> action)
    {
        var result = await task;

        return result.OnFailure(action);
    }

    // EnsureAsync
    // Espera el resultado y valida el valor exitoso.
    /// <summary>Awaits the result and validates the success value without breaking the chain.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="predicate">Validation predicate on the success value.</param>
    /// <param name="error">Error to return if validation fails.</param>
    /// <returns>The original result if valid, or a new failure with <paramref name="error"/>.</returns>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Task<Result<T>> task,
        Func<T, bool> predicate,
        Error error)
    {
        var result = await task;

        return result.Ensure(predicate, error);
    }
    #endregion

    #region Result
    // MatchAsync<TOut>
    // Espera un Result<Unit> y lo convierte en un valor final.
    /// <summary>Awaits a void result and converts it into a single value.</summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="onSuccess">Function to call when successful.</param>
    /// <param name="onFailure">Function to call when failed.</param>
    /// <returns>The result of <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
    public static async Task<TOut> MatchAsync<TOut>(
        this Task<Result> task,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        var result = await task;

        return result.Match(onSuccess, onFailure);
    }

    // SwitchAsync
    // Espera un Result y ejecuta una accion segun su estado. Terminal.
    /// <summary>Awaits a void result and executes an action based on its state. Terminal.</summary>
    /// <param name="onSuccess">Action to execute when successful.</param>
    /// <param name="onFailure">Action to execute when failed.</param>
    public static async Task SwitchAsync(
        this Task<Result> task,
        Action onSuccess,
        Action<Error> onFailure)
    {
        var result = await task;

        result.Switch(onSuccess, onFailure);
    }

    // BindAsync
    // Espera un Result y encadena otra operación async.
    /// <summary>Awaits a void result and chains an async void operation.</summary>
    /// <param name="next">Async function returning a void <see cref="Result"/>.</param>
    /// <returns>The result of <paramref name="next"/> on success, or a failure.</returns>
    public static async Task<Result> BindAsync(
        this Task<Result> task,
        Func<Task<Result>> next)
    {
        var result = await task;

        return result.IsSuccess
            ? await next()
            : Result.Failure(result.Error);
    }

    // BindAsync<T>
    // Espera un Result y encadena otra operación async que devuelve Result<T>.
    /// <summary>Awaits a void result and chains an async value operation.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="next">Async function returning a <see cref="Result{T}"/>.</param>
    /// <returns>The result of <paramref name="next"/> on success, or a failure.</returns>
    public static async Task<Result<T>> BindAsync<T>(
        this Task<Result> task,
        Func<Task<Result<T>>> next)
    {
        var result = await task;

        return result.IsSuccess
            ? await next()
            : Result.Failure<T>(result.Error);
    }

    // OnSuccessAsync
    /// <summary>Awaits a void result and executes a side effect if successful.</summary>
    /// <param name="action">Action to execute on success.</param>
    /// <returns>The original <see cref="Result"/> unchanged.</returns>
    public static async Task<Result> OnSuccessAsync(
        this Task<Result> task,
        Action action)
    {
        var result = await task;

        return result.OnSuccess(action);
    }

    // OnFailureAsync
    /// <summary>Awaits a void result and executes a side effect if failed.</summary>
    /// <param name="action">Action to execute on the error.</param>
    /// <returns>The original <see cref="Result"/> unchanged.</returns>
    public static async Task<Result> OnFailureAsync(
        this Task<Result> task,
        Action<Error> action)
    {
        var result = await task;

        return result.OnFailure(action);
    }
    #endregion
}
