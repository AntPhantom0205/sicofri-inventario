using Sicofri.Shared.Dtos.Configuracion;

namespace Sicofri.API.Services.Interfaces;

public interface IConfiguracionService
{
    Task<ConfiguracionDto> ObtenerAsync();
    Task<ConfiguracionDto> ActualizarAsync(ActualizarConfiguracionRequest datos);
}//Fin de la Interfaz IConfiguracionService