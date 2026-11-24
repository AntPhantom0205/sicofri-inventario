using Sicofri.API.Entities;

namespace Sicofri.API.Repositories.Interfaces;

public interface IConfiguracionRepository
{
    Task<Configuracion> ObtenerAsync();
    Task GuardarCambiosAsync();

}//Fin de la interfaz IConfiguracionRepository