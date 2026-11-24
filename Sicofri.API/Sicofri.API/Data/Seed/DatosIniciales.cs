using Microsoft.EntityFrameworkCore;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Security;
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

    /// <summary>
    /// Si la tabla de usuarios esta vacia, crea el usuario "jefe" con la contrasena
    /// definida en los secretos de usuario (JefeInicial:Contrasena).
    /// Se ejecuta cada vez que arranca la API, pero solo actua la primera vez.
    /// </summary>
    public static async Task CrearJefeInicialAsync(IServiceProvider servicios, IConfiguration configuracion)
    {
        using var alcance = servicios.CreateScope();
        var usuarios = alcance.ServiceProvider.GetRequiredService<IUsuarioRepository>();

        if (await usuarios.ExisteAlgunoAsync())
        {
            return;
        }

        string? contrasena = configuracion["JefeInicial:Contrasena"];
        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 8)
        {
            throw new InvalidOperationException(
                "Falta 'JefeInicial:Contrasena' (mínimo 8 caracteres) en los secretos de usuario.");
        }

        var hasher = alcance.ServiceProvider.GetRequiredService<PasswordHasher>();

        await usuarios.AgregarAsync(new Usuario
        {
            Nombre = "Jefe Calimar",
            NombreUsuario = "jefe",
            ContrasenaHash = hasher.Encriptar(contrasena),
            Rol = Rol.Jefe,
            Activo = true
        });

        await usuarios.GuardarCambiosAsync();
    }//Fin del metodo CrearJefeInicialAsync
}//Fin de la clase