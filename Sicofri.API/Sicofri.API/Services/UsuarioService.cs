using Sicofri.API.Entities;
using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Security;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Usuarios;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Services;

/// <summary>
/// Reglas de negocio para la administracion de cuentas.
/// Solo existe un Jefe (creado al iniciar el sistema); las demas cuentas
/// unicamente pueden ser Secretaria o Empleado.
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

    public async Task<List<UsuarioDto>> ObtenerTodosAsync()
    {
        var usuarios = await _usuarios.ObtenerActivosAsync();
        return usuarios.Select(ConvertirADto).ToList();
    }

    public async Task<UsuarioDto> ObtenerPorIdAsync(int idUsuario)
    {
        var usuario = await ObtenerActivoOFallarAsync(idUsuario);
        return ConvertirADto(usuario);
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest datos)
    {
        string nombre = ValidarNombre(datos.Nombre);
        string nombreUsuario = ValidarNombreUsuario(datos.NombreUsuario);
        ValidarContrasena(datos.Contrasena);
        ValidarRolAsignable(datos.Rol);

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
    }//Fin del metodo CrearAsync

    public async Task<UsuarioDto> ActualizarAsync(int idUsuario, ActualizarUsuarioRequest datos)
    {
        var usuario = await ObtenerActivoOFallarAsync(idUsuario);

        string nombre = ValidarNombre(datos.Nombre);
        string nombreUsuario = ValidarNombreUsuario(datos.NombreUsuario);

        if (usuario.Rol == Rol.Jefe)
        {
            // El Jefe puede cambiar su nombre, usuario y contrasena, pero nunca dejar de ser Jefe.
            if (datos.Rol != Rol.Jefe)
                throw new ReglaNegocioException("El rol del Jefe no se puede cambiar.");
        }
        else
        {
            ValidarRolAsignable(datos.Rol);
        }

        if (await _usuarios.ExisteNombreUsuarioAsync(nombreUsuario, excluirIdUsuario: idUsuario))
            throw new ReglaNegocioException("Ya existe un usuario con ese nombre de usuario.", StatusCodes.Status409Conflict);

        usuario.Nombre = nombre;
        usuario.NombreUsuario = nombreUsuario;
        usuario.Rol = datos.Rol;

        // Si no se envia contrasena nueva, se conserva la actual.
        if (!string.IsNullOrEmpty(datos.NuevaContrasena))
        {
            ValidarContrasena(datos.NuevaContrasena);
            usuario.ContrasenaHash = _hasher.Encriptar(datos.NuevaContrasena);
        }

        await _usuarios.GuardarCambiosAsync();

        return ConvertirADto(usuario);
    }//Fin del metodo ActualizarAsync

    /// <summary>
    /// "Elimina" al usuario desactivandolo: ya no aparece en la lista ni puede iniciar sesión,
    /// pero su nombre se conserva en el historial (traslados, viajes) para no perder la trazabilidad.
    /// </summary>
    public async Task EliminarAsync(int idUsuario)
    {
        var usuario = await ObtenerActivoOFallarAsync(idUsuario);

        if (usuario.Rol == Rol.Jefe)
            throw new ReglaNegocioException("La cuenta del Jefe no se puede eliminar.");

        usuario.Activo = false;
        usuario.TokenFcm = null;

        await _usuarios.GuardarCambiosAsync();
    }//Fin del metodo EliminarAsync

    // ---------------- Validaciones compartidas ----------------

    private async Task<Usuario> ObtenerActivoOFallarAsync(int idUsuario)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(idUsuario);

        if (usuario is null || !usuario.Activo)
            throw new ReglaNegocioException("El usuario no existe.", StatusCodes.Status404NotFound);

        return usuario;
    }//Fin del metodo ObtenerActivoOFallarAsync

    private static string ValidarNombre(string? nombre)
    {
        string limpio = nombre?.Trim() ?? string.Empty;

        if (limpio.Length == 0 || limpio.Length > 60)
            throw new ReglaNegocioException("El nombre completo es obligatorio y debe tener máximo 60 caracteres.");

        return limpio;
    }//Fin del metodo ValidarNombre

    private static string ValidarNombreUsuario(string? nombreUsuario)
    {
        string limpio = nombreUsuario?.Trim().ToLowerInvariant() ?? string.Empty;

        if (limpio.Length == 0 || limpio.Length > 30 || limpio.Contains(' '))
            throw new ReglaNegocioException("El nombre de usuario es obligatorio, sin espacios y de máximo 30 caracteres.");

        return limpio;
    }//Fin del metodo ValidarNombreUsuario

    private static void ValidarContrasena(string? contrasena)
    {
        if (string.IsNullOrEmpty(contrasena) || contrasena.Length < LargoMinimoContrasena)
            throw new ReglaNegocioException($"La contraseña debe tener al menos {LargoMinimoContrasena} caracteres.");
    }//Fin de la funcion ValidarContrasena

    /// <summary>Solo se pueden asignar los roles Secretaria o Empleado. El Jefe es unico.</summary>
    private static void ValidarRolAsignable(Rol rol)
    {
        if (rol == Rol.Jefe)
            throw new ReglaNegocioException("No se pueden asignar el rol Jefe. Solo existe un Jefe en el sistema.");

        if (!Enum.IsDefined(rol))
            throw new ReglaNegocioException("El rol seleccionado no es valido. Debe ser Secretaria o Empleado.");
    }//Fin del metodo ValidarRolAsignable

    private static UsuarioDto ConvertirADto(Usuario usuario)
    {
        return new UsuarioDto(usuario.IdUsuario, usuario.Nombre, usuario.NombreUsuario, usuario.Rol, usuario.Activo);
    }//Fin del metodo ConvertirADto

}//Fin de la clase