using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class MovimientoTrasladoConfiguration :IEntityTypeConfiguration<MovimientoTraslado>
{
    public void Configure(EntityTypeBuilder<MovimientoTraslado> builder)
    {
        builder.HasKey(m => m.IdMovimiento);

        builder.Property(m => m.Cantidad).HasPrecision(10, 2);

        builder.HasOne(m => m.ProductoOrigen)
               .WithMany()
               .HasForeignKey(m => m.IdProductoOrigen)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ProductoDestino)
               .WithMany()
               .HasForeignKey(m => m.IdProductoDestino)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.UbicacionOrigen)
               .WithMany()
               .HasForeignKey(m => m.IdUbicacionOrigen)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.UbicacionDestino)
               .WithMany()
               .HasForeignKey(m => m.IdUbicacionDestino)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Usuario)
               .WithMany(u => u.Movimientos)
               .HasForeignKey(m => m.IdUsuario)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("ck_movimiento_cantidad", "cantidad > 0"));

    }//Fin del metodo Configure
}//Fin de la clase