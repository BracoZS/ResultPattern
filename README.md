# Result Pattern

[![build](https://github.com/BracoZS/ResultPattern/actions/workflows/build.yml/badge.svg)](https://github.com/BracoZS/ResultPattern/actions/workflows/build.yml)

`Result<T>` representa el resultado de una operacion: `exito` con valor o `fallo` con error.

Sirve para modelar fallos que pertenecen al flujo normal de la aplicacion:

  * Fallos que pertenecen al flujo normal de la aplicación.
  * Validación.
  * Recursos no encontrados.
  * Lectura de datos.
  * Conflictos.
  * Permisos.
  * Persistencia.
  * Respuestas inválidas o errores esperados de servicios externos.
  * Flujos donde cada paso depende del anterior.

Es útil en situaciones donde una operación puede fallar normalmente o cuando el fallo es parte del comportamiento previsto.

No reemplaza las excepciones, las complementa. 

### ¿Por qué el patrón Result?

El patrón `Result` ofrece una forma clara y explícita de gestionar el éxito y el fracaso sin depender de excepciones para el flujo de control.

En lugar de lanzar excepciones o devolver `null`, los métodos devuelven un resultado estructurado que hace que los resultados sean predecibles y más fáciles de gestionar.

❌ Problemas que resuelve
Los enfoques tradicionales suelen dar lugar a:

- Flujo de control oculto mediante excepciones
- Valores `null` y posibles excepciones `NullReferenceException`
- Sentencias `if` profundamente anidadas
- Gestión de errores inconsistente en toda la aplicación

✔️ Ventajas
Con `Result<T>` obtienes:

- Flujo explícito de éxito/fracaso
- Flujo de control no basado en excepciones
- API más seguras y predecibles
- Composición más sencilla de operaciones
- Mayor facilidad para realizar pruebas


## `Result` type

La forma base de trabajar con `Result<T>` es retornando explícitamente **éxito** o **fallo**.


`Result` (sin valor) se usa en operaciones donde solo importa saber si la operacion fue exitosa o fallida.

```csharp
public Result SaveUser(User user)
{
    if (user is null)
        return Result.Failure(Error.Validation("User.Required", "El usuario es requerido"));

    _users.Save(user);

    return Result.Ok();
}
```
`Result<T>` (con valor) se usa cuando se pretende devolver un valor.

```csharp
public Result<User> CreateUser(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result<User>.Failure(Error.Validation("User.NotFound", "El usuario es requerido"));

    return Result<User>.Ok(new User(name));
}
```

> Nota: el resultado puede devolverse de forma explícita o implícta mediante [conversión implícita](#conversión-implícita)

#### Properties

`Result<T>` expone las siguientes propiedades para inspeccionar el estado del resultado:

- `IsSuccess`: Indica si la operación fue exitosa.
- `IsFailure`: Indica si la operación falló. Es el inverso de `IsSuccess`.
- `Error`: Contiene el error asociado al fallo. Si el resultado es exitoso, contiene `Error.None`.
- `Value`: Contiene el valor del resultado cuando la operación es exitosa. *Si el resultado es un fallo, acceder a esta propiedad lanza una excepción*.

Verifica la propiedad `IsSuccess` o `IsFailure` cuando el flujo aplique _'early returns'_.

```csharp
Result<User> result = await GetUserAsync(id);

if (result.IsFailure)
{
    Console.WriteLine($"Error: {result.Error.Message}");
    return;
}

User user = result.Value;
Console.WriteLine($"Usuario encontrado: {user.Name}");
```

> [!WARNING]
> `Value` solo debe leerse despues de confirmar que el resultado fue exitoso. De lo contrario se producirá una excepción. Este es el comportamiento esperado.

## `Error` type

`Error` describe un fallo esperado de forma estable.

```csharp
public sealed record Error(ErrorType Type, string Code, string Message);
```
| Propiedad   | Tipo        | Descripción                                                                                                                                       |
| ----------- | ----------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Type`      | `ErrorType` | Categoriza el error a nivel general. Permite identificar rápidamente la naturaleza del fallo.                                                     |
| `Code` | `string`    | Identificador específico del error dentro del dominio de la aplicación. Debe ser estable y útil para trazabilidad, logs, pruebas e integraciones. |
| `Message`   | `string`    | Describe el problema de forma legible para humanos y puede variar según el contexto.                                                              |




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

> [!TIP]
>
> El tipo `Error` expone factories estáticos para representar los errores más comunes del sistema.
>
> Puedes utilizar los factories incluidos para crear errores con un formato consistente:

```csharp
Error.Validation("User.EmailInvalid", "El correo electrónico no es válido.");
Error.NotFound("User.NotFound", $"El usuario con id '{id}' no existe.");
Error.Conflict("User.AlreadyExists", "El usuario ya se encuentra registrado.");
Error.Unauthorized("Auth.InvalidCredentials", "Las credenciales son incorrectas.");
Error.Internal("No se pudo leer la configuración.");
Error.General();
```

### Conversión implícita

El tipo `Result<T>` soporta conversiones implícitas  para reducir boilerplate y permitir una expresión más natural del flujo [[docs](https://learn.microsoft.com/es-es/dotnet/csharp/language-reference/operators/user-defined-conversion-operators?utm_source=chatgpt.com)].

Esto permite retornar directamente un `Error` o un valor `T` sin necesidad de envolverlos manualmente en `Result`.


`Error → Result / Result<T>`

`T     → Result<T>`


Esto reduce boilerplate y permite expresar fallos de forma más directa, mejorando la legibilidad en métodos que devuelven `Result<T>`.

```csharp
public async Task<Result<User>> GetUserAsync(int id)
{
    var user = await _repo.FindAsync(id);

    return user is null
        ? Error.NotFound("User.NotFound", $"El usuario con id '{id}' no existe")
        : user;
}
```
En el ejemplo anterior:

- `Error → Result<User>` mediante conversión implícita
- `User → Result<User>` mediante conversión implícita


## Extensiones

Las extensiones están disponibles en versiones síncronas y asíncronas.

La versión asíncrona es donde el patrón aporta mayor valor, ya que permite encadenar validaciones, repositorios, servicios externos y transformaciones sin necesidad de múltiples `if` anidados. Trabaja sobre `Task<Result<T>>` y es la opción más utilizada en aplicaciones modernas, donde predominan operaciones de I/O y composición de resultados.

| Funcion | Uso |
| --- | --- |
| [`Map` / `MapAsync`](#map--mapasync) | Transforma |
| [`Bind` / `BindAsync`](#bind--bindasync) | Encadena |
| [`Ensure` / `EnsureAsync`](#ensure--ensureasync) | Valida |
| [`OnSuccess` / `OnSuccessAsync`](#onsuccess--onsuccessasync) | Accion en exito |
| [`OnFailure` / `OnFailureAsync`](#onfailure--onfailureasync) | Accion en fallo |
| [`Match` / `MatchAsync`](#match--matchasync) | Cierra (retorna valor) |
| [`Switch` / `SwitchAsync`](#switch--switchasync) | Cierra (ejecuta accion) |

### Match / MatchAsync

Usa `Match` o `MatchAsync` para devolver un tipo, manejando ambos casos (éxito o error) según el resultado. Define qué hacer cuando la operación tiene éxito y qué hacer cuando falla.

```csharp
string message = await GetUserAsync(id)
    .MatchAsync(
        user => $"Usuario: {user.Name}",
        error => $"Error: {error.Message}");
```

### Switch / SwitchAsync

Usa `Switch` o `SwitchAsync` para cerrar el flujo y **ejecutar una acción**. No devuelve un valor.

```csharp
await GetUserAsync(id)
    .SwitchAsync(
        user => Console.WriteLine($"Usuario actual: {user.Name}"),
        error => Console.WriteLine(error.Message));
```

### Map / MapAsync 

`Map` o `MapAsync` se utilizan para **transformar** el valor contenido en un `Result<T>` exitoso.

Si el resultado es un error, se propaga sin aplicar la transformación.

```csharp
// Async: User -> UserDto si la operación fue exitosa
Result<UserDto> asyncResult = await GetUserAsync(id)
    .MapAsync(user => new UserDto(user.Id, user.Name));

// Sync: versión síncrona
Result<UserDto> syncResult = GetUser(id)
    .Map(user => new UserDto(user.Id, user.Name));
```

### Bind / BindAsync

`Bind` y `BindAsync` se utilizan para **encadenar** múltiples operaciones dependientes. Usalos  cuando el siguiente paso de la cadena también devuelve un `Result<T>` y, por lo tanto, puede fallar. Permite construir flujos complejos de forma lineal y sin anidaciones.

Si una de las operaciones falla, el error se propaga automáticamente, se devuelve y la ejecución se detiene.

```csharp
// Transformación encadenada con otra operación que devuelve Result
Result<Profile> asyncResult = await GetUserAsync(id)
    .BindAsync(user => GetProfileAsync(user.Id));

// Encadenamiento múltiple de operaciones
Result<Order> result = await ValidateRequestAsync(request)
    .BindAsync(validRequest => CreateOrderAsync(validRequest))
    .BindAsync(order => ReserveStockAsync(order))
    .BindAsync(order => SaveOrderAsync(order));
```
> El flujo se lee de arriba hacia abajo como una secuencia de pasos del dominio.

Este enfoque evita el *callback hell* o el crecimiento de `if` anidados, manteniendo el flujo de ejecución más declarativo y fácil de leer.

### Ensure / EnsureAsync

Usa `Ensure` o `EnsureAsync` para **validar** el valor de un `Result` exitoso, sin salir del flujo.

```csharp
// Async
Result<User> asyncResult = await GetUserAsync(id)
    .EnsureAsync(
        user => user.IsActive,
        Error.Validation("User.State", "El usuario no esta activo."));

// Sync
Result<User> syncResult = GetUser(id)
    .Ensure(
        user => user.IsActive,
        Error.Validation("User.State", "El usuario no esta activo."));
```

Si el resultado ya es un fallo, la validación no se ejecuta y se conserva el error original..

### OnSuccess / OnSuccessAsync

Usa `OnSuccess` o `OnSuccessAsync` para **ejecutar** una accion lateral unicamente cuando el `Result` es **exitoso**, sin modificar el valor **ni interrumpir el flujo**. 

Son útiles para logging, metricas, cache o notificaciones, etc.

```csharp
// Async
Result<User> asyncResult = await GetUserAsync(id)
    .OnSuccessAsync(user => Log($"Usuario encontrado: {user.Id}"));

// Sync
Result<User> syncResult = GetUser(id)
    .OnSuccess(user => Log($"Usuario encontrado: {user.Id}"));
```

> [!TIP]
> `OnSuccess` es útil cuando necesitas observar el flujo sin interferir con su resultado.


### OnFailure / OnFailureAsync

Usa `OnFailure` o `OnFailureAsync` para **ejecutar** una accion lateral (logging, métricas, alertas, etc.) solamente cuando la operación **falla**, sin modificar el resultado **ni interrumpir el flujo**. 
 
```csharp
// Async
Result<User> asyncResult = await GetUserAsync(id)
    .OnFailureAsync(error => Log(error.Message));

// Sync
Result<User> syncResult = GetUser(id)
    .OnFailure(error => Log(error.Message));
```

No transforma el error; solo observa el fallo y devuelve el mismo resultado, permitiendo continuar la cadena.

## Ejemplo Completo

```csharp
public async Task<Result<UserDto>> GetUserDtoAsync(int id)
{
    return await GetUserAsync(id)
        .EnsureAsync(
            user => user.IsActive,
            Error.Validation("User.State", "El usuario no esta activo."))
        .BindAsync(user => LoadPermissionsAsync)
        .OnSuccessAsync(user => Log($"Usuario cargado: {user.Id}"))
        .OnFailureAsync(error => Log($"Error: {error.Message}"))
        .MapAsync(UserToDto);
}

private static UserDto UserToDto(User user) =>
    new(user.Id, user.Name);
```
Consumo del resultado:
```csharp
public async Task<string> GetUserMessageAsync(int id)
{
    var result = await _userService.GetUserDtoAsync(id);

    return result.Match(
        dto => $"Usuario: {dto.Name}",
        error =>  $"Error: {error.Message}");
}
```
Este flujo representa una composición declarativa donde cada paso opera sobre el resultado anterior sin romper la cadena.

Si un paso falla, los siguientes no se ejecutan. El error se propaga hasta el final del flujo.

> [!NOTE]
>El flujo sincrono queda para validaciones puras, transformaciones en memoria o reglas de dominio que no requieren IO.

```csharp
public Result<int> CalculateTotal(int price)
{
    return Ok(price)
        .Ensure(value => value > 0,
                Error.Validation("Value.Invalid", "El valor debe ser mayor a 0"))
        .Map(value => value * 2)
        .OnSuccess(value => Log($"Total calculado: {value}"));
}
```

## Consejos de Uso

##### 🟢 Buenas prácticas

- Usa `Result<T>` para errores esperados del caso de uso.
- Devuelve `Error` tan pronto como falle una validación.
- Prefiere flujos `async` cuando haya I/O, servicios externos o repositorios.
- Usa `Map` cuando solo transformes el valor.
- Usa `Bind` cuando el siguiente paso devuelva un `Result`.
- Usa `Ensure` para validaciones dentro de la cadena.
- Usa `OnSuccess` y `OnFailure` para efectos laterales (logs, métricas, etc.).
- Finaliza los flujos con `Match`, `IsFailure` o `IsSuccess`.
- Mantén los `Code` estables; el `Message` puede cambiar.
- Si no hay valor válido, devuelve `Error` en lugar de `null`.
---

##### 🔴 Malas prácticas

- No uses `Result<T>` para ocultar errores de programación.
- No accedas a `Value` sin verificar éxito primero.
- No uses `OnSuccess` o `OnFailure` para modificar el resultado.
- No devuelvas `null` como éxito.
- No mezcles lógica de dominio con manejo de errores en el mismo flujo.

> [!IMPORTANT]
>- Usa `Result<T>` para controlar el flujo, no para esconderlo.





