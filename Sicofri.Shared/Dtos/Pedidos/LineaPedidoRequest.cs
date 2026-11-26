namespace Sicofri.Shared.Dtos.Pedidos;

/// <summary>
/// Una linea al armar o actualizar un pedido: de que lote y cuantos kilos.
/// </summary>
public record LineaPedidoRequest(int IdProducto, decimal Cantidad); 
//Fin de la clase LineaPedidoRequest