using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.HasKey(c => c.IdCliente);

        builder.Property(c => c.Nombre).HasMaxLength(60);
        builder.Property(c => c.Provincia).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.UbicacionDetalle).HasMaxLength(150);
        builder.Property(c => c.Telefono).HasMaxLength(20);
        builder.Property(c => c.NombreContacto).HasMaxLength(60);

        // Acelera el mapa, que agrupa los clientes por provincia.
        builder.HasIndex(c => c.Provincia);
    }//Fin del metodo Configure
}//Fin de la clase ClienteConfiguration