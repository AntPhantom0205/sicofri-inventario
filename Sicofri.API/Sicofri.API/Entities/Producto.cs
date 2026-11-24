using Sicofri.Shared.Enums;

namespace Sicofri.API.Entities;

/// <summary>
/// Un lote de producto congelado en una ubicación específica.
/// Si se traslada una parte, se crea un lote nuevo en el destino con las mismas
/// fechas.
/// </summary>

public class Producto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreProveedor { get; set; } = string.Empty;

    /// <summary>Empacado, Fresco o CongeladoPropio.</summary>
    public TipoLote TipoLote { get; set; }

    /// <summary>Número de lote impreso por el proveedor. Obligatorio en los empacados.</summary>
    public string? CodigoLote { get; set; }

    /// <summary>Cantidad disponible en kilogramos.</summary>
    public decimal Cantidad { get; set; }

    public DateOnly FechaIngreso { get; set; }
    public DateOnly FechaProduccion { get; set; }
    public DateOnly FechaCaducidad { get; set; }

    public int IdUbicacion { get; set; }
    public Ubicacion Ubicacion { get; set; } = null!;

    public ICollection<LineaPedido> LineasPedido { get; set; } = new List<LineaPedido>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

}//Fin de la clase
