namespace ResultPattern.Tests;

public class ResultOfTTests
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
        var error = Error.Validation("User.Email", "El email no es valido.");

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("Validation.User.Email", error.Code);
        Assert.Equal("El email no es valido.", error.Message);
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
        Assert.DoesNotContain("identificador", error.Message);
    }

    [Fact]
    public void Conflict_Should_Set_Properties()
    {
        var error = Error.Conflict("Order", "La orden ya esta cerrada.");

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.Equal("Conflict.Order", error.Code);
        Assert.Equal("La orden ya esta cerrada.", error.Message);
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
        var error = Error.Internal("no se pudo leer config");

        Assert.Equal(ErrorType.Internal, error.Type);
        Assert.Equal("Internal.General", error.Code);
        Assert.Equal("no se pudo leer config", error.Message);
    }

    [Fact]
    public void Internal_Without_Message_Should_Use_Default()
    {
        var error = Error.Internal();

        Assert.Equal("Ocurrio un error interno.", error.Message);
    }

    [Fact]
    public void General_Should_Set_Properties()
    {
        var error = Error.General("algo inesperado");

        Assert.Equal(ErrorType.General, error.Type);
        Assert.Equal("General.Custom", error.Code);
        Assert.Equal("algo inesperado", error.Message);
    }

    [Fact]
    public void General_Without_Message_Should_Use_Default()
    {
        var error = Error.General();

        Assert.Equal("Ha ocurrido un error inesperado.", error.Message);
    }
}

public class ResultExtensionsSyncTests
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
