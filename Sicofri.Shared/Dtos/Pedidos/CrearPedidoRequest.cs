namespace Sicofri.Shared.Dtos.Pedidos;

/// <summary>
/// Datos de la pantalla "Crear pedido". El creador se toma del token.
/// </summary>
public record CrearPedidoRequest(
    int IdCliente,
    DateOnly FechaEntrega,
    List<LineaPedidoRequest> Lineas); //Fin de la clase CrearPedidoRequest