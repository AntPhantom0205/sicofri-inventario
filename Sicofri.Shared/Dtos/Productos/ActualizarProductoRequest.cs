using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Productos;

/// <summary>
/// Correccion de los datos de un lote Esto no incluye la ubicacion:
/// para cambiar un lote de lugar se usa un traslado, asi queda registrado en la bitacora.
/// </summary>

public record ActualizarProductoRequest(
    string Nombre,
    string NombreProveedor,
    TipoLote TipoLote,
    string? CodigoLote,
    decimal Cantidad,
    DateOnly FechaIngreso,
    DateOnly FechaProduccion,
    DateOnly? FechaCaducidad,
    int? MesesVidaUtil);//Fin de la clase ActualizarProductoRequest
