using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class CentroCustoConfiguration : IEntityTypeConfiguration<CentroCusto>
{
    public void Configure(EntityTypeBuilder<CentroCusto> builder)
    {
        builder.ToTable("CentrosCusto");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Codigo).HasMaxLength(30).IsRequired();
        builder.Property(c => c.Descricao).HasMaxLength(150).IsRequired();

        builder.HasIndex(c => new { c.EmpresaId, c.Codigo }).IsUnique();
    }
}
