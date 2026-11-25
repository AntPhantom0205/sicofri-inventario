using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class ViajeEncargadoConfiguration : IEntityTypeConfiguration<ViajeEncargado>
{
    public void Configure(EntityTypeBuilder<ViajeEncargado> builder)
    {
        // Llave compuesta: un empleado no puede estar dos veces en el mismo viaje.
        builder.HasKey(ve => new { ve.IdViaje, ve.IdUsuario });

        // Si se elimina un viaje (solo posible cuando este esta Programado), se van sus asignaciones.
        builder.HasOne(ve => ve.Viaje)
               .WithMany(v => v.Encargados)
               .HasForeignKey(ve => ve.IdViaje)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ve => ve.Usuario)
               .WithMany(u => u.ViajesEncargados)
               .HasForeignKey(ve => ve.IdUsuario)
               .OnDelete(DeleteBehavior.Restrict);
    }
}//Fin de la clase ViajeEncargadoConfiguration