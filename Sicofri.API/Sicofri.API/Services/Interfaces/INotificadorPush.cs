namespace Sicofri.API.Services.Interfaces;

/// <summary>
/// Envia avisos push a los celulares. La implementacion real usa Firebase 
/// Cloud Messaging.
/// </summary>
public interface INotificadorPush
{
    /// <summary>False si Firebase no esta configurado: en ese caso no 
    /// se envia nada.</summary>
    bool EstaDisponible { get; }

    /// <summary>
    /// Envia el mismo aviso a varios celulares.
    /// Devuelve los tokens que Firebase rechazo por invalidos 
    /// (app desinstalada, token vencido),
    /// para que se puedan borrar de la base de datos.
    /// </summary>
    Task<List<string>> EnviarAsync(
        IReadOnlyList<string> tokens,
        string titulo,
        string cuerpo,
        IReadOnlyDictionary<string, string>? datos = null);
}//Fin de la interfaz INotificadorPush