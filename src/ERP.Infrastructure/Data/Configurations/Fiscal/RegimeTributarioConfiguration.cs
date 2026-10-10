using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Fiscal;

public class RegimeTributarioConfiguration : IEntityTypeConfiguration<RegimeTributario>
{
    public void Configure(EntityTypeBuilder<RegimeTributario> builder)
    {
        builder.ToTable("RegimesTributarios");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.IdCscToken).HasMaxLength(10);
        builder.Property(r => r.CodigoCsc).HasMaxLength(100);
        builder.Property(r => r.AliquotaCreditoSimples).HasPrecision(18, 2);
        builder.Property(r => r.MensagemPadraoNotas).HasMaxLength(1000);

        builder.HasIndex(r => r.EmpresaId).IsUnique();
    }
}
