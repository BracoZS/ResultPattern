namespace ResultPattern.Tests;

public class ResultTTests
{
    [Fact]
    public void Ok_Should_Create_Success_Result()
    {
        var result = Result.Ok(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_Should_Create_Failure_Result()
    {
        var error = Error.Validation("Test.Field", "Campo invalido");
        var result = Result<int>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Value_Should_Throw_On_Failure()
    {
        var result = Result.Failure<int>(Error.General());

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Implicit_Conversion_From_Value_Should_Create_Success()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Implicit_Conversion_From_Error_Should_Create_Failure()
    {
        Result<int> result = Error.NotFound("User", 1);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFound.User", result.Error.Code);
    }

    [Fact]
    public void Implicit_Conversion_To_Result_Should_Preserve_State()
    {
        Result<int> success = 42;
        Result<int> failure = Error.General();

        Result successResult = success;
        Result failureResult = failure;

        Assert.True(successResult.IsSuccess);
        Assert.True(failureResult.IsFailure);
    }

    [Fact]
    public void ToString_Should_Indicate_Success()
    {
        var result = Result.Ok("hello");

        Assert.Equal("Result(Success: hello)", result.ToString());
    }

    [Fact]
    public void ToString_Should_Indicate_Failure()
    {
        var result = Result.Failure<string>(Error.Validation("x", "bad"));

        Assert.Contains("Failure", result.ToString());
    }
}
