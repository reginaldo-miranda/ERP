using ERP.Domain.Core.Entities.Compras;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Compras;

public class EntradaMercadoriaConfiguration : IEntityTypeConfiguration<EntradaMercadoria>
{
    public void Configure(EntityTypeBuilder<EntradaMercadoria> builder)
    {
        builder.ToTable("EntradasMercadoria");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Numero).HasMaxLength(30).IsRequired();
        builder.Property(e => e.NumeroNotaFiscal).HasMaxLength(50).IsRequired();
        builder.Property(e => e.SerieNotaFiscal).HasMaxLength(20);
        builder.Property(e => e.ChaveAcessoNfe).HasMaxLength(60);
        builder.Property(e => e.ConferenteNome).HasMaxLength(150);
        builder.Property(e => e.Observacoes).HasMaxLength(1000);
        builder.Property(e => e.MotivoCancelamento).HasMaxLength(500);

        builder.Property(e => e.ValorProdutos).HasPrecision(18, 2);
        builder.Property(e => e.ValorFrete).HasPrecision(18, 2);
        builder.Property(e => e.ValorSeguro).HasPrecision(18, 2);
        builder.Property(e => e.ValorOutrasDespesas).HasPrecision(18, 2);
        builder.Property(e => e.ValorDesconto).HasPrecision(18, 2);
        builder.Property(e => e.ValorTotal).HasPrecision(18, 2);

        builder.HasOne(e => e.Fornecedor)
            .WithMany()
            .HasForeignKey(e => e.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Deposito)
            .WithMany()
            .HasForeignKey(e => e.DepositoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Itens)
            .WithOne(i => i.EntradaMercadoria)
            .HasForeignKey(i => i.EntradaMercadoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.EmpresaId, e.Numero }).IsUnique();
        builder.HasIndex(e => new { e.EmpresaId, e.FornecedorId, e.NumeroNotaFiscal, e.SerieNotaFiscal });
        builder.HasIndex(e => e.PedidoCompraId);
        builder.HasIndex(e => e.IdParcelamento);
    }
}

public class EntradaMercadoriaItemConfiguration : IEntityTypeConfiguration<EntradaMercadoriaItem>
{
    public void Configure(EntityTypeBuilder<EntradaMercadoriaItem> builder)
    {
        builder.ToTable("EntradaMercadoriaItens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Quantidade).HasPrecision(18, 4);
        builder.Property(i => i.CustoUnitario).HasPrecision(18, 4);
        builder.Property(i => i.CustoTotal).HasPrecision(18, 2);

        builder.Property(i => i.CodigoLote).HasMaxLength(50);
        builder.Property(i => i.NumerosSerieTexto).HasMaxLength(4000);
        builder.Property(i => i.Observacao).HasMaxLength(500);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.PedidoCompraItem)
            .WithMany()
            .HasForeignKey(i => i.PedidoCompraItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Lote)
            .WithMany()
            .HasForeignKey(i => i.LoteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.EntradaMercadoriaId);
        builder.HasIndex(i => i.ProdutoId);
        builder.HasIndex(i => i.LoteId);
    }
}
