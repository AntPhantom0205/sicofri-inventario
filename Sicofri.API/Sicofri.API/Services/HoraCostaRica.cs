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
}//Fin de la clase HoraCostaRica