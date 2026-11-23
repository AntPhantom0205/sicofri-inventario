using Sicofri.API.Entities;

namespace Sicofri.API.Repositories.Interfaces;

/// <summary>Acceso a la tabla de usuarios.</summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<List<Usuario>> ObtenerTodosAsync();
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);
    Task<bool> ExisteAlgunoAsync();
    Task AgregarAsync(Usuario usuario);
    Task GuardarCambiosAsync();

}//Fin de la interfaz
