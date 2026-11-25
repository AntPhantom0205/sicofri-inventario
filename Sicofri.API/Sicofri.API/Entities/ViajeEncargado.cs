namespace Sicofri.API.Entities;

/// <summary>
/// Tabla intermedia: relaciona un viaje con cada uno de sus empleados encargados (de 1 a 3).
/// </summary>
public class ViajeEncargado
{
    public int IdViaje { get; set; }
    public Viaje Viaje { get; set; } = null!;

    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;
}//Fin de la clase ViajeEncargado