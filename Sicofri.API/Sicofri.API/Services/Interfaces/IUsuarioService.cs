using Sicofri.Shared.Dtos.Usuarios;

namespace Sicofri.API.Services.Interfaces;

public interface IUsuarioService
{
    Task<UsuarioDto> CrearAsync(CrearUsuarioRequest datos);
    Task<List<UsuarioDto>> ObtenerTodosAsync();

}//Fin de la interfaz