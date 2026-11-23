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

        // Si se elimina un viaje, sus pedidos quedan sin viaje asignado
        // (no se borran).
        builder.HasOne(p => p.Viaje)
               .WithMany(v => v.Pedidos)
               .HasForeignKey(p => p.IdViaje)
               .OnDelete(DeleteBehavior.SetNull);
    }//Fin del metodo Configure

}//Fin de la clase