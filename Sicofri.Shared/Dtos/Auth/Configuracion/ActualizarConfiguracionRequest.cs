namespace Sicofri.Shared.Dtos.Configuracion;

/// <summary>
/// Cambio de los dias de anticipacion para avisar caducidades (solo Jefe).
/// </summary>
public record ActualizarConfiguracionRequest(int DiasAnticipacion);
//Fin de la clase ActualizarConfiguracionRequest