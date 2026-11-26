using Sicofri.API.Entities;

namespace Sicofri.API.Repositories.Interfaces;

/// <summary>
/// Acceso a las notificaciones de caducidad y a los tokens de los celulares.
/// </summary>

public interface INotificacionRepository
{
    Task<HashSet<int>> ObtenerIdsProductosNotificadosAsync();
    Task<List<Notificacion>> ObtenerPendientesDeLotesAgotadosAsync();
    Task<(List<Notificacion> Notificaciones, int Total)> BuscarAsync(bool? atendida, int saltar, int tomar);
    Task<Notificacion?> ObtenerPorIdAsync(int idNotificacion);
    Task<int> ContarPendientesAsync();
    Task<List<string>> ObtenerTokensDelJefeAsync();
    Task QuitarTokensAsync(List<string> tokens);
    Task AgregarAsync(Notificacion notificacion);
    Task GuardarCambiosAsync();
}//Fin de la interfaz INotificacionRepository