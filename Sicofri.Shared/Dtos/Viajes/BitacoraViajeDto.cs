using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Viajes;

/// <summary>
/// Un viaje en la bitacora de entregas: quien lo programo, quienes lo hicieron,
/// cuando salio, cuando termino, cuanto duro y como termino cada pedido.
/// </summary>
public record BitacoraViajeDto(
    int IdViaje,
    string Numero,
    DateOnly Fecha,
    EstadoViaje Estado,
    List<string> Encargados,
    string CreadoPor,
    DateTimeOffset? FechaHoraInicio,
    DateTimeOffset? FechaHoraFin,
    int? DuracionMinutos,
    int TotalPedidos,
    int PedidosEntregados,
    decimal TotalKg,
    List<BitacoraPedidoDto> Pedidos); //Fin de la clase BitacoraViajeDto