namespace ResultPattern.Tests;

public class ErrorTests
{
    [Fact]
    public void None_Should_Have_None_Type()
    {
        Assert.Equal(ErrorType.None, Error.None.Type);
        Assert.Equal("", Error.None.Code);
    }

    [Fact]
    public void Validation_Should_Set_Properties()
    {
        var error = Error.Validation("User.Email", "The email is not valid.");

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("Validation.User.Email", error.Code);
        Assert.Equal("The email is not valid.", error.Message);
    }

    [Fact]
    public void NotFound_With_Key_Should_Include_Key_In_Message()
    {
        var error = Error.NotFound("User", 42);

        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.Equal("NotFound.User", error.Code);
        Assert.Contains("42", error.Message);
    }

    [Fact]
    public void NotFound_Without_Key_Should_Not_Include_Key()
    {
        var error = Error.NotFound("Product");

        Assert.Equal("NotFound.Product", error.Code);
        Assert.DoesNotContain("identifier", error.Message);
    }

    [Fact]
    public void Conflict_Should_Set_Properties()
    {
        var error = Error.Conflict("Order", "The order is already closed.");

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("Conflict.Order", error.Code);
        Assert.Equal("The order is already closed.", error.Message);
    }

    [Fact]
    public void Unauthorized_Should_Have_Fixed_Values()
    {
        Assert.Equal(ErrorType.Unauthorized, Error.Unauthorized.Type);
        Assert.Equal("Auth.Unauthorized", Error.Unauthorized.Code);
    }

    [Fact]
    public void Internal_Should_Set_Properties()
    {
        var error = Error.Internal("could not read config");

        Assert.Equal(ErrorType.Internal, error.Type);
        Assert.Equal("Internal.General", error.Code);
        Assert.Equal("could not read config", error.Message);
    }

    [Fact]
    public void Internal_Without_Message_Should_Use_Default()
    {
        var error = Error.Internal();

        Assert.Equal("An internal error occurred.", error.Message);
    }

    [Fact]
    public void General_Should_Set_Properties()
    {
        var error = Error.General("something unexpected");

        Assert.Equal(ErrorType.General, error.Type);
        Assert.Equal("General.Custom", error.Code);
        Assert.Equal("something unexpected", error.Message);
    }

    [Fact]
    public void General_Without_Message_Should_Use_Default()
    {
        var error = Error.General();

        Assert.Equal("An unexpected error has occurred.", error.Message);
    }
}
