# Result Pattern

[![build](https://github.com/BracoZS/ResultPattern/actions/workflows/build.yml/badge.svg)](https://github.com/BracoZS/ResultPattern/actions/workflows/build.yml)

`Result<T>` represents the outcome of an operation: `success` with a value or `failure` with an error.

It is used to model failures that belong to the normal flow of an application:

* Failures that are part of the application's normal flow.
* Validation.
* Resources not found.
* Data reading.
* Conflicts.
* Permissions.
* Persistence.
* Invalid responses or expected errors from external services.
* Flows where each step depends on the previous one.

It is useful in situations where an operation can fail normally or when failure is part of the expected behavior.

It does not replace exceptions, it complements them.

### Why the Result Pattern?

The `Result` pattern provides a clear and explicit way to manage success and failure without relying on exceptions for control flow.

Instead of throwing exceptions or returning `null`, methods return a structured result that makes outcomes predictable and easier to handle.

❌ Problems it solves

Traditional approaches often lead to:

- Hidden control flow through exceptions
- `null` values and potential `NullReferenceException`s
- Deeply nested `if` statements
- Inconsistent error handling across the application

✔️ Benefits

With `Result<T>` you get:

- Explicit success/failure flow
- Non-exception-based control flow
- Safer and more predictable APIs
- Easier composition of operations
- Improved testability

## Installation

Package: https://www.nuget.org/packages/ResultPattern.Net

```bash
nuget install ResultPattern.Net
```

```bash
dotnet add package ResultPattern.Net
```


## `Result` type

The fundamental usage of `Result` is by explicitly returning either **success** or **failure**.

`Result` (without a value) is used for operations where only the success or failure of the operation matters.

```csharp
public Result SaveUser(User user)
{
    if (user is null)
        return Result.Failure(Error.Validation("User.Required", "User is required"));

    _users.Save(user);

    return Result.Ok();
}
```

`Result<T>` is used when an operation needs to return a value.

```csharp
public Result<User> CreateUser(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result<User>.Failure(Error.Validation("User.NotFound", "User is required"));

    return Result<User>.Ok(new User(name));
}
```

> [!NOTE]
> The result can be returned either explicitly or implicitly through [implicit conversion](#implicit-conversion)

#### Properties

`Result<T>` exposes the following properties to inspect the result state:

- `IsSuccess`: Indicates whether the operation succeeded.
- `IsFailure`: Indicates whether the operation failed. This is the inverse of `IsSuccess`.
- `Error`: Contains the error associated with a failure. If the result is successful, it contains `Error.None`.
- `Value`: Contains the result value when the operation succeeds. If the result is a failure, accessing this property throws an exception.

Check `IsSuccess` or `IsFailure` when the flow uses _'early returns'_.

```csharp
Result<User> result = await GetUserAsync(id);

if (result.IsFailure)
{
    Console.WriteLine($"Error: {result.Error.Message}");
    return;
}

User user = result.Value;
Console.WriteLine($"User found: {user.Name}");
```

> [!WARNING]
> Value should only be accessed after confirming that the result is successful. Otherwise, an exception will be thrown. This is the expected behavior.


## `Error` type

`Error` describes an expected failure in a stable and structured way.

```csharp
public sealed record Error(ErrorType Type, string Code, string Message);
```

| Property |	Type |	Description
| ---------|-------|--------------------------|
| `Type` |	`ErrorType` |	Categorizes the error at a high level. Makes it easier to identify the nature of the failure.
|`Code` |	`string` |	A domain-specific error identifier. It should remain stable and be useful for tracing, logging, testing, and integrations.
|`Message` |	`string` |	A human-readable description of the problem. It may vary depending on the context.
<details> 
  <summary>View ErrorType enum</summary>

```csharp
public enum ErrorType
{
    // Success
    None,

    // Input / Permissions
    Validation,        // Invalid input data
    Unauthorized,      // Not authenticated / invalid credentials
    Forbidden,         // Insufficient permissions

    // State / Resources
    NotFound,          // Resource does not exist
    Conflict,          // Inconsistent state (e.g. duplicate, concurrency)
    InvalidOperation,  // Invalid state for performing the action
    NotSupported,      // Operation not supported in this context

    // Execution
    Timeout,           // Operation exceeded the time limit
    Cancelled,         // Operation was cancelled (cancellation token, user, etc.)

    // Other
    General,           // Unspecified or unknown error
    Internal,          // Internal technical error (bug, unhandled exception)
}
```
</details> 
<br>

> [!TIP]
> The Error type provides static factory methods for representing the most common system errors.
>
> You can use the built-in factories to create errors with a consistent format:

```csharp
Error.Validation("User.EmailInvalid", "The email address is invalid.");
Error.NotFound("User.NotFound", $"The user with id '{id}' does not exist.");
Error.Conflict("User.AlreadyExists", "The user is already registered.");
Error.Unauthorized("Auth.InvalidCredentials", "The credentials are invalid.");
Error.Internal("Failed to read configuration.");
Error.General();
```

### Implicit Conversion

`Result<T>` supports implicit conversions to reduce boilerplate and enable a more natural expression of the flow [[docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/user-defined-conversion-operators)].

This allows you to return either an `Error` or a value of type `T` directly, without manually wrapping them in a `Result`.

`Error → Result / Result<T>`

`T     → Result<T>`

This reduces boilerplate and makes failure paths more concise, improving readability in methods that return `Result<T>`.

```csharp
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _repo.FindAsync(id);

    return user is null
        ? Error.NotFound("User.NotFound", $"The user with id '{id}' does not exist")
        : user;
}
```

In the example above:

- `Error → Result<User>` through implicit conversion
- `User → Result<User>` through implicit conversion

## Extensions

Extensions are available in both synchronous and asynchronous versions.

The asynchronous version is where the pattern provides the most value, allowing you to chain validations, repositories, external services, and transformations without multiple nested if statements. It operates on `Task<Result<T>>` and is the most common choice in modern applications, where I/O operations and result composition are prevalent.


| Function | Purpose |
| --- | --- |
| [`Map` / `MapAsync`](#map--mapasync) | Transform |
| [`Bind` / `BindAsync`](#bind--bindasync) | Chain |
| [`Ensure` / `EnsureAsync`](#ensure--ensureasync) | Validate |
| [`OnSuccess` / `OnSuccessAsync`](#onsuccess--onsuccessasync) | Execute on success |
| [`OnFailure` / `OnFailureAsync`](#onfailure--onfailureasync) | Execute on failure |
| [`Match` / `MatchAsync`](#match--matchasync) | Close (returns a value) |
| [`Switch` / `SwitchAsync`](#switch--switchasync) | Close (executes an action) |

### Match / MatchAsync

Use `Match` or `MatchAsync` to return a value by handling both outcomes (success or failure). Define what should happen when the operation succeeds and what should happen when it fails.

Both delegates must return the same type.

```csharp
string message = await GetUserAsync(id)
    .MatchAsync(
        user => $"User: {user.Name}",
        error => $"Error: {error.Message}");
```
        
### Switch / SwitchAsync

Use `Switch` or `SwitchAsync` to terminate the flow and **execute an action**. It does not return a value.

```csharp
await GetUserAsync(id)
    .SwitchAsync(
        user => Console.WriteLine($"Current user: {user.Name}"),
        error => Console.WriteLine(error.Message));
```

### Map / MapAsync

`Map` and `MapAsync` are used to **transform** the value contained in a successful `Result<T>`.

If the result is a failure, the error is propagated without applying the transformation.

```csharp
// Async: User -> UserDto if GetUserAsync succeeds
Result<UserDto> asyncResult = await GetUserAsync(id)
    .MapAsync(user => new UserDto(user.Id, user.Name));

// Sync: synchronous version
Result<UserDto> syncResult = GetUser(id)
    .Map(user => new UserDto(user.Id, user.Name));
```

### Bind / BindAsync

`Bind` and `BindAsync` are used to **chain** multiple dependent operations. Use them when the next step in the chain also returns a `Result<T>` and can therefore fail. They allow complex flows to be expressed linearly, without nesting.

If any operation fails, the error is automatically propagated, returned, and execution stops.

```csharp
// Chaining with another operation that returns Result
Result<Profile> asyncResult = await GetUserAsync(id)
    .BindAsync(user => GetProfileAsync(user.Id));

// Multiple dependent operations
Result<Order> result = await ValidateRequestAsync(request)
    .BindAsync(validRequest => CreateOrderAsync(validRequest))
    .BindAsync(order => ReserveStockAsync(order))
    .BindAsync(order => SaveOrderAsync(order));
```

> The flow reads from top to bottom as a sequence of domain steps.

This approach avoids callback hell and deeply nested if statements, keeping the execution flow more declarative and easier to read.

### Ensure / EnsureAsync

Use `Ensure` or `EnsureAsync` to **validate** the value of a successful `Result` without leaving the flow.

```csharp
// Async
Result<User> asyncResult = await GetUserAsync(id)
    .EnsureAsync(
        user => user.IsActive,
        Error.Validation("User.State", "The user is not active."));

// Sync
Result<User> syncResult = GetUser(id)
    .Ensure(
        user => user.IsActive,
        Error.Validation("User.State", "The user is not active."));
```

If the result is already a failure, the validation is not executed and the original error is preserved.


### OnSuccess / OnSuccessAsync

Use `OnSuccess` or `OnSuccessAsync` to **execute** a side effect only when the `Result` is **successful**, **without modifying** the value or interrupting the flow.

They are useful for logging, metrics, caching, notifications, and similar scenarios.

```csharp
// Async
Result<User> asyncResult = await GetUserAsync(id)
    .OnSuccessAsync(user => Log($"User found: {user.Id}"));

// Sync
Result<User> syncResult = GetUser(id)
    .OnSuccess(user => Log($"User found: {user.Id}"));
```

> [!TIP]
> OnSuccess is useful when you need to observe the flow without affecting its outcome.

### OnFailure / OnFailureAsync

Use `OnFailure` or `OnFailureAsync` to **execute** a side effect (logging, metrics, alerts, etc.) only **when the operation fails**, **without modifying** the result or interrupting the flow.

```csharp
// Async
Result<User> asyncResult = await GetUserAsync(id)
    .OnFailureAsync(error => Log($"Error: {error.Message}"));

// Sync
Result<User> syncResult = GetUser(id)
    .OnFailure(error => Log($"Error: {error.Message}"));
```

It does not transform the error; it only observes the failure and returns the same result, allowing the chain to continue.

## Complete Example

```csharp
public async Task<Result<UserDto>> GetUserDtoAsync(int id)
{
    return await GetUserAsync(id)
        .EnsureAsync(
            user => user.IsActive,
            Error.Validation("User.State", "The user is not active."))
        .BindAsync(user => LoadPermissionsAsync)
        .OnSuccessAsync(user => Log($"User loaded: {user.Id}"))
        .OnFailureAsync(error => Log($"Error: {error.Message}"))
        .MapAsync(UserToDto);
}

private static UserDto UserToDto(User user) =>
    new(user.Id, user.Name);
```
Consuming the result:

```csharp
public async Task<string> GetUserMessageAsync(int id)
{
    var result = await _userService.GetUserDtoAsync(id);

    return result.Match(
        dto => $"User: {dto.Name}",
        error => $"Error: {error.Message}");
}
```

This flow represents a declarative composition where each step operates on the result of the previous one without breaking the chain.

If a step fails, the following steps are not executed. The error is propagated to the end of the flow.

> [!NOTE]
> The synchronous flow is best suited for pure validations, in-memory transformations, or domain rules that do not require I/O.

```csharp
public Result<int> CalculateTotal(int price)
{
    return Ok(price)
        .Ensure(value => value > 0,
                Error.Validation("Value.Invalid", "The value must be greater than 0"))
        .Map(value => value * 2)
        .OnSuccess(value => Log($"Calculated total: {value}"));
}
```

## Usage Guidelines

##### 🟢 Best Practices

- Use Result<T> for expected use-case failures.
- Return an Error as soon as a validation fails.
- Prefer async flows when working with I/O, external services, or repositories.
- Use Map when you only need to transform the value.
- Use Bind when the next step returns a Result.
- Use Ensure for validations within the chain.
- Use OnSuccess and OnFailure for side effects (logging, metrics, etc.).
- End flows with Match, IsFailure, or IsSuccess.
- Keep Code values stable; Message values may change.
- If there is no valid value, return an Error instead of null.

##### 🔴 Anti-Patterns

- Do not use Result<T> to hide programming errors.
- Do not access Value without checking for success first.
- Do not use OnSuccess or OnFailure to modify the result.
- Do not return null as a successful result.
- Do not mix domain logic and error handling within the same flow.

> [!IMPORTANT]
> Use Result<T> to control the flow, not to hide it.

## Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2026 BracoZS
