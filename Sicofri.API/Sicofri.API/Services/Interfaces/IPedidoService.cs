using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Pedidos;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Services.Interfaces;

public interface IPedidoService
{
    Task<ResultadoPaginado<PedidoResumenDto>> BuscarAsync(EstadoPedido? estado, string? busqueda, int pagina, int tamanoPagina);
    Task<PedidoDto> ObtenerPorIdAsync(int idPedido);
    Task<PedidoDto> CrearAsync(CrearPedidoRequest datos, int idUsuario);
    Task<PedidoDto> ActualizarAsync(int idPedido, ActualizarPedidoRequest datos);
    Task<PedidoDto> CancelarAsync(int idPedido, int idUsuario);
}//Fin de la interfaz IPedidoService