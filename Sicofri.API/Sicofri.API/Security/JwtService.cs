using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Sicofri.API.Entities;

namespace Sicofri.API.Security;

/// <summary>
/// Fabrica los tokens JWT que la app usa para identificarse ante la API.
/// </summary>

public class JwtService
{
    private readonly JwtSettings _config;

    public JwtService(IOptions<JwtSettings> opciones)
    {
        _config = opciones.Value;
    }
    /// <summary>
    /// Crea un token con la identidad y el rol del usuario.
    /// Devuelve el token y el momento exacto en que expira (en UTC).
    /// </summary>

    public (string Token, DateTime Expira) GenerarToken(Usuario usuario)
    {
        DateTime expira = DateTime.UtcNow.AddMinutes(_config.MinutosExpiracion);

        // Los "claims" son los datos que viajan dentro del token.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Clave));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expira,
            Issuer = _config.Emisor,
            Audience = _config.Audiencia,
            SigningCredentials = credenciales
        };

        string token = new JsonWebTokenHandler().CreateToken(descriptor);

        return (token, expira);
    }//Fin del metodo GenerarToken
}//Fin de la clase