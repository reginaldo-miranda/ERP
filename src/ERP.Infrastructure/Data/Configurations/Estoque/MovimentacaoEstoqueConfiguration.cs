using ERP.Domain.Core.Entities.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Estoque;

public class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.ToTable("MovimentacoesEstoque");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Quantidade)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(m => m.CustoUnitario)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(m => m.CustoTotal)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(m => m.DocumentoOrigem)
            .HasMaxLength(50);

        builder.Property(m => m.Observacao)
            .HasMaxLength(500);

        builder.HasOne(m => m.Produto)
            .WithMany()
            .HasForeignKey(m => m.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.DepositoOrigem)
            .WithMany()
            .HasForeignKey(m => m.DepositoOrigemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.DepositoDestino)
            .WithMany()
            .HasForeignKey(m => m.DepositoDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.EmpresaId, m.DataMovimentacao });
        builder.HasIndex(m => new { m.EmpresaId, m.ProdutoId });
    }
}
