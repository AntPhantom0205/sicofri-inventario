using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class UbicacionConfiguration : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> builder)
    {
        builder.HasKey(u => u.IdUbicacion);

        builder.Property(u => u.Nombre).HasMaxLength(40);
        builder.Property(u => u.Zona).HasConversion<string>().HasMaxLength(20);

    }//Fin del metodo Configure
}//Fin de la clase