namespace Sicofri.Shared.Dtos.Configuracion;

/// <summary>
/// Reglas de caducidad vigentes. Solo DiasAnticipacion se puede cambiar;
/// las reglas de los productos frescos son fijas (definidas con Calimar).
/// </summary>
public record ConfiguracionDto(
    int DiasAnticipacion,
    int DiasAvisoFresco,
    int DiasVidaFresco);//Fin del metodo ConfiguracionDto