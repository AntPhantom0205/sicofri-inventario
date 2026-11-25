using System.Text;
using Microsoft.EntityFrameworkCore;
using Sicofri.API.Data.Seed;
using Sicofri.API.Entities;

namespace Sicofri.API.Data;

/// <summary>
/// Puente entre las clases de C# y la base de datos PostgreSQL.
/// Cada DbSet representa una tabla.
/// </summary>

public class SicofriDbContext : DbContext
{
    public SicofriDbContext(DbContextOptions<SicofriDbContext> options) : base(options)
    {

    }
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<MovimientoTraslado> MovimientosTraslado => Set<MovimientoTraslado>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Viaje> Viajes => Set<Viaje>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<LineaPedido> LineasPedido => Set<LineaPedido>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();
    public DbSet<ViajeEncargado> ViajeEncargados => Set<ViajeEncargado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       base.OnModelCreating(modelBuilder);
        //Busca y aplica todos los archivos de la carpeta Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SicofriDbContext).Assembly);

        //Datos iniciales: Las 6 ubicaciones y la configuracion por defecto.
        DatosIniciales.Cargar(modelBuilder);

        //Debe ir al final esto porque permite renombrar las tablas y las columna a
        //Minuscula con guion bajo, que es la convención de PostgreSQL.
        AplicarNombreSnakeCase(modelBuilder);
    }//Fin del metodo OnModelCreating

    private static void AplicarNombreSnakeCase(ModelBuilder modelBuilder)
    {
        foreach (var entidad in modelBuilder.Model.GetEntityTypes())
        {
            entidad.SetTableName(ConvertirASnakeCase(entidad.ClrType.Name));

            foreach (var propiedad in entidad.GetProperties())
            {
                propiedad.SetColumnName(ConvertirASnakeCase(propiedad.Name));
            }
        }
    }//Fin del metodo AplicarNombreSnakeCase

    /// <summary>Convierte "IdProducto" en "id_producto".</summary>
    private static string ConvertirASnakeCase(string nombre)
    {
        var resultado = new StringBuilder();

        for (int i = 0; i < nombre.Length; i++)
        {
            char letra = nombre[i];

            if (char.IsUpper(letra))
            {
                if (i > 0) resultado.Append('_');
                resultado.Append(char.ToLowerInvariant(letra));
            }
            else
            {
                resultado.Append(letra);
            }
        }

        return resultado.ToString();
    }//Fin del metodo ConvertirASnakeCase

}//Fin de la clase
