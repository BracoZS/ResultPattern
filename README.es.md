# ResultPattern.Net

[![NuGet version](https://img.shields.io/nuget/v/ResultPattern.Net.svg)](https://www.nuget.org/packages/ResultPattern.Net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![build](https://github.com/BracoZS/ResultPattern/actions/workflows/build.yml/badge.svg)](https://github.com/BracoZS/ResultPattern/actions/workflows/build.yml)

Manejo explícito de éxito/fallo en C# sin excepciones para flujos de control predecibles.

## ¿Por qué?

El patrón Result hace explícitos el éxito y el fallo en los tipos del sistema. Cada firma de método declara que puede fallar, y el compilador asegura que ambos caminos se manejen.

- **Flujo de control explícito** — sin ramas ocultas via try/catch, sin sorpresas en runtime
- **Sin nulls** — cada fallo lleva un Error estructurado, nunca una referencia nula
- **Pipelines lineales** — encadena operaciones con Map, Bind y Ensure; los errores cortocircuitan automáticamente sin anidar
- **Consistente en toda tu base de código** — mismo patrón para validaciones, búsquedas, permisos y llamadas externas

## Destacados

- **Result\<T\> + Result** — resultados fuertemente tipados para operaciones con valor y void
- **Tipo Error** — modelo de error estructurado con factories estáticos (Validation, NotFound, Conflict, etc.)
- **Extensiones fluidas** — Map, Bind, Ensure, OnSuccess, OnFailure, Match — síncronas y asíncronas
- **Try / TryAsync** — envuelve excepciones en Result automáticamente con mapeo opcional de error
- **Conversiones implícitas** — retorna `T` o `Error` directamente, sin envoltura manual
- **Composición de pipelines** — encadena operaciones; los errores cortocircuitan automáticamente

## Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `IsSuccess` | `bool` | `true` cuando la operación tuvo éxito |
| `IsFailure` | `bool` | `true` cuando la operación falló |
| `Value` | `T` | El valor del resultado (lanza excepción en fallo) |
| `Error` | `Error` | El error (`Error.None` en éxito) |

## Inicio rápido

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

## Uso

### Result básico

```csharp
Result save = Result.Ok();
Result fail = Result.Failure(Error.Internal("Something went wrong"));
```

### Result con valor

```csharp
Result<User> user = Result.Ok(new User("Alice"));
Result<User> error = Result<User>.Failure(Error.NotFound("User", 42));

// Conversiones implícitas
Result<User> a = new User("Alice");
Result<User> b = Error.NotFound("User", 42);
```

### Factories de Error

```csharp
Error.Validation("Email.Invalid", "Invalid email format");
Error.NotFound("User", 42);
Error.Conflict("Order.Duplicate", "Order already exists");
Error.Unauthorized;
Error.Internal("Failed to read configuration");
Error.General();
```

### Try — atrapa excepciones en Result

```csharp
Result<int> result = Result.Try(() => int.Parse(input));

// Con mapeo personalizado de error
Result<User> user = Result.Try(
    () => _repo.Find(id),
    ex => Error.Internal(ex.Message));
```

```csharp
// Async
Task<Result<User>> user = Result.TryAsync(
    () => _repo.FindAsync(id));
```

## Extensiones

### Pipeline síncrono fluido

```csharp
Result<Order> result = Validate(request)
    .Ensure(r => r.Total > 0, Error.Validation("Order.Total", "Must be positive"))
    .Map(r => new Order(r))
    .Bind(order => Save(order))
    .OnSuccess(order => Log($"Order {order.Id} created"))
    .OnFailure(error => Log($"Failed: {error.Message}"));
```

### Pipeline asíncrono fluido

```csharp
Task<Result<OrderDto>> result = ValidateAsync(request)
    .EnsureAsync(r => r.Total > 0, Error.Validation("Order.Total", "Must be positive"))
    .MapAsync(r => new Order(r))
    .BindAsync(order => SaveAsync(order))
    .OnSuccessAsync(order => Log($"Order {order.Id} created"))
    .OnFailureAsync(error => Log($"Failed: {error.Message}"))
    .MapAsync(order => new OrderDto(order.Id, order.Total));
```

### Switch — termina y ejecuta una acción

```csharp
GetUserAsync(id).SwitchAsync(
    user => Console.WriteLine($"User: {user.Name}"),
    error => Console.WriteLine(error.Message));
```

### Match — termina y produce un valor

```csharp
string message = await GetUserAsync(id)
    .MatchAsync(
        user => $"User: {user.Name}",
        error => $"Error: {error.Message}");
```

## Referencia de extensiones

| Método | Opera sobre | Propósito |
|--------|-------------|-----------|
| `Map` / `MapAsync` | `Result<T>` / `Task<Result<T>>` | Transforma el valor |
| `Bind` / `BindAsync` | `Result<T>` / `Task<Result<T>>` | Encadena a otro Result |
| `Ensure` / `EnsureAsync` | `Result<T>` / `Task<Result<T>>` | Valida el valor |
| `OnSuccess` / `OnSuccessAsync` | `Result<T>` / `Task<Result<T>>` | Efecto lateral en éxito |
| `OnFailure` / `OnFailureAsync` | `Result<T>` / `Task<Result<T>>` | Efecto lateral en fallo |
| `Match` / `MatchAsync` | `Result<T>` / `Task<Result<T>>` | Cierra flujo, produce valor |
| `Switch` / `SwitchAsync` | `Result<T>` / `Task<Result<T>>` | Cierra flujo, ejecuta acción |

## Requisitos

- .NET 10.0+

## Contribuciones

¡Las contribuciones son bienvenidas! Abre un issue o envía un pull request.

## Licencia

[MIT](LICENSE)

Copyright (c) 2026 BracoZS
