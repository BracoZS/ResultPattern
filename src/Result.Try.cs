namespace ResultPattern;

public partial class Result
{
    /// <summary>
    /// Executes the specified function and wraps the result in a <see cref="Result{T}"/>,
    /// catching any non-fatal exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="func">The function to execute. Must not be <see langword="null"/>.</param>
    /// <param name="map">
    /// An optional delegate that maps a caught <see cref="Exception"/> to an <see cref="Error"/>.
    /// When <see langword="null"/>, <see cref="Error.Internal"/> with the exception string is used.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the function result, or a failed result
    /// containing the mapped error.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Re-thrown without wrapping when the operation is canceled.</exception>
    public static Result<T> Try<T>(Func<T> func, Func<Exception, Error>? map = null)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return Ok(func());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Failure<T>(map?.Invoke(ex) ?? Error.Internal(ex.ToString()));
        }
    }

    /// <summary>
    /// Executes the specified action and wraps the outcome in a <see cref="Result"/>,
    /// catching any non-fatal exceptions.
    /// </summary>
    /// <param name="action">The action to execute. Must not be <see langword="null"/>.</param>
    /// <param name="map">
    /// An optional delegate that maps a caught <see cref="Exception"/> to an <see cref="Error"/>.
    /// When <see langword="null"/>, <see cref="Error.Internal"/> with the exception string is used.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result"/> if the action completes, or a failed result
    /// containing the mapped error.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Re-thrown without wrapping when the operation is canceled.</exception>
    public static Result Try(Action action, Func<Exception, Error>? map = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action();
            return Ok();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Failure(map?.Invoke(ex) ?? Error.Internal(ex.ToString()));
        }
    }

    /// <summary>
    /// Executes the specified async function and wraps the result in a <see cref="Result{T}"/>,
    /// catching any non-fatal exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the return value.</typeparam>
    /// <param name="func">The async function to execute. Must not be <see langword="null"/>.</param>
    /// <param name="map">
    /// An optional delegate that maps a caught <see cref="Exception"/> to an <see cref="Error"/>.
    /// When <see langword="null"/>, <see cref="Error.Internal"/> with the exception string is used.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the function result, or a failed result
    /// containing the mapped error.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="func"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Re-thrown without wrapping when the operation is canceled.</exception>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func, Func<Exception, Error>? map = null)
    {
        ArgumentNullException.ThrowIfNull(func);

        try
        {
            return Ok(await func().ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Failure<T>(map?.Invoke(ex) ?? Error.Internal(ex.ToString()));
        }
    }

    /// <summary>
    /// Executes the specified async action and wraps the outcome in a <see cref="Result"/>,
    /// catching any non-fatal exceptions.
    /// </summary>
    /// <param name="action">The async action to execute. Must not be <see langword="null"/>.</param>
    /// <param name="map">
    /// An optional delegate that maps a caught <see cref="Exception"/> to an <see cref="Error"/>.
    /// When <see langword="null"/>, <see cref="Error.Internal"/> with the exception string is used.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result"/> if the action completes, or a failed result
    /// containing the mapped error.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <see langword="null"/>.</exception>
    /// <exception cref="OperationCanceledException">Re-thrown without wrapping when the operation is canceled.</exception>
    public static async Task<Result> TryAsync(Func<Task> action, Func<Exception, Error>? map = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            await action().ConfigureAwait(false);
            return Ok();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Failure(map?.Invoke(ex) ?? Error.Internal(ex.ToString()));
        }
    }
}
