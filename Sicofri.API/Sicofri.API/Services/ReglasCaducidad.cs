using Sicofri.API.Entities;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Services;

/// <summary>
/// Reglas de caducidad definidas con Pescaderia Calimar.
/// </summary>
public class ReglasCaducidad
{
    /// <summary>Un producto fresco se consume en 3 dias.</summary>
    public const int DiasVidaFresco = 3;

    /// <summary>Los frescos se avisan 1 día antes; los demas usan los dias configurables.</summary>
    public const int DiasAvisoFresco = 1;

    /// <summary>La fecha de "hoy" en Costa Rica, sin importar la zona horaria del servidor que se escoga.</summary>
    public static DateOnly HoyEnCostaRica()
    {
        DateTime ahora = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "America/Costa_Rica");
        return DateOnly.FromDateTime(ahora);
    }

    public static int DiasDeAviso(TipoLote tipo, int diasConfigurados)
    {
        return tipo == TipoLote.Fresco ? DiasAvisoFresco : diasConfigurados;
    }

    /// <summary>
    /// Un lote esta "proximo a caducar" si todavia tiene existencia y le quedan
    /// tantos dias o menos como su limite de aviso (incluye los ya vencidos).
    /// </summary>
    public static bool EstaProximoACaducar(Producto producto, DateOnly hoy, int diasConfigurados)
    {
        int diasRestantes = producto.FechaCaducidad.DayNumber - hoy.DayNumber;
        return producto.Cantidad > 0 && diasRestantes <= DiasDeAviso(producto.TipoLote, diasConfigurados);
    }//Fin del metodo EstaProximoACaducar

}//Fin de la clase ReglasCaducidad
