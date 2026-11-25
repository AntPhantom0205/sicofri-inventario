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
    }//Fin del metodo IniciarSessionAsync

    /// <summary>
    /// Guarda el token de Firebase del celular del usuario.
    /// Si el token es null, se borra (cierre de sesion).
    /// </summary>
    public async Task RegistrarDispositivoAsync(int idUsuario, string? tokenFcm)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(idUsuario);

        if (usuario is null || !usuario.Activo)
            throw new ReglaNegocioException("El usuario de la sesión no existe o está inactivo.", StatusCodes.Status401Unauthorized);

        string? token = string.IsNullOrWhiteSpace(tokenFcm) ? null : tokenFcm.Trim();

        if (token is { Length: > 500 })
            throw new ReglaNegocioException("El token del dispositivo no es válido.");

        if (token is not null)
            await _usuarios.QuitarTokenDeOtrosUsuariosAsync(token, idUsuario);

        usuario.TokenFcm = token;
        await _usuarios.GuardarCambiosAsync();
    }//Fin del metodo RegistrarDispositivoAsync
}//Fin de la clase AuthService