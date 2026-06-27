namespace ResultPattern.Tests;

public class ResultTryTests
{
    [Fact]
    public void Try_Should_Return_Success_When_Function_Succeeds()
    {
        var result = Result.Try(() => 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Try_Should_Return_Failure_When_Function_Throws()
    {
        var result = Result.Try<int>(() => throw new InvalidOperationException("algo fallo"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error.Type);
    }

    [Fact]
    public void Try_Should_Use_Custom_Error_Map()
    {
        var result = Result.Try<int>(
            () => throw new InvalidOperationException("bad"),
            ex => Error.Validation("Custom", ex.Message));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal("Validation.Custom", result.Error.Code);
        Assert.Equal("bad", result.Error.Message);
    }

    [Fact]
    public void Try_Should_Throw_ArgumentNullException_When_Func_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => Result.Try<int>(null!));
    }

    [Fact]
    public void Try_Should_Rethrow_OperationCanceledException()
    {
        Assert.Throws<OperationCanceledException>(
            () => Result.Try<int>(() => throw new OperationCanceledException()));
    }

    [Fact]
    public void Try_Action_Should_Return_Success_When_Action_Succeeds()
    {
        var sideEffect = 0;
        var result = Result.Try(() => sideEffect = 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, sideEffect);
    }

    [Fact]
    public void Try_Action_Should_Return_Failure_When_Action_Throws()
    {
        var result = Result.Try(() => throw new InvalidOperationException("fallo"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error.Type);
    }

    [Fact]
    public void Try_Action_Should_Use_Custom_Error_Map()
    {
        var result = Result.Try(
            () => throw new InvalidOperationException("bad"),
            ex => Error.Conflict("Test", ex.Message));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task TryAsync_Should_Return_Success_When_Function_Succeeds()
    {
        var result = await Result.TryAsync(() => Task.FromResult(42));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TryAsync_Should_Return_Failure_When_Function_Throws()
    {
        var result = await Result.TryAsync<int>(() => throw new InvalidOperationException("fallo async"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error.Type);
    }

    [Fact]
    public async Task TryAsync_Should_Use_Custom_Error_Map()
    {
        var result = await Result.TryAsync<int>(
            () => throw new InvalidOperationException("bad"),
            ex => Error.NotFound("Async", ex.Message));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task TryAsync_Should_Rethrow_OperationCanceledException()
    {
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => Result.TryAsync<int>(() => throw new OperationCanceledException()));
    }

    [Fact]
    public async Task TryAsync_Action_Should_Return_Success_When_Action_Succeeds()
    {
        var sideEffect = 0;
        var result = await Result.TryAsync(() => { sideEffect = 42; return Task.CompletedTask; });

        Assert.True(result.IsSuccess);
        Assert.Equal(42, sideEffect);
    }

    [Fact]
    public async Task TryAsync_Action_Should_Return_Failure_When_Action_Throws()
    {
        var result = await Result.TryAsync(() => throw new InvalidOperationException("fallo async action"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error.Type);
    }

    [Fact]
    public async Task TryAsync_Action_Should_Use_Custom_Error_Map()
    {
        var result = await Result.TryAsync(
            () => throw new InvalidOperationException("bad"),
            ex => Error.Internal(ex.Message));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Internal, result.Error.Type);
    }
}
