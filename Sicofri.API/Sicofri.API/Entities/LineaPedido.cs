namespace Sicofri.API.Entities;

/// <summary>
/// Una linea del pedido: cuantos kg se tomaron de cual lote.
/// La ubicacion de origen es la ubicacion de ese lote.
/// </summary>

public class LineaPedido
{
    public int IdDetalle { get; set; }
    public decimal Cantidad { get; set; }

    public int IdPedido { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public int IdProducto { get; set; }
    public Producto Producto { get; set; } = null!;

}//Fin de la clase
