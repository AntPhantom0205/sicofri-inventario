using Sicofri.Shared.Enums;

namespace Sicofri.API.Entities;

/// <summary>
/// Ruta de entrega que agrupa varios pedidos, con un empleado encargado.
/// </summary>

public class Viaje
{
    public int IdViaje { get; set; }
    public DateOnly Fecha { get; set; }
    public EstadoViaje Estado { get; set; } = EstadoViaje.Programado;

    public int IdUsuarioEncargado { get; set; }
    public Usuario UsuarioEncargado { get; set; } = null!;
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

}//Fin de la clase
