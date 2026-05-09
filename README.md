## 🚧 En construcción

Esta sección aún se encuentra en desarrollo y puede cambiar en futuras versiones de la documentación.

# Result Pattern

`Result<T>` representa el resultado de una operacion: `exito` con valor o `fallo` con error.

Sirve para modelar fallos que pertenecen al flujo normal de la aplicacion: validacion, recursos no encontrados, lectura de datos,  conflictos, permisos,persistencia respuestas invalidas o errores esperados de servicios externosy flujos donde cada paso depende del anterior.

Sirve para modelar:

  * Fallos que pertenecen al flujo normal de la aplicación.
  * Validación.
  * Recursos no encontrados.
  * Lectura de datos.
  * Conflictos.
  * Permisos.
  * Persistencia.
  * Respuestas inválidas o errores esperados de servicios externos.
  * Flujos donde cada paso depende del anterior.


Usalo cuando una operacion puede fallar de forma normal o cualquier flujo donde el fallo sea parte del caso de uso.

No reemplaza las excepciones, las complementa. No lo uses para esconder bugs. Si el problema es un bug, una dependencia mal configurada o un estado imposible, una excepcion sigue siendo la herramienta correcta.


## `Result` type

La forma base de trabajar con `Result<T>` es retornando explícitamente **éxito** o **fallo**.

`Result<T>` (con valor) se usa cuando se pretende devolver un valor.

```csharp
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _users.FindAsync(id);

    if (user is null)
        return Result<User>.Failure(Error.NotFound("User.NotFound", id));

    return Result<User>.Ok(user);
}
```

`Result` (sin valor) se usa en operaciones donde solo importa saber si la operacion fue exitosa o fallida.

```csharp
public async Task<Result> SaveUserAsync(User user)
{
    if (user is null)
        return Result.Failure(Error.Validation("User.Required", "El usuario es requerido"));

    await _users.SaveAsync(user);

    return Result.Ok();
}
```

> Nota: el resultado puede seguir devolviéndose de forma explícita o mediante [conversión implícita](#conversión-implícita)

#### Properties

`Result<T>` expone las siguientes propiedades para inspeccionar el estado del resultado:

- `IsSuccess`: Indica si la operación fue exitosa.
- `IsFailure`: Indica si la operación falló. Es el inverso de `IsSuccess`.
- `Error`: Contiene el error asociado al fallo. Si el resultado es exitoso, contiene `Error.None`.
- `Value`: Contiene el valor del resultado cuando la operación es exitosa. Si el resultado es un fallo, acceder a esta propiedad lanza una excepción.

Verifica la propiedad `IsSuccess` o `IsFailure` cuando el flujo necesite salir temprano.

```csharp
Result<User> result = await GetUserAsync(id);

if (result.IsFailure)
{
    Console.WriteLine("Ocurrió un error inesperado: {result.Error.Message}");
    return;
}

Console.WriteLine(result.Value);
```

> Nota: `Value` solo debe leerse despues de confirmar que el resultado fue exitoso. De lo contrario se producirá una excepción. Este es el comportamiento esperado.

## `Error` type

`Error` describe un fallo esperado de forma estable.

```csharp
public sealed record Error(ErrorType Type, string ErrorCode, string Message){};
```
<details>
  <summary>Ver enum ErrorType</summary>

```csharp
public enum ErrorType
{
    // Éxito
    None,

    // Entrada/Permisos
    Validation,        // Datos de entrada inválidos
    Unauthorized,      // No autenticado / credenciales inválidas
    Forbidden,         // Sin permisos

    // Estado/Recursos
    NotFound,          // Recurso no existe
    Conflict,          // Estado inconsistente (ej: duplicado, concurrencia)
    InvalidOperation,  // Estado incorrecto para ejecutar la acción)
    NotSupported,      // Operación no soportada en este contexto

    // Ejecución
    Timeout,           // Operación excedió tiempo límite
    Cancelled,         // Operación abortada (token de cancelación, usuario, etc.

    // Otros
    General,           // Error no especificado, desconocido
    Internal,          // Error técnico interno (bug, excepción no manejada)
}
```
</details>
<br>

Parámetros:
- `ErrorType`: Categoriza el error a nivel general. Permite identificar rápidamente la naturaleza del fallo.

- `ErrorCode`: Identificador específico del error dentro del dominio de la aplicación. Debe ser estable, con significado funcional, y útil para trazabilidad, logs, pruebas e integraciones.

- `Message`: Describe el problema de forma legible para humanos y puede variar según el contexto.


Puedes usar los factories incluidos para crear errores con el mismo formato.

```csharp
Error.Validation("User.EmailInvalid", "El correo electrónico no es válido.");
Error.NotFound("User.NotFound", $"El usuario con id '{id}' no existe.");
Error.Conflict("User.AlreadyExists", "El usuario ya se encuentra registrado.");
Error.Unauthorized("Auth.InvalidCredentials", "Las credenciales son incorrectas.");
Error.Internal("No se pudo leer la configuración.");
Error.General();
```

### Conversión implícita

También es posible devolver un `Result`/`Result<T>` ó `Error` directamente desde un método que retorna Result<T> gracias a la conversión implícita [[docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/user-defined-conversion-operators?utm_source=chatgpt.com)].

`Error -> Result<T> / Result`
 
`T -> Result<T>`

Esto reduce boilerplate y permite expresar fallos de forma más directa.

```csharp
// works also with async
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _users.FindAsync(id);  

    if (user is null)
   
        return Error.NotFound("User.NotFound", id);  // Error -> Result<User>

    return user;        // User -> Result<User>
}
```

## Ejemplo Completo

```csharp
public async Task<Result<UserDto>> GetUserDtoAsync(int id)
{
    return await GetUserAsync(id)
        .EnsureAsync(
            user => user.IsActive,
            Error.Validation("User.State", "El usuario no esta activo."))
        .BindAsync(user => LoadPermissionsAsync(user))
        .TapAsync(user => Log($"Usuario cargado: {user.Id}"))
        .TapFailureAsync(error => Log(error.Message))
        .MapAsync(user => new UserDto(user.Id, user.Name));
}
```
```csharp
public async Task<string> GetUserMessageAsync(int id)
{
    var result = await _userService.GetUserDtoAsync(id);

    return result.Match(
        dto => $"Usuario: {dto.Name}",
        error => error.Message);
}
```
Si un paso falla, los siguientes no se ejecutan. El error se propaga hasta el final del flujo.

## Extensiones

Las extensiones cuentan con una versión síncrona y una asíncrona. Sin embargo, la versión asíncrona es donde el patrón aporta mayor valor, ya que permite encadenar validaciones, repositorios, servicios externos y transformaciones sin recurrir a múltiples `if` anidados.

La versión async trabaja sobre `Task<Result<T>>` y es la más común en flujos de aplicación modernos, donde predominan operaciones I/O y composición de resultados.

### Match / MatchAsync

Permiten ejecutar lógica en función del estado del resultado

Usa `Match` o `MatchAsync` para cerrar el flujo y manejar exito o fallo.

```csharp
// 1. Operacion con valor de retorno + proyección (T -> string)
string message = await GetUserAsync(id)
    .MatchAsync(
        user => $"Usuario: {user.Name}",
        error => $"Error: {error.Message}");

// 2. Solo efectos secundarios (no retorno útil)
await GetUserAsync(id)
    .MatchAsync(
        user => Console.WriteLine($"Usuario actual: {user.Name}"),
        error => Console.WriteLine(error.Message));

// 3. Operación sin valor de retorno (Result)
await SaveUserAsync(user)
    .MatchAsync(
        () => ShowSaved(),
        error => ShowError(error.Message));
```

### Map / MapAsync 

`Map` o `MapAsync` se usan cuando quieres **transformar** el valor de un `Result<T>` exitoso.

En caso de fallo, el error se propaga sin aplicar la transformación.

```csharp
// Async: transforma User -> UserDto si fue exitoso
Result<UserDto> asyncResult = await GetUserAsync(id)
    .MapAsync(user => new UserDto(user.Id, user.Name));

// Sync: misma transformación en versión síncrona
Result<UserDto> syncResult = GetUser(id)
    .Map(user => new UserDto(user.Id, user.Name));
```

### Bind / BindAsync

**Concatenan** varios pasos dependientes. Usa `Bind` o `BindAsync`  cuando el siguiente paso de la cadena también devuelve un `Result<T>` y, por lo tanto, puede fallar.
 Además, estas operaciones pueden encadenarse múltiples veces, permitiendo construir flujos complejos de forma lineal y sin anidaciones.

En caso de fallo el error se propaga automáticamente sin continuar la ejecución.

```csharp
Result<Profile> asyncResult = await GetUserAsync(id)
    .BindAsync(user => GetProfileAsync(user.Id));

// encadenamiento múltiple
Result<Order> result = await ValidateRequestAsync(request)
    .BindAsync(validRequest => CreateOrderAsync(validRequest))
    .BindAsync(order => ReserveStockAsync(order))
    .BindAsync(order => SaveOrderAsync(order));
```
> El flujo se lee de arriba hacia abajo como una secuencia de pasos del dominio.

Este enfoque evita el *callback hell* o el crecimiento de `if` anidados, manteniendo el flujo de ejecución más declarativo y fácil de leer.

### Ensure / EnsureAsync

Usa `Ensure` o `EnsureAsync` para **validar** el valor exitoso, sin salir del flujo.

```csharp
// Async.
Result<User> asyncResult = await GetUserAsync(id)
    .EnsureAsync(
        user => user.IsActive,  // succes
        Error.Validation("User.State", "El usuario no esta activo."));  // failure

// Sync.
Result<User> syncResult = GetUser(id)
    .Ensure(
        user => user.IsActive,
        Error.Validation("User.State", "El usuario no esta activo."));
```

Si el resultado ya era fallo, la validacion no se ejecuta y el error original se conserva.

### Tap / TapAsync

Usa `Tap` o `TapAsync` para **ejecutar** una accion lateral cuando hay exito (logging, metricas, cache o notificaciones, etc.) **sin alterar** el resultado.

```csharp
// Async.
Result<User> asyncResult = await GetUserAsync(id)
    .TapAsync(user => Log($"Usuario encontrado: {user.Id}"));

// Sync.
Result<User> syncResult = GetUser(id)
    .Tap(user => Log($"Usuario encontrado: {user.Id}"));
```

> Es el equivalente funcional a un `void side-effect` dentro de una cadena de composición

No alteran el Result; simplemente ejecutan una acción sobre el valor exitoso y devuelve el mismo resultado, permitiendo continuar la cadena.


### TapFailure / TapFailureAsync

Usa `TapFailure` o `TapFailureAsync` para ejecutar una accion lateral (logging, métricas, alertas, etc.) cuando la operación falla, sin modificar el resultado. 
 
```csharp
// Async.
Result<User> asyncResult = await GetUserAsync(id)
    .TapFailureAsync(error => Log(error.Message));

// Sync.
Result<User> syncResult = GetUser(id)
    .TapFailure(error => Log(error.Message));
```

No transforma el error; solo observa el fallo y devuelve el mismo resultado, permitiendo continuar la cadena.

## Guia Rapida

| Funcion | Uso |
| --- | --- |
| `Match` / `MatchAsync` | Cerrar el flujo y manejar exito o fallo. |
| `Map` / `MapAsync` | Transformar el valor exitoso. |
| `Bind` / `BindAsync` | Encadenar otra operacion que puede fallar. |
| `Ensure` / `EnsureAsync` | Validar el valor exitoso. |
| `Tap` / `TapAsync` | Ejecutar una accion lateral en exito. |
| `TapFailure` / `TapFailureAsync` | Ejecutar una accion lateral en fallo. |

El flujo sincrono queda para validaciones puras, transformaciones en memoria o reglas de dominio que no requieren IO.

```csharp
Result<UserDto> result = GetUser(id)
    .Ensure(
        user => user.IsActive,
        Error.Validation("User.State", "El usuario no esta activo."))
    .Bind(user => LoadPermissions(user))
    .Tap(user => Log($"Usuario cargado: {user.Id}"))
    .TapFailure(error => Log(error.Message))
    .Map(user => new UserDto(user.Id, user.Name));
```

## Consejos de Uso

Usa `Result<T>` para errores esperados del caso de uso.

No lo uses para ocultar errores de programacion.

Devuelve `Error` tan pronto como una validacion falle.

Prefiere cadenas async cuando el flujo cruza IO, servicios externos o repositorios.

Usa `Map` si solo transformas el valor.

Usa `Bind` si el siguiente paso tambien devuelve `Result`.

Usa `Ensure` para validar dentro de una cadena.

Usa `Tap` y `TapFailure` para acciones laterales, no para cambiar el resultado.

Termina los flujos con `Match`, `IsFailure` o `IsSuccess`.

No leas `Value` sin comprobar antes que el resultado fue exitoso.

No devuelvas `null` como exito. Si no hay valor, devuelve un `Error`.

Manten los `ErrorCode` estables. `Message` (en `Error`) puede cambiar; el codigo deberia servir para identificar el error.