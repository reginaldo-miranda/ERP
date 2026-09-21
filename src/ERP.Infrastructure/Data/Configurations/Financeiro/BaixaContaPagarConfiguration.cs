using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class BaixaContaPagarConfiguration : IEntityTypeConfiguration<BaixaContaPagar>
{
    public void Configure(EntityTypeBuilder<BaixaContaPagar> builder)
    {
        builder.ToTable("BaixasContasPagar");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Observacoes).HasMaxLength(500);

        builder.Property(b => b.ValorPrincipal).HasPrecision(18, 2);
        builder.Property(b => b.ValorJuros).HasPrecision(18, 2);
        builder.Property(b => b.ValorMulta).HasPrecision(18, 2);
        builder.Property(b => b.ValorDesconto).HasPrecision(18, 2);
        builder.Property(b => b.ValorTotalPago).HasPrecision(18, 2);

        builder.HasOne(b => b.ContaPagar)
            .WithMany(cp => cp.Baixas)
            .HasForeignKey(b => b.ContaPagarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.ContaBancaria)
            .WithMany()
            .HasForeignKey(b => b.ContaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.FormaPagamento)
            .WithMany()
            .HasForeignKey(b => b.FormaPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.MovimentacaoFinanceira)
            .WithMany()
            .HasForeignKey(b => b.MovimentacaoFinanceiraId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(b => new { b.EmpresaId, b.DataBaixa });
        builder.HasIndex(b => b.ContaPagarId);
    }
}
