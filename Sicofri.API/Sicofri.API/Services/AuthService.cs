using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Security;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Auth;

namespace Sicofri.API.Services;

/// <summary>Valida credenciales y entrega el token de acceso.</summary>
public class AuthService : IAuthService
{
    private const string MensajeCredencialesInvalidas = "Usuario o contraseña incorrectos.";

    private readonly IUsuarioRepository _usuarios;
    private readonly PasswordHasher _hasher;
    private readonly JwtService _jwt;

    public AuthService(IUsuarioRepository usuarios, PasswordHasher hasher, JwtService jwt)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginResponse> IniciarSesionAsync(LoginRequest datos)
    {
        if (string.IsNullOrWhiteSpace(datos.NombreUsuario) || string.IsNullOrWhiteSpace(datos.Contrasena))
        {
            throw new ReglaNegocioException("Debe ingresar el usuario y la contraseña.");
        }

        string nombreUsuario = datos.NombreUsuario.Trim().ToLowerInvariant();
        var usuario = await _usuarios.ObtenerPorNombreUsuarioAsync(nombreUsuario);

        // Mismo mensaje para todos los casos: no se revela si el usuario existe o no.
        if (usuario is null || !usuario.Activo || !_hasher.Verificar(datos.Contrasena, usuario.ContrasenaHash))
        {
            throw new ReglaNegocioException(MensajeCredencialesInvalidas, StatusCodes.Status401Unauthorized);
        }

        var (token, expira) = _jwt.GenerarToken(usuario);

        return new LoginResponse(token, expira, usuario.IdUsuario, usuario.Nombre, usuario.NombreUsuario, usuario.Rol);
    }
}