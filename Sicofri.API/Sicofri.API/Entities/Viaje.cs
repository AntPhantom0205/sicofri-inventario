using Sicofri.Shared.Enums;

namespace Sicofri.API.Entities;

/// <summary>
/// Ruta de entrega que agrupa varios pedidos, con 1 a 3 empleados encargados.
/// </summary>
public class Viaje
{
    public int IdViaje { get; set; }
    public DateOnly Fecha { get; set; }
    public EstadoViaje Estado { get; set; } = EstadoViaje.Programado;

    /// <summary>Hora real en que los encargados iniciaron el viaje (UTC).</summary>
    public DateTime? FechaHoraInicio { get; set; }

    /// <summary>Hora en que se entrego el ultimo pedido (UTC).</summary>
    public DateTime? FechaHoraFin { get; set; }

    public int IdUsuarioCreador { get; set; }
    public Usuario UsuarioCreador { get; set; } = null!;

    public ICollection<ViajeEncargado> Encargados { get; set; } = new List<ViajeEncargado>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}//Fin de la clase Viaje