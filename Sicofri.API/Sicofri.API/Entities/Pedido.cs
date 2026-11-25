using Sicofri.Shared.Enums;

namespace Sicofri.API.Entities;

/// <summary>
/// Conjunto de productos que se entregan a un cliente.
/// El inventario se descuenta al armarse y se devuelve si se cancela.
/// </summary>
public class Pedido
{
    public int IdPedido { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.Armado;

    /// <summary>Fecha programada de entrega.</summary>
    public DateOnly FechaEntrega { get; set; }

    public int IdCliente { get; set; }
    public Cliente Cliente { get; set; } = null!;

    /// <summary>Vacio mientras el pedido no se asigna a un viaje.</summary>
    public int? IdViaje { get; set; }
    public Viaje? Viaje { get; set; }

    //Datos para la bitacora

    public DateTime FechaHoraCreacion { get; set; }
    public int IdUsuarioCreador { get; set; }
    public Usuario UsuarioCreador { get; set; } = null!;

    public DateTime? FechaHoraEntregado { get; set; }
    public int? IdUsuarioEntrega { get; set; }
    public Usuario? UsuarioEntrega { get; set; }

    public DateTime? FechaHoraCancelado { get; set; }
    public int? IdUsuarioCancela { get; set; }
    public Usuario? UsuarioCancela { get; set; }

    public ICollection<LineaPedido> Lineas { get; set; } = new List<LineaPedido>();
}//Fin de la clase Pedido