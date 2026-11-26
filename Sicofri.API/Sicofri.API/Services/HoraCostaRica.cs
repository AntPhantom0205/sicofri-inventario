namespace Sicofri.API.Services;

/// <summary>
/// Conversiones entre la hora UTC (como se guarda en la base) y la hora de Costa Rica.
/// </summary>
public static class HoraCostaRica
{
    private static readonly TimeZoneInfo Zona = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");

    /// <summary>Medianoche de esa fecha en Costa Rica, expresada en UTC (para filtrar en la base).</summary>
    public static DateTime InicioDelDiaUtc(DateOnly fecha)
    {
        return TimeZoneInfo.ConvertTimeToUtc(fecha.ToDateTime(TimeOnly.MinValue), Zona);
    }//Fin del metodo InicioDelDiaUtc

    /// <summary>Convierte una hora UTC guardada en la base a hora de Costa Rica.</summary>
    public static DateTimeOffset ALocal(DateTime fechaUtc)
    {
        var utc = new DateTimeOffset(DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc));
        return TimeZoneInfo.ConvertTime(utc, Zona);
    }//Fin del metodo ALocal

    /// <summary>
    /// Cuanto tiempo falta para la proxima vez que el reloj de Costa Rica marque esa hora.
    /// Ej.: a las 22:00, faltan 8 horas para las 06:00 (del dia siguiente).
    /// </summary>
    public static TimeSpan TiempoHasta(TimeOnly hora)
    {
        DateTime ahora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zona);
        DateTime proxima = ahora.Date + hora.ToTimeSpan();

        if (proxima <= ahora)
            proxima = proxima.AddDays(1);

        return proxima - ahora;
    }//Fin del metodo TiempoHasta
}//Fin de la clase HoraCostaRica