using System.Security.Claims;
using Sicofri.API.Exceptions;

namespace Sicofri.API.Security;

/// <summary>Atajos para leer los datos del usuario que vienen dentro del token.</summary>
public static class ClaimsPrincipalExtensiones
{
    public static int ObtenerIdUsuario(this ClaimsPrincipal usuario)
    {
        string? valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(valor, out int idUsuario))
            return idUsuario;

        throw new ReglaNegocioException(
            "No se pudo identificar al usuario de la sesión. Inicie sesión de nuevo.",
            StatusCodes.Status401Unauthorized);
    }//Fin del metodo ObtenerIdUsuario
}//Fin de la clase ClaimsPrincipalExtensiones