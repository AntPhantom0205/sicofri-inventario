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
        builder.Property(p => p.Cantidad).HasPrecision(10, 2);

        // Un lote pertenece a una ubicación; una ubicación tiene muchos lotes.
        builder.HasOne(p => p.Ubicacion)
               .WithMany(u => u.Productos)
               .HasForeignKey(p => p.IdUbicacion)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("ck_producto_cantidad", "cantidad >= 0"));

    }//Fin del metodo Configure
}//Fin de la clase