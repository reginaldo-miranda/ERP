using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Fiscal;

public class CfopConfiguration : IEntityTypeConfiguration<Cfop>
{
    public void Configure(EntityTypeBuilder<Cfop> builder)
    {
        builder.ToTable("Cfops");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Codigo).HasMaxLength(4).IsRequired();
        builder.Property(c => c.Descricao).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Aplicacao).HasMaxLength(1000);

        builder.HasIndex(c => c.Codigo).IsUnique();
        builder.HasIndex(c => c.TipoOperacao);
    }
}
