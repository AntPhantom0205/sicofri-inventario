using Microsoft.EntityFrameworkCore.Storage;
using Sicofri.API.Entities;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Repositories.Interfaces;

/// <summary>
/// Acceso a pedidos y a las operaciones de inventario que generan.
/// </summary>
public interface IPedidoRepository
{
    Task<Pedido?> ObtenerPorIdAsync(int idPedido);
    Task<(List<Pedido> Pedidos, int Total)> BuscarAsync(
        EstadoPedido? estado, int? idPedido, string? nombreCliente, int saltar, int tomar);
    Task<Cliente?> ObtenerClienteAsync(int idCliente);
    Task<List<Producto>> ObtenerLotesAsync(IEnumerable<int> idsProductos);
    Task<bool> DescontarKilosAsync(int idProducto, decimal cantidad);
    Task DevolverKilosAsync(int idProducto, decimal cantidad);
    Task AgregarAsync(Pedido pedido);
    void EliminarLineas(IEnumerable<LineaPedido> lineas);
    Task<IDbContextTransaction> IniciarTransaccionAsync();
    Task GuardarCambiosAsync();
}//Fin del interfaz IPedidoRepository