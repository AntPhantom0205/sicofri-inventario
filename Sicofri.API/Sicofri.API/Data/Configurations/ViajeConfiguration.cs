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

        builder.HasOne(v => v.UsuarioCreador)
               .WithMany()
               .HasForeignKey(v => v.IdUsuarioCreador)
               .OnDelete(DeleteBehavior.Restrict);
    }//Fin del método Configure
}//Fin de la clase ViajeConfiguration