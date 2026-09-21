using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class ContaReceberConfiguration : IEntityTypeConfiguration<ContaReceber>
{
    public void Configure(EntityTypeBuilder<ContaReceber> builder)
    {
        builder.ToTable("ContasReceber");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Descricao).HasMaxLength(250).IsRequired();
        builder.Property(c => c.NumeroDocumento).HasMaxLength(50);
        builder.Property(c => c.Observacoes).HasMaxLength(500);

        builder.Property(c => c.ValorOriginal).HasPrecision(18, 2);
        builder.Property(c => c.ValorRecebido).HasPrecision(18, 2);
        builder.Property(c => c.SaldoRestante).HasPrecision(18, 2);

        builder.HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ContaBancaria)
            .WithMany()
            .HasForeignKey(c => c.ContaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.FormaPagamento)
            .WithMany()
            .HasForeignKey(c => c.FormaPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PlanoConta)
            .WithMany()
            .HasForeignKey(c => c.PlanoContaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CentroCusto)
            .WithMany()
            .HasForeignKey(c => c.CentroCustoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.EmpresaId, c.DataVencimento });
        builder.HasIndex(c => new { c.EmpresaId, c.Status });
        builder.HasIndex(c => new { c.EmpresaId, c.ClienteId });
        builder.HasIndex(c => c.IdParcelamento);
    }
}
