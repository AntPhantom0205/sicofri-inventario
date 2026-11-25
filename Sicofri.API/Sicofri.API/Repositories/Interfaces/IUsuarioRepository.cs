using Sicofri.API.Entities;

namespace Sicofri.API.Repositories.Interfaces;

/// <summary>Acceso a la tabla de usuarios.</summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int idUsuario);
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);
    Task<List<Usuario>> ObtenerActivosAsync();
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirIdUsuario = null);
    Task<bool> ExisteAlgunoAsync();
    Task AgregarAsync(Usuario usuario);
    Task GuardarCambiosAsync();
    Task QuitarTokenDeOtrosUsuariosAsync(string tokenFcm, int idUsuario);

}//Fin de la interfaz
