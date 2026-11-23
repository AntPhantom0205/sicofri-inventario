namespace Sicofri.API.Security;

/// <summary>
/// Configuración del token JWT. Se llena automáticamente con la sección "Jwt"
/// de appsettings.json y de los secretos de usuario.
/// </summary>

public class JwtSettings
{
    public const string Seccion = "Jwt";

    public string Clave { get; set; } = string.Empty;
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public int MinutosExpiracion { get; set; } = 480;

}//Fin de la clase
