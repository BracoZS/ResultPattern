namespace ResultPattern;

public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error None { get; } = new(ErrorType.None, string.Empty, "No error.");
    public static Error Unauthorized { get; } = new(ErrorType.Unauthorized, "Auth.Unauthorized", "Not authorized.");

    public static Error General(string? message = null)
        => new(ErrorType.General, "General.Custom", message ?? "An unexpected error has occurred.");

    public static Error Validation(string field, string reason)
        => new(ErrorType.Validation, $"Validation.{field}", reason);

    public static Error NotFound(string resourceName, object? key = null)
        => new(
            ErrorType.NotFound,
            $"NotFound.{resourceName}",
            key is null
                ? $"Resource '{resourceName}' not found."
                : $"Resource '{resourceName}' with identifier '{key}' not found.");

    public static Error Conflict(string resourceName, string reason)
        => new(ErrorType.Conflict, $"Conflict.{resourceName}", reason);

    public static Error Internal(string? details = null)
        => new(ErrorType.Internal, "Internal.General", details ?? "An internal error occurred.");
}
