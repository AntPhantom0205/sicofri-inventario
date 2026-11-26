namespace Sicofri.Shared.Dtos.Pedidos;

/// <summary>
/// Correccion de un pedido Armado (Jefe o Secretaria). Las lineas enviadas
/// REEMPLAZAN a las anteriores: se devuelven los kilos viejos y se descuentan 
/// los nuevos.
/// </summary>
public record ActualizarPedidoRequest(
    int IdCliente,
    DateOnly FechaEntrega,
    List<LineaPedidoRequest> Lineas); //Fin de la clase ActualizarPedidoRequest