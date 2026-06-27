namespace ResultPattern;

/// <summary>Defines categories for domain errors.</summary>
public enum ErrorType
{
    // Éxito
    /// <summary>No error occurred. Used for sentinel purposes.</summary>
    None,

    // Entrada/Permisos
    /// <summary>Input data failed validation.</summary>
    Validation,        // Datos de entrada inválidos
    /// <summary>User is not authenticated or credentials are invalid.</summary>
    Unauthorized,      // No autenticado / credenciales inválidas
    /// <summary>User lacks permission to perform the operation.</summary>
    Forbidden,         // Sin permisos

    // Estado/Recursos
    /// <summary>A resource was not found.</summary>
    NotFound,          // Recurso no existe
    /// <summary>The operation caused a state conflict (e.g. duplicate, concurrency).</summary>
    Conflict,          // Estado inconsistente (ej: duplicado, concurrencia)
    /// <summary>The operation is not valid in the current state.</summary>
    InvalidOperation,  // Estado incorrecto para ejecutar la acción
    /// <summary>The operation is not supported in this context.</summary>
    NotSupported,      // Operación no soportada en este contexto

    // Ejecución
    /// <summary>The operation exceeded the time limit.</summary>
    Timeout,           // Operación excedió tiempo límite
    /// <summary>The operation was cancelled (token, user, etc.).</summary>
    Cancelled,         // Operación abortada (token de cancelación, usuario, etc.)

    // Otros
    /// <summary>Unspecified or unknown error.</summary>
    General,           // Error no especificado, desconocido
    /// <summary>Internal technical error (bug, unhandled exception).</summary>
    Internal,          // Error técnico interno (bug, excepción no manejada)
}