namespace ResultPattern.Tests;

public class ResultTests
{
    [Fact]
    public void Ok_Should_Create_Success()
    {
        var result = Result.Ok();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_Should_Create_Failure()
    {
        var error = Error.Internal("algo fallo");
        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Implicit_Conversion_From_Error()
    {
        Result result = Error.Conflict("Order", "duplicado");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToString_Should_Indicate_Success()
    {
        var result = Result.Ok();

        Assert.Equal("Result(Success)", result.ToString());
    }

    [Fact]
    public void ToString_Should_Indicate_Failure()
    {
        var result = Result.Failure(Error.General());

        Assert.Contains("Failure", result.ToString());
    }
}
