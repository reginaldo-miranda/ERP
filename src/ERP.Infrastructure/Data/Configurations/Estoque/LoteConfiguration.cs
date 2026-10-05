using ERP.Domain.Core.Entities.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Estoque;

public class LoteConfiguration : IEntityTypeConfiguration<Lote>
{
    public void Configure(EntityTypeBuilder<Lote> builder)
    {
        builder.ToTable("Lotes");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.CodigoLote)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(l => l.QuantidadeInicial)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(l => l.QuantidadeAtual)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(l => l.CustoUnitario)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(l => l.Observacoes)
            .HasMaxLength(500);

        builder.HasOne(l => l.Produto)
            .WithMany()
            .HasForeignKey(l => l.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Deposito)
            .WithMany()
            .HasForeignKey(l => l.DepositoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.EmpresaId, l.ProdutoId, l.CodigoLote, l.DepositoId }).IsUnique();
        builder.HasIndex(l => new { l.EmpresaId, l.DataValidade });
    }
}
