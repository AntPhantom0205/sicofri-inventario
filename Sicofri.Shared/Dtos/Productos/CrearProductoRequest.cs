using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Productos;

/// <summary>
/// Datos para registrar un lote. Segun el tipo:
/// - Empacado: CodigoLote y FechaCaducidad son obligatorios (vienen en la etiqueta).
/// - Fresco: la caducidad se calcula sola (producción + 3 dias).
/// - CongeladoPropio: MesesVidaUtil es obligatorio y la caducidad se calcula.
/// </summary>

public record CrearProductoRequest(
    string Nombre,
    string NombreProveedor,
    TipoLote TipoLote,
    string? CodigoLote,
    decimal Cantidad,
    int IdUbicacion,
    DateOnly FechaIngreso,
    DateOnly FechaProduccion,
    DateOnly? FechaCaducidad,
    int? MesesVidaUtil);
//Fin de la clase CrearProductoRequest
