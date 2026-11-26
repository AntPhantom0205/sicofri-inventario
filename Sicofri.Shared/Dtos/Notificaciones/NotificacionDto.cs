using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Notificaciones;

/// <summary>
/// Una notificacion de caducidad con los datos del lote, para la campanita y la bitacora.
/// AtendidaPor: nombre de quien la atendio, "Sistema (lote agotado)" si se cerró sola, o null si esta pendiente.
/// </summary>

public record NotificacionDto(
    int IdNotificacion,
    DateOnly FechaGenerada,
    bool Atendida,
    DateTimeOffset? FechaHoraAtendida,
    string? AtendidaPor,
    int IdProducto,
    string Producto,
    TipoLote TipoLote,
    string Ubicacion,
    decimal Cantidad,
    DateOnly FechaCaducidad,
    int DiasParaCaducar);//Fin de la clase de NotificacionDto