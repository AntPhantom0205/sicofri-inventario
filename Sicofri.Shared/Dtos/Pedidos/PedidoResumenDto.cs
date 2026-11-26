using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Pedidos;

/// <summary>
/// Una tarjeta de la lista de pedidos (pantalla 4.3), sin el detalle de lineas.
/// </summary>
public record PedidoResumenDto(
    int IdPedido,
    string Numero,
    EstadoPedido Estado,
    string Cliente,
    DateOnly FechaEntrega,
    int? IdViaje,
    decimal TotalKg,
    int CantidadLineas); //Fin de la clase PedidoResumenDto