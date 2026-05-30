using System;
using System.Threading.Tasks;

namespace ResultPattern;

public static class ResultExtensionsAsync
{
    #region Result<T>
    // MatchAsync<T, TOut>
    // Espera el resultado y lo convierte en un valor final.
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
    public static async Task<Result<TOut>> MapAsync<T, TOut>(
        this Task<Result<T>> task,
        Func<T, TOut> mapper)
    {
        var result = await task;

        return result.Map(mapper);
    }

    // BindAsync<T, TOut>
    // Espera el resultado y encadena otra operacion async.
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
    public static async Task<Result<T>> OnSuccessAsync<T>(
        this Task<Result<T>> task,
        Action<T> action)
    {
        var result = await task;

        return result.OnSuccess(action);
    }

    // OnFailureAsync
    // Espera el resultado y ejecuta una accion lateral si fallo.
    public static async Task<Result<T>> OnFailureAsync<T>(
        this Task<Result<T>> task,
        Action<Error> action)
    {
        var result = await task;

        return result.OnFailure(action);
    }

    // EnsureAsync
    // Espera el resultado y valida el valor exitoso.
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
    public static async Task SwitchAsync(
        this Task<Result> task,
        Action onSuccess,
        Action<Error> onFailure)
    {
        var result = await task;

        result.Switch(onSuccess, onFailure);
    }

    // BindAsync
    /// <summary>
    /// Chains an asynchronous operation to be executed if the preceding asynchronous result is successful.
    /// </summary>
    /// <remarks>This method enables fluent composition of asynchronous operations that return a Result.
    /// If the initial task fails, the next operation is not executed and the failure is propagated.
    /// </remarks>
    /// <param name="task">The initial asynchronous operation whose result determines whether the next operation is invoked.</param>
    /// <param name="next">A function that returns the next asynchronous operation to execute if the initial result is successful.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The result contains the outcome of the next operation if the
    /// initial result is successful; otherwise, it contains the failure from the initial result.
    /// </returns>
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
    //
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
    public static async Task<Result> OnSuccessAsync(
        this Task<Result> task,
        Action action)
    {
        var result = await task;

        return result.OnSuccess(action);
    }

    // OnFailureAsync
    public static async Task<Result> OnFailureAsync(
        this Task<Result> task,
        Action<Error> action)
    {
        var result = await task;

        return result.OnFailure(action);
    }
    #endregion
}
