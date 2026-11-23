namespace Sicofri.API.Entities;

/// <summary>
/// Aviso generado cuando un lote está próximo a caducar.
/// </summary>

public class Notificacion
{
    public int IdNotificacion { get; set; }
    public DateOnly FechaGenerada { get; set; }
    public bool Atendida { get; set; }

    public int IdProducto { get; set; }
    public Producto Producto { get; set; } = null!;

}//Fin de la clase