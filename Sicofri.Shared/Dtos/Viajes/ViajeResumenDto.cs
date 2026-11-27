using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Viajes;

/// <summary>
/// Una fila de la lista de viajes, sin el detalle de cada pedido.
/// </summary>
public record ViajeResumenDto(
    int IdViaje,
    string Numero,
    DateOnly Fecha,
    EstadoViaje Estado,
    List<string> Encargados,
    int TotalPedidos,
    int PedidosEntregados,
    decimal TotalKg); //Fin de la clase ViajeResumenDto