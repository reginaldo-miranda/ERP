using ERP.Domain.Core.Entities.Compras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Compras;

public class ProdutoFornecedorConfiguration : IEntityTypeConfiguration<ProdutoFornecedor>
{
    public void Configure(EntityTypeBuilder<ProdutoFornecedor> builder)
    {
        builder.ToTable("ProdutosFornecedores");
        builder.HasKey(pf => pf.Id);

        builder.Property(pf => pf.CodigoProdutoFornecedor)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(pf => pf.DescricaoProdutoFornecedor)
            .HasMaxLength(255);

        builder.Property(pf => pf.UnidadeMedidaFornecedor)
            .HasMaxLength(20);

        builder.Property(pf => pf.FatorConversao)
            .HasPrecision(18, 4)
            .HasDefaultValue(1.0m);

        builder.HasOne(pf => pf.Produto)
            .WithMany()
            .HasForeignKey(pf => pf.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pf => pf.Fornecedor)
            .WithMany()
            .HasForeignKey(pf => pf.FornecedorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice para busca rápida de De-Para por Fornecedor + Código no Fornecedor
        builder.HasIndex(pf => new { pf.EmpresaId, pf.FornecedorId, pf.CodigoProdutoFornecedor }).IsUnique();
        builder.HasIndex(pf => pf.ProdutoId);
    }
}
