using Microsoft.EntityFrameworkCore;
using Sicofri.API.Data;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;

namespace Sicofri.API.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly SicofriDbContext _db;

    public ConfiguracionRepository(SicofriDbContext db)
    {
        _db = db;
    }

    /// <summary>La tabla tiene una sola fila, creada por los datos iniciales.</summary>
    public async Task<Configuracion> ObtenerAsync()
    {
        return await _db.Configuraciones.FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No existe la fila de configuración. Ejecute Update-Database.");
    }//Fin del metodo ObtenerAsync
    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }//Fin del metodo GuardarCambiosAsync

}//Fin de la clase ConfiguracionRepository
