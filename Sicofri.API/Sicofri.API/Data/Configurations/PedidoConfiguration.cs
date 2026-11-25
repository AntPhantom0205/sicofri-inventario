using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.HasKey(p => p.IdPedido);

        builder.Property(p => p.Estado).HasConversion<string>().HasMaxLength(15);

        builder.HasOne(p => p.Cliente)
               .WithMany(c => c.Pedidos)
               .HasForeignKey(p => p.IdCliente)
               .OnDelete(DeleteBehavior.Restrict);

        // Si se elimina un viaje Programado, sus pedidos quedan libres (no se borran).
        builder.HasOne(p => p.Viaje)
               .WithMany(v => v.Pedidos)
               .HasForeignKey(p => p.IdViaje)
               .OnDelete(DeleteBehavior.SetNull);

        // Quién creó, entregó y canceló: tres relaciones distintas hacia Usuario.
        builder.HasOne(p => p.UsuarioCreador)
               .WithMany()
               .HasForeignKey(p => p.IdUsuarioCreador)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UsuarioEntrega)
               .WithMany()
               .HasForeignKey(p => p.IdUsuarioEntrega)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UsuarioCancela)
               .WithMany()
               .HasForeignKey(p => p.IdUsuarioCancela)
               .OnDelete(DeleteBehavior.Restrict);

        // Acelera la bitácora y "mis entregas", que filtran por estado y fecha.
        builder.HasIndex(p => p.Estado);
        builder.HasIndex(p => p.FechaEntrega);
    }//Fin del metodo Configure
}//Fin de la clase PedidoConfiguration