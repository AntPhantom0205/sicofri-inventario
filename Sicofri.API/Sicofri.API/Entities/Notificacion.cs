namespace Sicofri.API.Entities;

/// <summary>
/// Aviso generado cuando un lote está próximo a caducar. Existe una sola por lote.
/// Queda registrado quién la atendió y cuándo (Jefe o Secretaria).
/// </summary>
public class Notificacion
{
    public int IdNotificacion { get; set; }
    public DateOnly FechaGenerada { get; set; }
    public bool Atendida { get; set; }

    /// <summary>Hora en que se marcó como atendida (UTC). Vacía mientras está pendiente.</summary>
    public DateTime? FechaHoraAtendida { get; set; }

    /// <summary>
    /// Quien la atendio. Vacio si esta pendiente, o si el sistema lo cerro
    /// automaticamente porque el lote se agoto.
    /// </summary>
    public int? IdUsuarioAtiende { get; set; }
    public Usuario? UsuarioAtiende { get; set; }

    public int IdProducto { get; set; }
    public Producto Producto { get; set; } = null!;
}//Fin de la clase 