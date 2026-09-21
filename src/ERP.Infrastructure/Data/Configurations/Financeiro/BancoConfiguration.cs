using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class BancoConfiguration : IEntityTypeConfiguration<Banco>
{
    public void Configure(EntityTypeBuilder<Banco> builder)
    {
        builder.ToTable("Bancos");
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => b.Codigo).IsUnique();
        builder.Property(b => b.Codigo).HasMaxLength(10).IsRequired();
        builder.Property(b => b.Nome).HasMaxLength(150).IsRequired();
        builder.Property(b => b.NomeReduzido).HasMaxLength(60);
    }
}
