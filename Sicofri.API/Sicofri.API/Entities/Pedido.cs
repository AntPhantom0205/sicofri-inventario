using Sicofri.Shared.Enums;
namespace Sicofri.API.Entities;

/// <summary>
/// Conjunto de productos que se entregan a un cliente.
/// El inventario se descuenta cuando el pedido se arma.
/// </summary>

public class Pedido
{
    public int IdPedido { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.Armado;
    public DateOnly FechaEntrega { get; set; }

    public int IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    /// <summary>
    /// Puede quedar vacío si el pedido aún no se asigna a un viaje.
    /// </summary>
    public int? IdViaje { get; set; }
    public Viaje? Viaje { get; set; }

    public ICollection<LineaPedido> Lineas { get; set; } = new List<LineaPedido>();

}//Fin de la clase