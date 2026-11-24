using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Usuarios;

/// <summary>
/// Datos para actualizar un usuario (solo Jefe).
/// Si NuevaContrasena viene vacía, se conserva la contraseña actual.
/// </summary>

public record ActualizarUsuarioRequest(
    string Nombre,
    string NombreUsuario,
    string? NuevaContrasena,
    Rol Rol);