using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Configuracion;

namespace Sicofri.API.Services;

/// <summary>Consulta y cambio de los parámetros generales del sistema.</summary>
public class ConfiguracionService : IConfiguracionService
{
    private const int DiasAnticipacionMinimo = 1;
    private const int DiasAnticipacionMaximo = 60;

    private readonly IConfiguracionRepository _configuracion;

    public ConfiguracionService(IConfiguracionRepository configuracion)
    {
        _configuracion = configuracion;
    }

    public async Task<ConfiguracionDto> ObtenerAsync()
    {
        var configuracion = await _configuracion.ObtenerAsync();
        return ConvertirADto(configuracion.DiasAnticipacion);
    }//Fin del metodo ObtenerAsync

    public async Task<ConfiguracionDto> ActualizarAsync(ActualizarConfiguracionRequest datos)
    {
        if (datos.DiasAnticipacion < DiasAnticipacionMinimo || datos.DiasAnticipacion > DiasAnticipacionMaximo)
        {
            throw new ReglaNegocioException(
                $"Los días de anticipación deben estar entre {DiasAnticipacionMinimo} y {DiasAnticipacionMaximo}.");
        }

        var configuracion = await _configuracion.ObtenerAsync();
        configuracion.DiasAnticipacion = datos.DiasAnticipacion;
        await _configuracion.GuardarCambiosAsync();

        return ConvertirADto(configuracion.DiasAnticipacion);
    }//Fin del metodo ActualizarAsync

    private static ConfiguracionDto ConvertirADto(int diasAnticipacion)
    {
        return new ConfiguracionDto(
            diasAnticipacion,
            ReglasCaducidad.DiasAvisoFresco,
            ReglasCaducidad.DiasVidaFresco);
    }//Fin del metodo ConvertirDto
}//Fin de la clase ConfiguracionService