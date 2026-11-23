using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Usuarios;

/// <summary>
/// Información pública de un usuario. Nunca incluye la contraseña.
/// </summary>
public record UsuarioDto(
    int IdUsuario,
    string Nombre,
    string NombreUsuario,
    Rol Rol,
    bool Activo);