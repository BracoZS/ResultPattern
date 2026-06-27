# ResultPattern.Net

[![NuGet version](https://img.shields.io/nuget/v/ResultPattern.Net.svg)](https://www.nuget.org/packages/ResultPattern.Net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![build](https://github.com/BracoZS/ResultPattern/actions/workflows/build.yml/badge.svg)](https://github.com/BracoZS/ResultPattern/actions/workflows/build.yml)

Explicit success/failure handling in C# without exceptions for predictable control flow.

## Why?

The Result pattern makes success and failure explicit in your type system. Every method signature declares that it can fail, and the compiler ensures both paths are handled.

- **Explicit control flow** — no hidden try/catch branches, no surprises at runtime
- **No nulls** — every failure carries a structured Error, never a null reference
- **Linear pipelines** — chain operations with Map, Bind, and Ensure; errors short-circuit automatically without nesting
- **Consistent across your codebase** — same pattern for validations, lookups, permissions, and external calls

## Highlights

- **Result\<T\> + Result** — strongly-typed outcomes for value-returning and void operations
- **Error type** — structured error model with static factories (Validation, NotFound, Conflict, etc.)
- **Fluent extensions** — Map, Bind, Ensure, OnSuccess, OnFailure, Match — both sync and async
- **Try / TryAsync** — wrap exceptions into Result automatically with optional error mapping
- **Implicit conversions** — return `T` or `Error` directly, no wrapping boilerplate
- **Pipeline composition** — chain operations; errors short-circuit automatically

## Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsSuccess` | `bool` | `true` when the operation succeeded |
| `IsFailure` | `bool` | `true` when the operation failed |
| `Value` | `T` | The result value (throws on failure) |
| `Error` | `Error` | The error (`Error.None` on success) |

## Quick Start

```bash
dotnet add package ResultPattern.Net
```

```csharp
using ResultPattern;

Result<int> Divide(int a, int b)
{
    if (b == 0)
        return Error.Validation("Division.Zero", "Cannot divide by zero");

    return a / b;
}

var result = Divide(10, 2)
    .Ensure(x => x > 0, Error.Validation("Result.Negative", "Result must be positive"))
    .Map(x => x * 2);

Console.WriteLine(result.IsSuccess ? result.Value.ToString() : result.Error.Message);
// Output: 10
```

## Usage

### Basic Result

```csharp
Result save = Result.Ok();
Result fail = Result.Failure(Error.Internal("Something went wrong"));
```

### Result with value

```csharp
Result<User> user = Result.Ok(new User("Alice"));
Result<User> error = Result<User>.Failure(Error.NotFound("User", 42));

// Implicit conversions
Result<User> a = new User("Alice");
Result<User> b = Error.NotFound("User", 42);
```

### Error factories

```csharp
Error.Validation("Email.Invalid", "Invalid email format");
Error.NotFound("User", 42);
Error.Conflict("Order.Duplicate", "Order already exists");
Error.Unauthorized;
Error.Internal("Failed to read configuration");
Error.General();
```

### Try — catch exceptions into Result

```csharp
Result<int> result = Result.Try(() => int.Parse(input));

// With custom error mapping
Result<User> user = Result.Try(
    () => _repo.Find(id),
    ex => Error.Internal(ex.Message));
```

```csharp
// Async
Task<Result<User>> user = Result.TryAsync(
    () => _repo.FindAsync(id));
```

## Extensions

### Fluent sync pipeline

```csharp
Result<Order> result = Validate(request)
    .Ensure(r => r.Total > 0, Error.Validation("Order.Total", "Must be positive"))
    .Map(r => new Order(r))
    .Bind(order => Save(order))
    .OnSuccess(order => Log($"Order {order.Id} created"))
    .OnFailure(error => Log($"Failed: {error.Message}"));
```

### Fluent async pipeline

```csharp
Task<Result<OrderDto>> result = ValidateAsync(request)
    .EnsureAsync(r => r.Total > 0, Error.Validation("Order.Total", "Must be positive"))
    .MapAsync(r => new Order(r))
    .BindAsync(order => SaveAsync(order))
    .OnSuccessAsync(order => Log($"Order {order.Id} created"))
    .OnFailureAsync(error => Log($"Failed: {error.Message}"))
    .MapAsync(order => new OrderDto(order.Id, order.Total));
```

### Switch — terminate and execute an action

```csharp
GetUserAsync(id).SwitchAsync(
    user => Console.WriteLine($"User: {user.Name}"),
    error => Console.WriteLine(error.Message));
```

### Match — terminate and produce a value

```csharp
string message = await GetUserAsync(id)
    .MatchAsync(
        user => $"User: {user.Name}",
        error => $"Error: {error.Message}");
```

## Extensions reference

| Method | Works on | Purpose |
|--------|----------|---------|
| `Map` / `MapAsync` | `Result<T>` / `Task<Result<T>>` | Transform the value |
| `Bind` / `BindAsync` | `Result<T>` / `Task<Result<T>>` | Chain to another Result |
| `Ensure` / `EnsureAsync` | `Result<T>` / `Task<Result<T>>` | Validate the value |
| `OnSuccess` / `OnSuccessAsync` | `Result<T>` / `Task<Result<T>>` | Side effect on success |
| `OnFailure` / `OnFailureAsync` | `Result<T>` / `Task<Result<T>>` | Side effect on failure |
| `Match` / `MatchAsync` | `Result<T>` / `Task<Result<T>>` | Close flow, produce value |
| `Switch` / `SwitchAsync` | `Result<T>` / `Task<Result<T>>` | Close flow, execute action |

## Requirements

- .NET 10.0+


## Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.


## License

[MIT](LICENSE)

Copyright (c) 2026 BracoZS
