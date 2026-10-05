using ERP.Domain.Core.Entities.Compras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Compras;

public class PedidoCompraConfiguration : IEntityTypeConfiguration<PedidoCompra>
{
    public void Configure(EntityTypeBuilder<PedidoCompra> builder)
    {
        builder.ToTable("PedidosCompra");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Numero).HasMaxLength(30).IsRequired();
        builder.Property(p => p.CompradorNome).HasMaxLength(150);
        builder.Property(p => p.Observacoes).HasMaxLength(1000);
        builder.Property(p => p.MotivoCancelamento).HasMaxLength(500);

        builder.Property(p => p.ValorBruto).HasPrecision(18, 2);
        builder.Property(p => p.ValorDesconto).HasPrecision(18, 2);
        builder.Property(p => p.ValorFrete).HasPrecision(18, 2);
        builder.Property(p => p.ValorSeguro).HasPrecision(18, 2);
        builder.Property(p => p.ValorOutrasDespesas).HasPrecision(18, 2);
        builder.Property(p => p.ValorTotal).HasPrecision(18, 2);

        builder.HasOne(p => p.Fornecedor)
            .WithMany()
            .HasForeignKey(p => p.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.SolicitacaoCompra)
            .WithMany()
            .HasForeignKey(p => p.SolicitacaoCompraId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.DepositoDestino)
            .WithMany()
            .HasForeignKey(p => p.DepositoDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.FormaPagamento)
            .WithMany()
            .HasForeignKey(p => p.FormaPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Itens)
            .WithOne(i => i.PedidoCompra)
            .HasForeignKey(i => i.PedidoCompraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Entradas)
            .WithOne(e => e.PedidoCompra)
            .HasForeignKey(e => e.PedidoCompraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.EmpresaId, p.Numero }).IsUnique();
        builder.HasIndex(p => new { p.EmpresaId, p.Status });
        builder.HasIndex(p => p.FornecedorId);
        builder.HasIndex(p => p.SolicitacaoCompraId);
    }
}

public class PedidoCompraItemConfiguration : IEntityTypeConfiguration<PedidoCompraItem>
{
    public void Configure(EntityTypeBuilder<PedidoCompraItem> builder)
    {
        builder.ToTable("PedidoCompraItens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantidade).HasPrecision(18, 4);
        builder.Property(i => i.QuantidadeEntregue).HasPrecision(18, 4);
        builder.Property(i => i.PrecoUnitario).HasPrecision(18, 4);
        builder.Property(i => i.ValorDesconto).HasPrecision(18, 2);
        builder.Property(i => i.SubTotal).HasPrecision(18, 2);
        builder.Property(i => i.Observacao).HasMaxLength(500);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.PedidoCompraId);
        builder.HasIndex(i => i.ProdutoId);
    }
}
