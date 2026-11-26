using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Notificaciones;

namespace Sicofri.API.Services.Interfaces;

public interface INotificacionService
{
    Task<ResultadoRevisionDto> RevisarAsync();
    Task<ResultadoPaginado<NotificacionDto>> BuscarAsync(bool? atendida, int pagina, int tamanoPagina);
    Task<int> ContarPendientesAsync();
    Task<NotificacionDto> AtenderAsync(int idNotificacion, int idUsuario);
}//Fin de la interfaz INotificacionService