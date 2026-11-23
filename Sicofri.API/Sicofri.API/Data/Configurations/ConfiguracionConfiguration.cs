using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sicofri.API.Entities;

namespace Sicofri.API.Data.Configurations;

public class ConfigurationConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> builder)
    {
        builder.HasKey(c => c.IdConfiguracion);

        builder.ToTable(t => t.HasCheckConstraint("ck_configuracion_dias", "dias_anticipacion > 0"));
    }//Fin del metodo Configure

}//Fin de la clase