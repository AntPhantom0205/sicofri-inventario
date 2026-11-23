namespace Sicofri.API.Entities;

/// <summary>
/// Parametros generales del sistema. La tabla tiene una sola fila.
/// </summary>

public class Configuracion
{
    public int IdConfiguracion { get; set; }

    /// <summary>Con cuantos dias de anticipacion se avisa una caducidad.</summary>
    public int DiasAnticipacion { get; set; } = 10;

}//Fin de la clase
