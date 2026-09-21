using ERP.Domain.Core.Entities.Estoque;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Estoque;

public class EstoqueProdutoConfiguration : IEntityTypeConfiguration<EstoqueProduto>
{
    public void Configure(EntityTypeBuilder<EstoqueProduto> builder)
    {
        builder.ToTable("EstoqueProdutos");
        builder.HasKey(ep => ep.Id);

        builder.Property(ep => ep.Quantidade)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(ep => ep.CustoMedio)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(ep => ep.CustoUltimaCompra)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(ep => ep.EstoqueMinimo)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(ep => ep.EstoqueMaximo)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.HasOne(ep => ep.Produto)
            .WithMany()
            .HasForeignKey(ep => ep.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ep => ep.Deposito)
            .WithMany(d => d.EstoquesProdutos)
            .HasForeignKey(ep => ep.DepositoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ep => new { ep.EmpresaId, ep.ProdutoId, ep.DepositoId }).IsUnique();
    }
}
