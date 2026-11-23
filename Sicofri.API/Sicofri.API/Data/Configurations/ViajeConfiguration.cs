using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class ViajeConfiguration : IEntityTypeConfiguration<Viaje>
{
    public void Configure(EntityTypeBuilder<Viaje> builder)
    {
        builder.HasKey(v => v.IdViaje);

        builder.Property(v => v.Estado).HasConversion<string>().HasMaxLength(15);

        builder.HasOne(v => v.UsuarioEncargado)
               .WithMany(u => u.ViajesEncargados)
               .HasForeignKey(v => v.IdUsuarioEncargado)
               .OnDelete(DeleteBehavior.Restrict);
    }//Fin del metodo ViajeConfiguration
}//Fin de la clase