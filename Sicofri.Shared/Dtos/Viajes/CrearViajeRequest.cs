namespace Sicofri.Shared.Dtos.Viajes;

/// <summary>
/// Datos para programar un viaje: fecha, 1 a 3 empleados y los pedidos que lleva.
/// </summary>
public record CrearViajeRequest(
    DateOnly Fecha,
    List<int> IdsEncargados,
    List<int> IdsPedidos); //Fin de la clase CrearViajeRequest