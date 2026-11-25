using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> builder)
    {
        builder.HasKey(n => n.IdNotificacion);

        builder.HasOne(n => n.Producto)
               .WithMany(p => p.Notificaciones)
               .HasForeignKey(n => n.IdProducto)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.UsuarioAtiende)
               .WithMany()
               .HasForeignKey(n => n.IdUsuarioAtiende)
               .OnDelete(DeleteBehavior.Restrict);

        // Una sola notificación por lote: la base de datos misma lo garantiza.
        builder.HasIndex(n => n.IdProducto).IsUnique();

        // Acelera la consulta de pendientes (la campanita).
        builder.HasIndex(n => n.Atendida);
    }//Fin del metodo Configure
}//Fin de la clase NotificacionConfiguration