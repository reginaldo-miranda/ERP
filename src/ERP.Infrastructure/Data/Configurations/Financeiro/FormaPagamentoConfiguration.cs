using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class FormaPagamentoConfiguration : IEntityTypeConfiguration<FormaPagamento>
{
    public void Configure(EntityTypeBuilder<FormaPagamento> builder)
    {
        builder.ToTable("FormasPagamento");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Nome).HasMaxLength(100).IsRequired();
        builder.Property(f => f.TaxaPercentual).HasPrecision(6, 2);

        builder.HasIndex(f => new { f.EmpresaId, f.Nome });
    }
}
