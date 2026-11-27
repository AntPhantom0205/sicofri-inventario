namespace Sicofri.Shared.Dtos.Viajes;

/// <summary>
/// Cambios a un viaje Programado. Los encargados y pedidos enviados REEMPLAZAN 
/// a los anteriores:
/// los pedidos que se quitan quedan libres para otro viaje.
/// </summary>
public record ActualizarViajeRequest(
    DateOnly Fecha,
    List<int> IdsEncargados,
    List<int> IdsPedidos); //Fin de la clase ActualizarViajeRequest