namespace ResultPattern;

/// <summary>
/// Represents a domain error with a type, machine-readable code, and human-readable message.
/// </summary>
/// <param name="Type">The category of the error.</param>
/// <param name="Code">A machine-readable error code (e.g., <c>"Validation.Email"</c>).</param>
/// <param name="Message">A human-readable description of the error.</param>
public sealed partial record Error(ErrorType Type, string Code, string Message)
{
    public Error(string message) : this(ErrorType.General, "Generic", message) { }

    /// <summary>Gets a sentinel error representing the absence of an error.</summary>
    public static Error None { get; } = new(ErrorType.None, string.Empty, "No error.");
}

// ─── Factories ──────────────────────────────────────────────

public sealed partial record Error
{
    // ─── General ───────────────────────────────────────────────

    /// <summary>Creates a general-purpose error with an optional custom message.</summary>
    /// <param name="message">The error message. When <see langword="null"/>, a default is used.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.General"/>.</returns>
    public static Error General(string? message = null)
        => new(ErrorType.General, "General.Custom", message ?? "An unexpected error has occurred.");

    // ─── Auth ───────────────────────────────────────────────────

    /// <summary>Gets a sentinel error for unauthorized access.</summary>
    public static Error Unauthorized { get; } = new(ErrorType.Unauthorized, "Auth.Unauthorized", "Not authorized.");

    /// <summary>Creates a forbidden error for when the user lacks permissions.</summary>
    /// <param name="resourceName">The resource the user tried to access.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.Forbidden"/>.</returns>
    public static Error Forbidden(string resourceName)
        => new(ErrorType.Forbidden, $"Forbidden.{resourceName}", $"Access to '{resourceName}' is forbidden.");

    // ─── Domain ─────────────────────────────────────────────────

    /// <summary>Creates a validation error for the specified field.</summary>
    /// <param name="field">The field name that failed validation.</param>
    /// <param name="reason">A description of the validation failure.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.Validation"/>.</returns>
    public static Error Validation(string field, string reason)
        => new(ErrorType.Validation, $"Validation.{field}", reason);

    /// <summary>Creates a not-found error for the specified resource.</summary>
    /// <param name="resourceName">The name of the resource that was not found.</param>
    /// <param name="key">The optional identifier used when searching for the resource.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.NotFound"/>.</returns>
    public static Error NotFound(string resourceName, object? key = null)
        => new(
            ErrorType.NotFound,
            $"NotFound.{resourceName}",
            key is null
                ? $"Resource '{resourceName}' not found."
                : $"Resource '{resourceName}' with identifier '{key}' not found.");

    /// <summary>Creates a conflict error for the specified resource.</summary>
    /// <param name="resourceName">The name of the resource involved in the conflict.</param>
    /// <param name="reason">A description of the conflict.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.Conflict"/>.</returns>
    public static Error Conflict(string resourceName, string reason)
        => new(ErrorType.Conflict, $"Conflict.{resourceName}", reason);

    /// <summary>Creates an invalid operation error.</summary>
    /// <param name="resourceName">The resource or operation name.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.InvalidOperation"/>.</returns>
    public static Error InvalidOperation(string resourceName)
        => new(ErrorType.InvalidOperation, $"Invalid.{resourceName}", $"Operation '{resourceName}' is not valid in the current state.");

    /// <summary>Creates a not-supported error.</summary>
    /// <param name="resourceName">The unsupported resource or operation.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.NotSupported"/>.</returns>
    public static Error NotSupported(string resourceName)
        => new(ErrorType.NotSupported, $"NotSupported.{resourceName}", $"Operation '{resourceName}' is not supported.");

    // ─── Tech ───────────────────────────────────────────────────

    /// <summary>Creates an internal error for unexpected technical failures.</summary>
    /// <param name="details">Optional details such as a stack trace or exception message.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.Internal"/>.</returns>
    public static Error Internal(string? details = null)
        => new(ErrorType.Internal, "Internal.General", details ?? "An internal error occurred.");

    /// <summary>Creates a timeout error.</summary>
    /// <param name="resourceName">The operation that timed out.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.Timeout"/>.</returns>
    public static Error Timeout(string resourceName)
        => new(ErrorType.Timeout, $"Timeout.{resourceName}", $"Operation '{resourceName}' timed out.");

    /// <summary>Creates a cancellation error.</summary>
    /// <param name="resourceName">The operation that was cancelled.</param>
    /// <returns>An <see cref="Error"/> with <see cref="ErrorType.Cancelled"/>.</returns>
    public static Error Cancelled(string resourceName)
        => new(ErrorType.Cancelled, $"Cancelled.{resourceName}", $"Operation '{resourceName}' was cancelled.");
}
