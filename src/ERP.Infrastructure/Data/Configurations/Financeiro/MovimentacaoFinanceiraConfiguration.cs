using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class MovimentacaoFinanceiraConfiguration : IEntityTypeConfiguration<MovimentacaoFinanceira>
{
    public void Configure(EntityTypeBuilder<MovimentacaoFinanceira> builder)
    {
        builder.ToTable("MovimentacoesFinanceiras");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Descricao).HasMaxLength(250).IsRequired();
        builder.Property(m => m.DocumentoReferencia).HasMaxLength(50);
        builder.Property(m => m.Observacoes).HasMaxLength(500);
        builder.Property(m => m.Valor).HasPrecision(18, 2);

        builder.HasOne(m => m.ContaBancaria)
            .WithMany(c => c.Movimentacoes)
            .HasForeignKey(m => m.ContaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.PlanoConta)
            .WithMany()
            .HasForeignKey(m => m.PlanoContaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.CentroCusto)
            .WithMany()
            .HasForeignKey(m => m.CentroCustoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ContaPagar)
            .WithMany()
            .HasForeignKey(m => m.ContaPagarId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.ContaReceber)
            .WithMany()
            .HasForeignKey(m => m.ContaReceberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => new { m.EmpresaId, m.DataMovimentacao });
        builder.HasIndex(m => new { m.EmpresaId, m.ContaBancariaId });
    }
}
