using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.IdUsuario);

        builder.Property(u => u.Nombre).HasMaxLength(60);
        builder.Property(u => u.NombreUsuario).HasMaxLength(30);
        builder.Property(u => u.ContrasenaHash).HasMaxLength(100);
        builder.Property(u => u.Rol).HasConversion<string>().HasMaxLength(15);
        builder.Property(u => u.TokenFcm).HasMaxLength(500);

        // No pueden existir dos usuarios con el mismo nombre de usuario.
        builder.HasIndex(u => u.NombreUsuario).IsUnique();
    }//Fin del metodo Configure
}//Fin de la clase