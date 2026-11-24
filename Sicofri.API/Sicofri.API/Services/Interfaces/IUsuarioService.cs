using Sicofri.Shared.Dtos.Usuarios;

namespace Sicofri.API.Services.Interfaces;

public interface IUsuarioService
{
    Task<List<UsuarioDto>> ObtenerTodosAsync();
    Task<UsuarioDto> ObtenerPorIdAsync(int idUsuario);
    Task<UsuarioDto> CrearAsync(CrearUsuarioRequest datos);
    Task<UsuarioDto> ActualizarAsync(int idUsuario, ActualizarUsuarioRequest datos);
    Task EliminarAsync(int idUsuario);

}//Fin de la interfaz