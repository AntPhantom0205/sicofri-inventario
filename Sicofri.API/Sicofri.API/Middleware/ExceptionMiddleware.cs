using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sicofri.API.Exceptions;
using Sicofri.Shared.Dtos;

namespace Sicofri.API.Middleware;

/// <summary>
/// Atrapa cualquier error al procesar una peticion y lo convierte en una respuesta JSON
/// clara, en espanol, con el codigo HTTP correcto. En desarrollo agrega el detalle tecnico.
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _entorno;

    public ExceptionMiddleware(RequestDelegate siguiente, ILogger<ExceptionMiddleware> logger, IHostEnvironment entorno)
    {
        _siguiente = siguiente;
        _logger = logger;
        _entorno = entorno;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (ReglaNegocioException ex)
        {
            await EscribirAsync(contexto, ex.CodigoEstado, new ErrorResponse(ex.Message));
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            // PostgreSQL rechazo el cambio: se traduce la regla que se rompio.
            var (codigo, mensaje) = TraducirErrorBaseDatos(pg);
            _logger.LogWarning(ex, "PostgreSQL rechazó la operación ({Codigo}) en {Ruta}", pg.SqlState, contexto.Request.Path);
            await EscribirAsync(contexto, codigo, new ErrorResponse(mensaje, Detalle: DetalleTecnico(ex)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Ruta}", contexto.Request.Path);
            await EscribirAsync(contexto, StatusCodes.Status500InternalServerError,
                new ErrorResponse("Ocurrió un error inesperado en el servidor.", Detalle: DetalleTecnico(ex)));
        }
    }//Fin del metodo InvokeAsync

    /// <summary>Solo en desarrollo: tipo de error y mensaje exacto, incluyendo la causa interna.</summary>
    private string? DetalleTecnico(Exception ex)
    {
        if (!_entorno.IsDevelopment())
            return null;

        string detalle = $"{ex.GetType().Name}: {ex.Message}";
        if (ex.InnerException is not null)
            detalle += $" → Causa: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";

        return detalle;
    }//Fin del metodo DetalleTecnico

    private static (int Codigo, string Mensaje) TraducirErrorBaseDatos(PostgresException pg)
    {
        return pg.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation =>
                (StatusCodes.Status409Conflict, "Ya existe un registro con esos mismos datos."),
            PostgresErrorCodes.ForeignKeyViolation =>
                (StatusCodes.Status409Conflict, "No se puede completar la operación porque el registro está relacionado con otros datos (traslados, pedidos o viajes)."),
            PostgresErrorCodes.CheckViolation =>
                (StatusCodes.Status400BadRequest, TraducirRestriccion(pg.ConstraintName)),
            PostgresErrorCodes.StringDataRightTruncation =>
                (StatusCodes.Status400BadRequest, "Uno de los textos es más largo de lo permitido."),
            _ =>
                (StatusCodes.Status500InternalServerError, "Ocurrió un error al guardar en la base de datos.")
        };
    }//Fin del metodo TraducirErrorBaseDatos

    /// <summary>Traduce las reglas CHECK que definimos en Configurations.</summary>
    private static string TraducirRestriccion(string? nombreRestriccion)
    {
        return nombreRestriccion switch
        {
            "ck_producto_cantidad" => "La cantidad de un lote no puede ser negativa.",
            "ck_movimiento_cantidad" => "La cantidad a trasladar debe ser mayor a 0 kg.",
            "ck_linea_pedido_cantidad" => "La cantidad de cada línea del pedido debe ser mayor a 0 kg.",
            "ck_configuracion_dias" => "Los días de anticipación deben ser mayores a 0.",
            _ => "Los datos no cumplen una regla de la base de datos."
        };
    }//Fin del metodo TraducirRestriccion

    private static async Task EscribirAsync(HttpContext contexto, int codigoEstado, ErrorResponse error)
    {
        contexto.Response.StatusCode = codigoEstado;
        await contexto.Response.WriteAsJsonAsync(error);
    }//Fin del metodo EscribirAsync
}//Fin de la clase