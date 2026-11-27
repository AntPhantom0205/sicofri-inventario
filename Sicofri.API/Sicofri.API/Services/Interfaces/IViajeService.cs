using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Viajes;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Services.Interfaces;

public interface IViajeService
{
    Task<ResultadoPaginado<ViajeResumenDto>> BuscarAsync(EstadoViaje? estado, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina);
    Task<ViajeDto> ObtenerPorIdAsync(int idViaje);
    Task<ViajeDto> CrearAsync(CrearViajeRequest datos, int idUsuario);
    Task<ViajeDto> ActualizarAsync(int idViaje, ActualizarViajeRequest datos);
    Task EliminarAsync(int idViaje);
    Task<ViajeDto> IniciarAsync(int idViaje, int idUsuario, bool esJefe);
    Task<ViajeDto> EntregarPedidoAsync(int idViaje, int idPedido, int idUsuario, bool esJefe);
}//Fin de la Interfaz IViajeService