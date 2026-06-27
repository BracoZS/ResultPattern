namespace ResultPattern.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void Match_Should_Apply_OnSuccess()
    {
        var result = Result.Ok(10);

        var output = result.Match(v => v * 2, e => 0);

        Assert.Equal(20, output);
    }

    [Fact]
    public void Match_Should_Apply_OnFailure()
    {
        var result = Result.Failure<int>(Error.General());

        var output = result.Match(v => v * 2, e => -1);

        Assert.Equal(-1, output);
    }

    [Fact]
    public void Map_Should_Transform_Value_On_Success()
    {
        var result = Result.Ok(3).Map(x => x.ToString());

        Assert.True(result.IsSuccess);
        Assert.Equal("3", result.Value);
    }

    [Fact]
    public void Map_Should_Propagate_Error_On_Failure()
    {
        var error = Error.Validation("x", "bad");
        var result = Result.Failure<int>(error).Map(x => x.ToString());

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Bind_Should_Chain_On_Success()
    {
        var result = Result.Ok(5)
            .Bind(x => Result.Ok(x * 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    [Fact]
    public void Bind_Should_Propagate_Error()
    {
        var error = Error.NotFound("Item", 1);
        var result = Result.Failure<int>(error)
            .Bind(x => Result.Ok(x * 2));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Bind_Result_Should_Chain()
    {
        var result = Result.Ok(5)
            .Bind<int>(x => Result.Ok());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Ensure_Should_Fail_If_Predicate_Fails()
    {
        var result = Result.Ok(0)
            .Ensure(x => x > 0, Error.Validation("x", "debe ser positivo"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Ensure_Should_Pass_If_Predicate_Succeeds()
    {
        var result = Result.Ok(5)
            .Ensure(x => x > 0, Error.Validation("x", "debe ser positivo"));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void Ensure_Should_Skip_If_Already_Failed()
    {
        var original = Error.NotFound("x", 1);
        var result = Result.Failure<int>(original)
            .Ensure(x => false, Error.Validation("y", "nunca se evalua"));

        Assert.True(result.IsFailure);
        Assert.Equal(original, result.Error);
    }

    [Fact]
    public void OnSuccess_Should_Execute_Action_On_Success()
    {
        var sideEffect = 0;
        var result = Result.Ok(5).OnSuccess(x => sideEffect = x);

        Assert.Equal(5, sideEffect);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void OnSuccess_Should_Not_Execute_On_Failure()
    {
        var sideEffect = 0;
        var result = Result.Failure<int>(Error.General())
            .OnSuccess(x => sideEffect = 1);

        Assert.Equal(0, sideEffect);
    }

    [Fact]
    public void OnFailure_Should_Execute_On_Failure()
    {
        Error? captured = null;
        var error = Error.Validation("x", "bad");
        var result = Result.Failure<int>(error).OnFailure(e => captured = e);

        Assert.Equal(error, captured);
    }

    [Fact]
    public void OnFailure_Should_Not_Execute_On_Success()
    {
        var sideEffect = 0;
        var result = Result.Ok(5).OnFailure(e => sideEffect = 1);

        Assert.Equal(0, sideEffect);
    }

    [Fact]
    public void Switch_Should_Execute_OnSuccess_On_Success()
    {
        var sideEffect = 0;

        Result.Ok(5).Switch(
            x => sideEffect = x,
            _ => sideEffect = -1);

        Assert.Equal(5, sideEffect);
    }

    [Fact]
    public void Switch_Should_Execute_OnFailure_On_Failure()
    {
        var sideEffect = 0;
        var error = Error.Validation("x", "bad");

        Result.Failure<int>(error).Switch(
            _ => sideEffect = -1,
            e => sideEffect = 1);

        Assert.Equal(1, sideEffect);
    }

    [Fact]
    public void Result_Switch_Should_Execute_OnSuccess()
    {
        var sideEffect = 0;

        Result.Ok().Switch(
            () => sideEffect = 1,
            _ => sideEffect = -1);

        Assert.Equal(1, sideEffect);
    }

    [Fact]
    public void Result_Switch_Should_Execute_OnFailure()
    {
        var sideEffect = 0;

        Result.Failure(Error.General()).Switch(
            () => sideEffect = -1,
            _ => sideEffect = 1);

        Assert.Equal(1, sideEffect);
    }

    [Fact]
    public void Result_Match_Should_Apply_OnSuccess()
    {
        var result = Result.Ok();

        var output = result.Match(() => "ok", e => "fail");

        Assert.Equal("ok", output);
    }

    [Fact]
    public void Result_Bind_Should_Chain()
    {
        var result = Result.Ok()
            .Bind(() => Result.Ok(42));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Result_Ensure_Should_Fail_If_Predicate_Fails()
    {
        var result = Result.Ok()
            .Ensure(() => false, Error.Validation("x", "bad"));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Full_Pipeline_Should_Work()
    {
        var result = Result.Ok(5)
            .Ensure(x => x > 0, Error.Validation("x", "negativo"))
            .Map(x => x * 2)
            .Bind(x => Result.Ok(x + 1))
            .OnSuccess(x => { /* side effect */ })
            .OnFailure(e => { /* side effect */ });

        Assert.True(result.IsSuccess);
        Assert.Equal(11, result.Value);
    }
}
