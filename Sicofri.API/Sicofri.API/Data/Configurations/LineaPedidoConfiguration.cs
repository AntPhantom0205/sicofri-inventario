using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class LineaPedidoConfiguration : IEntityTypeConfiguration<LineaPedido>
{
    public void Configure(EntityTypeBuilder<LineaPedido> builder)
    {
        builder.HasKey(l => l.IdDetalle);

        builder.Property(l => l.Cantidad).HasPrecision(10, 2);

        // Las líneas son parte del pedido: si el pedido se borra, sus lineas tambien.
        builder.HasOne(l => l.Pedido)
               .WithMany(p => p.Lineas)
               .HasForeignKey(l => l.IdPedido)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Producto)
               .WithMany(p => p.LineasPedido)
               .HasForeignKey(l => l.IdProducto)
               .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t => t.HasCheckConstraint("ck_linea_pedido_cantidad", "cantidad > 0"));
    }//Fin del metodo Configure

}//Fin de la clase