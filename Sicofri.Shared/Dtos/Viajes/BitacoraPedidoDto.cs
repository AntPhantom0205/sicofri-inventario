using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Viajes;

/// <summary>Un pedido dentro de la bitacora: a quien iba, como termino
/// y quien lo entrego.</summary>
public record BitacoraPedidoDto(
    int IdPedido,
    string Numero,
    string Cliente,
    string ProvinciaCliente,
    EstadoPedido Estado,
    decimal TotalKg,
    DateTimeOffset? FechaHoraEntregado,
    string? EntregadoPor); //Fin de la clase BitacoraPedidoDto