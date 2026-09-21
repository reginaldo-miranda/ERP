using ERP.Domain.Core.Entities.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Estoque;

public class DepositoConfiguration : IEntityTypeConfiguration<Deposito>
{
    public void Configure(EntityTypeBuilder<Deposito> builder)
    {
        builder.ToTable("Depositos");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Nome)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Codigo)
            .HasMaxLength(20);

        builder.Property(d => d.Endereco)
            .HasMaxLength(250);

        builder.Property(d => d.Responsavel)
            .HasMaxLength(100);

        builder.HasIndex(d => new { d.EmpresaId, d.Nome });
    }
}
