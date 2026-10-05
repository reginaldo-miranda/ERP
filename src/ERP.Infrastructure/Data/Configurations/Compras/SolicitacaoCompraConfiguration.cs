using ERP.Domain.Core.Entities.Compras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Compras;

public class SolicitacaoCompraConfiguration : IEntityTypeConfiguration<SolicitacaoCompra>
{
    public void Configure(EntityTypeBuilder<SolicitacaoCompra> builder)
    {
        builder.ToTable("SolicitacoesCompra");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Numero).HasMaxLength(30).IsRequired();
        builder.Property(s => s.SolicitanteId).HasMaxLength(450).IsRequired();
        builder.Property(s => s.SolicitanteNome).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Departamento).HasMaxLength(100);
        builder.Property(s => s.Justificativa).HasMaxLength(1000);
        builder.Property(s => s.Observacoes).HasMaxLength(1000);
        builder.Property(s => s.AprovadorNome).HasMaxLength(150);
        builder.Property(s => s.MotivoRejeicao).HasMaxLength(500);

        builder.HasMany(s => s.Itens)
            .WithOne(i => i.SolicitacaoCompra)
            .HasForeignKey(i => i.SolicitacaoCompraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.EmpresaId, s.Numero }).IsUnique();
        builder.HasIndex(s => new { s.EmpresaId, s.Status });
    }
}

public class SolicitacaoCompraItemConfiguration : IEntityTypeConfiguration<SolicitacaoCompraItem>
{
    public void Configure(EntityTypeBuilder<SolicitacaoCompraItem> builder)
    {
        builder.ToTable("SolicitacaoCompraItens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantidade).HasPrecision(18, 4);
        builder.Property(i => i.QuantidadeAtendida).HasPrecision(18, 4);
        builder.Property(i => i.Observacao).HasMaxLength(500);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.SolicitacaoCompraId);
        builder.HasIndex(i => i.ProdutoId);
    }
}
