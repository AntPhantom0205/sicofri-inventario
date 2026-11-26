using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Pedidos;

/// <summary>
/// Detalle completo de un pedido: encabezado, lineas y rastro
/// (quién lo creo, entrego o cancelo, y cuando). para eso se necesita 
/// una bitacora de pedidos.
/// </summary>
public record PedidoDto(
    int IdPedido,
    string Numero,
    EstadoPedido Estado,
    int IdCliente,
    string Cliente,
    string ProvinciaCliente,
    DateOnly FechaEntrega,
    int? IdViaje,
    decimal TotalKg,
    DateTimeOffset FechaHoraCreacion,
    string CreadoPor,
    DateTimeOffset? FechaHoraEntregado,
    string? EntregadoPor,
    DateTimeOffset? FechaHoraCancelado,
    string? CanceladoPor,
    List<LineaPedidoDto> Lineas); //Fin de la clase PedidoDto