using Microsoft.EntityFrameworkCore;
using Sicofri.API.Entities;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Data.Seed;

/// <summary>
/// Datos que deben existir desde que se crea la base de datos.
/// </summary>

public class DatosIniciales
{

    public static void Cargar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ubicacion>().HasData(
            new Ubicacion { IdUbicacion = 1, Nombre = "Congelador 1", Zona = Provincia.Puntarenas },
            new Ubicacion { IdUbicacion = 2, Nombre = "Congelador 2", Zona = Provincia.Puntarenas },
            new Ubicacion { IdUbicacion = 3, Nombre = "Congelador 3", Zona = Provincia.Puntarenas },
            new Ubicacion { IdUbicacion = 4, Nombre = "Congelador 4", Zona = Provincia.Puntarenas },
            new Ubicacion { IdUbicacion = 5, Nombre = "Congelador 5", Zona = Provincia.Puntarenas },
            new Ubicacion { IdUbicacion = 6, Nombre = "Cámara San José", Zona = Provincia.SanJose }
        );

        modelBuilder.Entity<Configuracion>().HasData(
           new Configuracion { IdConfiguracion = 1, DiasAnticipacion = 10 }
       );
    }//Fin del metodo Cargar

}//Fin de la clase
