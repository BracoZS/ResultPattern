namespace ResultPattern.Tests;

public class ResultExtensionsAsyncTests
{
    [Fact]
    public async Task MatchAsync_Should_Apply_OnSuccess()
    {
        var result = await Task.FromResult(Result.Ok(10))
            .MatchAsync(v => v * 2, e => 0);

        Assert.Equal(20, result);
    }

    [Fact]
    public async Task MatchAsync_Should_Apply_OnFailure()
    {
        var result = await Task.FromResult(Result.Failure<int>(Error.General()))
            .MatchAsync(v => v * 2, e => -1);

        Assert.Equal(-1, result);
    }

    [Fact]
    public async Task MapAsync_Should_Transform_On_Success()
    {
        var result = await Task.FromResult(Result.Ok(3))
            .MapAsync(x => x.ToString());

        Assert.True(result.IsSuccess);
        Assert.Equal("3", result.Value);
    }

    [Fact]
    public async Task MapAsync_Should_Propagate_Error()
    {
        var error = Error.Validation("x", "bad");
        var result = await Task.FromResult(Result.Failure<int>(error))
            .MapAsync(x => x.ToString());

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task BindAsync_Should_Chain_On_Success()
    {
        var result = await Task.FromResult(Result.Ok(5))
            .BindAsync(x => Task.FromResult(Result.Ok(x * 2)));

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public async Task BindAsync_Should_Propagate_Error()
    {
        var error = Error.NotFound("Item", 1);
        var result = await Task.FromResult(Result.Failure<int>(error))
            .BindAsync(x => Task.FromResult(Result.Ok(x * 2)));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task EnsureAsync_Should_Fail_If_Predicate_Fails()
    {
        var result = await Task.FromResult(Result.Ok(0))
            .EnsureAsync(x => x > 0, Error.Validation("x", "debe ser positivo"));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task EnsureAsync_Should_Pass_If_Predicate_Succeeds()
    {
        var result = await Task.FromResult(Result.Ok(5))
            .EnsureAsync(x => x > 0, Error.Validation("x", "debe ser positivo"));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task OnSuccessAsync_Should_Execute_On_Success()
    {
        var sideEffect = 0;

        var result = await Task.FromResult(Result.Ok(5))
            .OnSuccessAsync(x => sideEffect = x);

        Assert.Equal(5, sideEffect);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OnSuccessAsync_Should_Not_Execute_On_Failure()
    {
        var sideEffect = 0;

        var result = await Task.FromResult(Result.Failure<int>(Error.General()))
            .OnSuccessAsync(x => sideEffect = 1);

        Assert.Equal(0, sideEffect);
    }

    [Fact]
    public async Task OnFailureAsync_Should_Execute_On_Failure()
    {
        Error? captured = null;
        var error = Error.Validation("x", "bad");

        var result = await Task.FromResult(Result.Failure<int>(error))
            .OnFailureAsync(e => captured = e);

        Assert.Equal(error, captured);
    }

    [Fact]
    public async Task OnFailureAsync_Should_Not_Execute_On_Success()
    {
        var sideEffect = 0;

        var result = await Task.FromResult(Result.Ok(5))
            .OnFailureAsync(e => sideEffect = 1);

        Assert.Equal(0, sideEffect);
    }

    [Fact]
    public async Task Full_Async_Pipeline_Should_Work()
    {
        var result = await Task.FromResult(Result.Ok(5))
            .EnsureAsync(x => x > 0, Error.Validation("x", "negativo"))
            .MapAsync(x => x * 2)
            .BindAsync(x => Task.FromResult(Result.Ok(x + 1)))
            .OnSuccessAsync(x => { /* side effect */ })
            .OnFailureAsync(e => { /* side effect */ });

        Assert.True(result.IsSuccess);
        Assert.Equal(11, result.Value);
    }

    [Fact]
    public async Task Async_Pipeline_Should_Stop_On_First_Error()
    {
        var sideEffect = 0;

        var result = await Task.FromResult(Result.Ok(5))
            .EnsureAsync(x => false, Error.Validation("x", "fallo"))
            .OnSuccessAsync(x => sideEffect = 1);

        Assert.True(result.IsFailure);
        Assert.Equal(0, sideEffect);
    }

    [Fact]
    public async Task SwitchAsync_Should_Execute_OnSuccess()
    {
        var sideEffect = 0;

        await Task.FromResult(Result.Ok(5)).SwitchAsync(
            x => sideEffect = x,
            _ => sideEffect = -1);

        Assert.Equal(5, sideEffect);
    }

    [Fact]
    public async Task SwitchAsync_Should_Execute_OnFailure()
    {
        var sideEffect = 0;
        var error = Error.Validation("x", "bad");

        await Task.FromResult(Result.Failure<int>(error)).SwitchAsync(
            _ => sideEffect = -1,
            e => sideEffect = 1);

        Assert.Equal(1, sideEffect);
    }

    [Fact]
    public async Task Result_SwitchAsync_Should_Execute_OnSuccess()
    {
        var sideEffect = 0;

        await Task.FromResult(Result.Ok()).SwitchAsync(
            () => sideEffect = 1,
            _ => sideEffect = -1);

        Assert.Equal(1, sideEffect);
    }

    [Fact]
    public async Task Result_SwitchAsync_Should_Execute_OnFailure()
    {
        var sideEffect = 0;

        await Task.FromResult(Result.Failure(Error.General())).SwitchAsync(
            () => sideEffect = -1,
            _ => sideEffect = 1);

        Assert.Equal(1, sideEffect);
    }

    [Fact]
    public async Task Result_MatchAsync_Should_Work()
    {
        var result = await Task.FromResult(Result.Ok())
            .MatchAsync(() => "ok", e => "fail");

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Result_BindAsync_Should_Chain()
    {
        var result = await Task.FromResult(Result.Ok())
            .BindAsync(() => Task.FromResult(Result.Ok(42)));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }
}
