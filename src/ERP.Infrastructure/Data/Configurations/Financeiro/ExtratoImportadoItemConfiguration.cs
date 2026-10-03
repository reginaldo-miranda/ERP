using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class ExtratoImportadoItemConfiguration : IEntityTypeConfiguration<ExtratoImportadoItem>
{
    public void Configure(EntityTypeBuilder<ExtratoImportadoItem> builder)
    {
        builder.ToTable("ExtratosImportadosItens");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TransacaoId).HasMaxLength(100);
        builder.Property(e => e.Descricao).HasMaxLength(500).IsRequired();
        builder.Property(e => e.TipoTransacao).HasMaxLength(20);
        builder.Property(e => e.Valor).HasPrecision(18, 2);
        builder.Property(e => e.Observacoes).HasMaxLength(500);

        builder.HasOne(e => e.ExtratoImportado)
            .WithMany(ei => ei.Itens)
            .HasForeignKey(e => e.ExtratoImportadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.MovimentacaoFinanceira)
            .WithMany()
            .HasForeignKey(e => e.MovimentacaoFinanceiraId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ContaPagar)
            .WithMany()
            .HasForeignKey(e => e.ContaPagarId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ContaReceber)
            .WithMany()
            .HasForeignKey(e => e.ContaReceberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.EmpresaId, e.StatusConciliacao });
        builder.HasIndex(e => new { e.ExtratoImportadoId, e.TransacaoId });
    }
}
