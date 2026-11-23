using Sicofri.API.Entities;
using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Security;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Usuarios;

namespace Sicofri.API.Services;

/// <summary>
/// Reglas de negocio para la administración de cuentas de usuario.
/// </summary>

public class UsuarioService : IUsuarioService
{
    private const int LargoMinimoContrasena = 8;

    private readonly IUsuarioRepository _usuarios;
    private readonly PasswordHasher _hasher;

    public UsuarioService(IUsuarioRepository usuarios, PasswordHasher hasher)
    {
        _usuarios = usuarios;
        _hasher = hasher;
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest datos)
    {
        string nombre = datos.Nombre?.Trim() ?? string.Empty;
        string nombreUsuario = datos.NombreUsuario?.Trim().ToLowerInvariant() ?? string.Empty;

        if (nombre.Length == 0 || nombre.Length > 60)
            throw new ReglaNegocioException("El nombre completo es obligatorio y debe tener máximo 60 caracteres.");

        if (nombreUsuario.Length == 0 || nombreUsuario.Length > 30 || nombreUsuario.Contains(' '))
            throw new ReglaNegocioException("El nombre de usuario es obligatorio, sin espacios y de máximo 30 caracteres.");

        if (string.IsNullOrEmpty(datos.Contrasena) || datos.Contrasena.Length < LargoMinimoContrasena)
            throw new ReglaNegocioException($"La contraseña debe tener al menos {LargoMinimoContrasena} caracteres.");

        if (!Enum.IsDefined(datos.Rol))
            throw new ReglaNegocioException("El rol seleccionado no es válido.");

        if (await _usuarios.ExisteNombreUsuarioAsync(nombreUsuario))
            throw new ReglaNegocioException("Ya existe un usuario con ese nombre de usuario.", StatusCodes.Status409Conflict);

        var usuario = new Usuario
        {
            Nombre = nombre,
            NombreUsuario = nombreUsuario,
            ContrasenaHash = _hasher.Encriptar(datos.Contrasena),
            Rol = datos.Rol,
            Activo = true
        };

        await _usuarios.AgregarAsync(usuario);
        await _usuarios.GuardarCambiosAsync();

        return ConvertirADto(usuario);
    }//Fin del método CrearAsync

    public async Task<List<UsuarioDto>> ObtenerTodosAsync()
    {
        var usuarios = await _usuarios.ObtenerTodosAsync();
        return usuarios.Select(ConvertirADto).ToList();
    }//Fin del metodo ObtenerTodosAsync

    private static UsuarioDto ConvertirADto(Usuario usuario)
    {
        return new UsuarioDto(usuario.IdUsuario, usuario.Nombre, usuario.NombreUsuario, usuario.Rol, usuario.Activo);
    }//Fin del metodo ConvertirADto

}//Fin de la clase