using Sicofri.Shared.Enums;
namespace Sicofri.API.Entities;

/// <summary>
/// Lugar físico de almacenamiento: los 5 congeladores de Puntarenas o la cámara de San José.
/// </summary>

public class Ubicacion
{

    public int IdUbicacion { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Provincia Zona { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();

}//Fin de la clase
