using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Sicofri.Shared.Dtos;

namespace Sicofri.API.Middleware;

/// <summary>
/// Reemplaza la respuesta automática (en ingles) que da ASP.NET cuando el JSON
/// recibido no se puede leer, por una respuesta en espanol que indica cada campo con problema.
/// </summary>

public class RespuestaValidacion
{
    private const string CampoGeneral = "general";

    public static IActionResult Crear(ActionContext contexto)
    {
        var errores = new Dictionary<string, string[]>();

        foreach (var (clave, entrada) in contexto.ModelState)
        {
            if (entrada.Errors.Count == 0)
                continue;

            string campo = LimpiarNombreCampo(clave);
            errores[campo] = entrada.Errors
                .Select(error => Traducir(campo, error))
                .Distinct()
                .ToArray();
        }

        return new BadRequestObjectResult(
            new ErrorResponse("Los datos enviados no son válidos. Revise los campos indicados.", errores));
    }//Fin del metodo Crear

    /// <summary>Convierte "$.fechaIngreso" en "fechaIngreso".</summary>
    private static string LimpiarNombreCampo(string clave)
    {
        string limpio = clave.Replace("$.", string.Empty).Trim('$', '.');

        if (limpio.Length == 0 || limpio.Equals("datos", StringComparison.OrdinalIgnoreCase))
            return CampoGeneral;

        return char.ToLowerInvariant(limpio[0]) + limpio[1..];
    }//Fin del metodo LimpiarNombreCampo

    private static string Traducir(string campo, ModelError error)
    {
        if (campo == CampoGeneral)
            return "No se recibieron datos, o el JSON tiene un formato incorrecto (revise comas, llaves y comillas).";

        if (error.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase))
            return $"El campo '{campo}' es obligatorio.";

        return $"El valor de '{campo}' no es válido. Revise el tipo de dato: número, fecha en formato AAAA-MM-DD, o una de las opciones permitidas.";
    }//Fin del metodo Traducir
}//Fin de la clase RespuestaValidacion 
