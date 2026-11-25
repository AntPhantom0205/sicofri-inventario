namespace Sicofri.Shared.Dtos.Traslados;

/// <summary>
/// Un registro de la bitacora de traslados. Tambien es la respuesta
/// que muestra la pantalla "Traslado realizado".
/// </summary>
public record MovimientoDto(
    int IdMovimiento,
    DateTimeOffset FechaHora,
    string Producto,
    decimal Cantidad,
    string UbicacionOrigen,
    string UbicacionDestino,
    string UsuarioResponsable,
    int IdProductoOrigen,
    int IdProductoDestino); //Fin de la clase MovimientoDto