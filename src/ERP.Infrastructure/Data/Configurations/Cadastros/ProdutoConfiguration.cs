using ERP.Domain.Core.Entities.Cadastros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Cadastros;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");
        builder.HasIndex(p => new { p.Codigo, p.EmpresaId }).IsUnique();
        builder.Property(p => p.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Nome).HasMaxLength(200).IsRequired();
        builder.Property(p => p.PrecoVenda).HasPrecision(18, 4);
        builder.Property(p => p.PrecoCusto).HasPrecision(18, 4);
        builder.Property(p => p.EstoqueMinimo).HasPrecision(18, 3);
        builder.Property(p => p.Ncm).HasMaxLength(10);
        builder.Property(p => p.Cest).HasMaxLength(10);
        builder.Property(p => p.Cfop).HasMaxLength(10);
        builder.Property(p => p.CodigoBarras).HasMaxLength(50);
        builder.HasOne(p => p.Categoria).WithMany(c => c.Produtos).HasForeignKey(p => p.CategoriaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(p => p.UnidadeMedida).WithMany(u => u.Produtos).HasForeignKey(p => p.UnidadeMedidaId).OnDelete(DeleteBehavior.SetNull);
    }
}
