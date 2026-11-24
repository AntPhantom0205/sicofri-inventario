using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sicofri.Shared.Dtos;

namespace Sicofri.API.Security;

/// <summary>
/// Respuestas claras para los errores de autenticación (401) y autorización (403).
/// Sin esto, ASP.NET responde con el cuerpo vacío y no se sabe qué falló.
/// </summary>
public static class EventosJwt
{
    public static JwtBearerEvents Crear()
    {
        return new JwtBearerEvents
        {
            // Se ejecuta cuando llega un token pero es rechazado: se anota el motivo en la consola.
            OnAuthenticationFailed = contexto =>
            {
                var logger = contexto.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Sicofri.Jwt");

                logger.LogWarning("Token rechazado en {Ruta}: {Motivo}",
                    contexto.Request.Path, contexto.Exception.Message);

                return Task.CompletedTask;
            },

            // 401: no hay token, o el token no es válido.
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();

                string mensaje = contexto.AuthenticateFailure switch
                {
                    null => "Debe iniciar sesión para usar este servicio.",
                    SecurityTokenExpiredException => "La sesión expiró. Inicie sesión de nuevo.",
                    SecurityTokenInvalidSignatureException =>
                        "El token no es válido: su firma no coincide. Puede ser un token viejo o la clave JWT cambió. Inicie sesión de nuevo.",
                    SecurityTokenMalformedException =>
                        "El token está mal formado. Copie solo el token, sin comillas y sin la palabra 'Bearer'.",
                    _ => "El token no es válido. Inicie sesión de nuevo."
                };

                var entorno = contexto.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
                string? detalle = entorno.IsDevelopment() && contexto.AuthenticateFailure is not null
                    ? $"{contexto.AuthenticateFailure.GetType().Name}: {contexto.AuthenticateFailure.Message}"
                    : null;

                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await contexto.Response.WriteAsJsonAsync(new ErrorResponse(mensaje, Detalle: detalle));
            },

            // 403: el token es válido, pero el rol no tiene permiso.
            OnForbidden = async contexto =>
            {
                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                await contexto.Response.WriteAsJsonAsync(
                    new ErrorResponse("No tiene permiso para realizar esta acción con su rol actual."));
            }
        };
    }//Fin del metodo Crear
}//Fin de la clae EventoJwt