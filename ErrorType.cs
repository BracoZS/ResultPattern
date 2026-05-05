namespace ResultPattern;

public enum ErrorType
{
    // Exito
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