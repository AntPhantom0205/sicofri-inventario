using Sicofri.Shared.Dtos.Pedidos;
using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Viajes;

/// <summary>
/// Un pedido dentro de un viaje, con todo lo que el empleado 
/// necesita para entregarlo:
/// donde queda el cliente, a quien llamar y que lleva.
/// </summary>
public record PedidoEnViajeDto(
    int IdPedido,
    string Numero,
    EstadoPedido Estado,
    string Cliente,
    string ProvinciaCliente,
    string UbicacionDetalle,
    string? Telefono,
    string? NombreContacto,
    DateOnly FechaEntrega,
    decimal TotalKg,
    DateTimeOffset? FechaHoraEntregado,
    string? EntregadoPor,
    List<LineaPedidoDto> Lineas); //Fin de la clase PedidoEnViajeDto