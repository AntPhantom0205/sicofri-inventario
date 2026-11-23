using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Auth;

/// <summary>
/// Respuesta de un inicio de sesión exitoso: 
/// el token y los datos básicos del usuario.
/// </summary>
public record LoginResponse(
     string Token,
    DateTime Expira,
    int IdUsuario,
    string Nombre,
    string NombreUsuario,
    Rol Rol);
