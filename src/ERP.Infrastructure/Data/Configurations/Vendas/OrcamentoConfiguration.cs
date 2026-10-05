using ERP.Domain.Core.Entities.Vendas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Vendas;

public class OrcamentoConfiguration : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> builder)
    {
        builder.ToTable("Orcamentos");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Numero).HasMaxLength(30).IsRequired();
        builder.Property(o => o.VendedorId).HasMaxLength(450);
        builder.Property(o => o.VendedorNome).HasMaxLength(150);
        builder.Property(o => o.Observacao).HasMaxLength(1000);

        builder.Property(o => o.ValorBruto).HasPrecision(18, 2);
        builder.Property(o => o.DescontoGeral).HasPrecision(18, 4);
        builder.Property(o => o.ValorDesconto).HasPrecision(18, 2);
        builder.Property(o => o.ValorTotal).HasPrecision(18, 2);

        builder.HasOne(o => o.Cliente)
            .WithMany()
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relação explícita (sem navegação inversa) para não conflitar com PedidoVenda.Orcamento.
        builder.HasOne(o => o.PedidoVenda)
            .WithMany()
            .HasForeignKey(o => o.PedidoVendaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.Itens)
            .WithOne(i => i.Orcamento)
            .HasForeignKey(i => i.OrcamentoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.EmpresaId, o.Numero }).IsUnique();
        builder.HasIndex(o => new { o.EmpresaId, o.Status });
        builder.HasIndex(o => o.ClienteId);
    }
}

public class OrcamentoItemConfiguration : IEntityTypeConfiguration<OrcamentoItem>
{
    public void Configure(EntityTypeBuilder<OrcamentoItem> builder)
    {
        builder.ToTable("OrcamentoItens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Descricao).HasMaxLength(250).IsRequired();
        builder.Property(i => i.Quantidade).HasPrecision(18, 4);
        builder.Property(i => i.PrecoUnitario).HasPrecision(18, 4);
        builder.Property(i => i.Desconto).HasPrecision(18, 4);
        builder.Property(i => i.ValorDesconto).HasPrecision(18, 2);
        builder.Property(i => i.SubTotal).HasPrecision(18, 2);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Servico)
            .WithMany()
            .HasForeignKey(i => i.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.OrcamentoId);
    }
}
