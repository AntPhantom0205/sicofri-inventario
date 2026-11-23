namespace Sicofri.API.Entities;

/// <summary>
/// Registro de un traslado: cuántos kg pasaron de un lote (origen) a otro
/// (destino), quién y cuándo.
/// </summary>

public class MovimientoTraslado
{
    public int IdMovimiento { get; set; }
    public decimal Cantidad { get; set; }

    /// <summary>Fecha y hora exacta, guardada en UTC.</summary>
    public DateTime FechaHora { get; set; }

    public int IdProductoOrigen { get; set; }
    public Producto ProductoOrigen { get; set; } = null!;

    public int IdProductoDestino { get; set; }
    public Producto ProductoDestino { get; set; } = null!;

    public int IdUbicacionOrigen { get; set; }
    public Ubicacion UbicacionOrigen { get; set; } = null!;

    public int IdUbicacionDestino { get; set; }
    public Ubicacion UbicacionDestino { get; set; } = null!;

    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

}//Fin de la clase