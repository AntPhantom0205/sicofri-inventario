using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Productos;

/// <summary>Informacion de un lote tal como se ve en la app.</summary>
public record ProductoDto(
    int IdProducto,
    string Nombre,
    string NombreProveedor,
    TipoLote TipoLote,
    string? CodigoLote,
    decimal Cantidad,
    int IdUbicacion,
    string NombreUbicacion,
    DateOnly FechaIngreso,
    DateOnly FechaProduccion,
    DateOnly FechaCaducidad,
    int DiasParaCaducar,
    bool ProximoACaducar
    );//Fin de la clase ProductoDto
