using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Viajes;

/// <summary>
/// Detalle completo de un viaje: encargados, pedidos y su avance.
/// </summary>
public record ViajeDto(
    int IdViaje,
    string Numero,
    DateOnly Fecha,
    EstadoViaje Estado,
    List<EncargadoDto> Encargados,
    DateTimeOffset? FechaHoraInicio,
    DateTimeOffset? FechaHoraFin,
    string CreadoPor,
    int TotalPedidos,
    int PedidosEntregados,
    decimal TotalKg,
    List<PedidoEnViajeDto> Pedidos); //Fin de la clase ViajeDto