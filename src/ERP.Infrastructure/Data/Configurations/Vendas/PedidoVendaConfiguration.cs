using ERP.Domain.Core.Entities.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Vendas;

public class PedidoVendaConfiguration : IEntityTypeConfiguration<PedidoVenda>
{
    public void Configure(EntityTypeBuilder<PedidoVenda> builder)
    {
        builder.ToTable("PedidosVenda");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Numero).HasMaxLength(30).IsRequired();
        builder.Property(p => p.VendedorId).HasMaxLength(450);
        builder.Property(p => p.VendedorNome).HasMaxLength(150);
        builder.Property(p => p.Observacao).HasMaxLength(1000);
        builder.Property(p => p.MotivoCancelamento).HasMaxLength(500);

        builder.Property(p => p.ValorBruto).HasPrecision(18, 2);
        builder.Property(p => p.DescontoGeral).HasPrecision(18, 4);
        builder.Property(p => p.ValorDesconto).HasPrecision(18, 2);
        builder.Property(p => p.ValorTotal).HasPrecision(18, 2);

        builder.HasOne(p => p.Cliente)
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relação explícita (sem navegação inversa) para não conflitar com Orcamento.PedidoVenda.
        builder.HasOne(p => p.Orcamento)
            .WithMany()
            .HasForeignKey(p => p.OrcamentoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Deposito)
            .WithMany()
            .HasForeignKey(p => p.DepositoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.FormaPagamento)
            .WithMany()
            .HasForeignKey(p => p.FormaPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Itens)
            .WithOne(i => i.PedidoVenda)
            .HasForeignKey(i => i.PedidoVendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Faturamentos)
            .WithOne(f => f.PedidoVenda)
            .HasForeignKey(f => f.PedidoVendaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.EmpresaId, p.Numero }).IsUnique();
        builder.HasIndex(p => new { p.EmpresaId, p.Status });
        builder.HasIndex(p => p.ClienteId);
        builder.HasIndex(p => p.OrcamentoId);
    }
}

public class PedidoVendaItemConfiguration : IEntityTypeConfiguration<PedidoVendaItem>
{
    public void Configure(EntityTypeBuilder<PedidoVendaItem> builder)
    {
        builder.ToTable("PedidoVendaItens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Descricao).HasMaxLength(250).IsRequired();
        builder.Property(i => i.Quantidade).HasPrecision(18, 4);
        builder.Property(i => i.PrecoUnitario).HasPrecision(18, 4);
        builder.Property(i => i.Desconto).HasPrecision(18, 4);
        builder.Property(i => i.ValorDesconto).HasPrecision(18, 2);
        builder.Property(i => i.SubTotal).HasPrecision(18, 2);
        builder.Property(i => i.CustoUnitario).HasPrecision(18, 4).HasDefaultValue(0m);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Servico)
            .WithMany()
            .HasForeignKey(i => i.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.PedidoVendaId);
    }
}

public class FaturamentoConfiguration : IEntityTypeConfiguration<Faturamento>
{
    public void Configure(EntityTypeBuilder<Faturamento> builder)
    {
        builder.ToTable("Faturamentos");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Numero).HasMaxLength(30).IsRequired();
        builder.Property(f => f.ValorTotal).HasPrecision(18, 2);
        builder.Property(f => f.CustoTotal).HasPrecision(18, 2);

        builder.HasIndex(f => new { f.EmpresaId, f.Numero }).IsUnique();
        builder.HasIndex(f => new { f.EmpresaId, f.Status });
        builder.HasIndex(f => f.PedidoVendaId);
        builder.HasIndex(f => f.IdParcelamento);
    }
}
