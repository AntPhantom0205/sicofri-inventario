using Sicofri.API.Exceptions;
using Sicofri.Shared.Dtos;

namespace Sicofri.API.Middleware;

/// <summary>
/// Atrapa cualquier error que ocurra al procesar una petición y lo convierte
/// en una respuesta JSON clara, en español, con el código HTTP correcto.
/// </summary>

public class ExceptionMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate siguiente, ILogger<ExceptionMiddleware> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (ReglaNegocioException ex)
        {
            // Error esperado del negocio: se le muestra el mensaje al usuario tal cual.
            await EscribirErrorAsync(contexto, ex.CodigoEstado, ex.Message);
        }
        catch (Exception ex)
        {
            // Error inesperado: se registra completo en la consola,
            // pero al usuario solo se le da un mensaje general (sin detalles internos).
            _logger.LogError(ex, "Error no controlado en {Ruta}", contexto.Request.Path);
            await EscribirErrorAsync(contexto, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado. Intente de nuevo más tarde.");
        }
    }//Fin del metodo InvokeAsync

    private static async Task EscribirErrorAsync(HttpContext contexto, int codigoEstado, string mensaje)
    {
        contexto.Response.StatusCode = codigoEstado;
        await contexto.Response.WriteAsJsonAsync(new ErrorResponse(mensaje));
    }//Fin del metodo EscribirErrorAsync
}//Fin de la clase