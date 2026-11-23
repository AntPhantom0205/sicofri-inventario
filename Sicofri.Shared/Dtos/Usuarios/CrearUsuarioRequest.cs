using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Usuarios;

/// <summary>Datos del formulario "Crear nuevo usuario" (solo Jefe).</summary>
public record CrearUsuarioRequest(
    string Nombre,
    string NombreUsuario,
    string Contrasena,
    Rol Rol);