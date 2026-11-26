namespace Sicofri.Shared.Dtos.Notificaciones;

/// <summary>Resumen de lo que hizo una revision de caducidades.</summary>
public record ResultadoRevisionDto(
    int NotificacionesNuevas,
    int CerradasPorLoteAgotado,
    int AvisosPushEnviados,
    bool FirebaseDisponible); //Fin de la clase ResultadoRevisionDto