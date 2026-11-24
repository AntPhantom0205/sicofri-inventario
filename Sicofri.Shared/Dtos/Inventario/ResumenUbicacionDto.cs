using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Inventario;

/// <summary>Resumen de un congelador o camara para el dashboard de la app.</summary>
public record ResumenUbicacionDto(
    int IdUbicacion,
    string Nombre,
    Provincia Zona,
    int CantidadLotes,
    decimal TotalKg,
    int LotesProximosACaducar);
    //Fin de la clase ResumenUbicacionDto