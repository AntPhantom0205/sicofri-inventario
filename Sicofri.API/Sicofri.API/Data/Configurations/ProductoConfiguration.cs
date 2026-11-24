using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.HasKey(p => p.IdProducto);

        builder.Property(p => p.Nombre).HasMaxLength(60);
        builder.Property(p => p.NombreProveedor).HasMaxLength(60);
        builder.Property(p => p.TipoLote).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.CodigoLote).HasMaxLength(40);
        builder.Property(p => p.Cantidad).HasPrecision(10, 2);

        // Un lote pertenece a una ubicacion; una ubicación tiene muchos lotes.
        builder.HasOne(p => p.Ubicacion)
               .WithMany(u => u.Productos)
               .HasForeignKey(p => p.IdUbicacion)
               .OnDelete(DeleteBehavior.Restrict);

        // Acelera la busqueda diaria de lotes próximos a caducar.
        builder.HasIndex(p => p.FechaCaducidad);

        builder.ToTable(t => t.HasCheckConstraint("ck_producto_cantidad", "cantidad >= 0"));
    }//Find del metodo Configure
}//Fin de la clase