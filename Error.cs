namespace ResultPattern;

public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error None { get; } = new(ErrorType.None, string.Empty, "No error.");
    public static Error Unauthorized { get; } = new(ErrorType.Unauthorized, "Auth.Unauthorized", "No autorizado.");

    public static Error General(string? message = null)
        => new(ErrorType.General, "General.Custom", message ?? "Ha ocurrido un error inesperado.");

    public static Error Validation(string field, string reason)
        => new(ErrorType.Validation, $"Validation.{field}", reason);

    public static Error NotFound(string resourceName, object? key = null)
        => new(
            ErrorType.NotFound,
            $"NotFound.{resourceName}",
            key is null
                ? $"No se pudo encontrar el recurso '{resourceName}'."
                : $"No se pudo encontrar el recurso '{resourceName}' con identificador '{key}'.");

    public static Error Conflict(string resourceName, string reason)
        => new(ErrorType.Conflict, $"Conflict.{resourceName}", reason);

    public static Error Internal(string? details = null)
        => new(ErrorType.Internal, "Internal.General", details ?? "Ocurrio un error interno.");
}
