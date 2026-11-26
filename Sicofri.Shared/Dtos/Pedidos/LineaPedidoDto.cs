namespace Sicofri.Shared.Dtos.Pedidos;

/// <summary>
/// Una linea del pedido con los datos de su lote.
/// </summary>
public record LineaPedidoDto(
    int IdDetalle,
    int IdProducto,
    string Producto,
    string? CodigoLote,
    decimal Cantidad,
    string UbicacionOrigen,
    DateOnly FechaCaducidad); //Fin de la clase LineaPedidoDto