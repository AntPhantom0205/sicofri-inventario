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
    }//Fin del metodo NotificacionConfiguration
}//Fin de la clase