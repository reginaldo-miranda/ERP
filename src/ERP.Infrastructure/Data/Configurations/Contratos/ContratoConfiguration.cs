using ERP.Domain.Core.Entities.Contratos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Contratos;

public class ContratoRecorrenteConfiguration : IEntityTypeConfiguration<ContratoRecorrente>
{
    public void Configure(EntityTypeBuilder<ContratoRecorrente> builder)
    {
        builder.ToTable("ContratosRecorrentes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Numero).HasMaxLength(30).IsRequired();
        builder.Property(c => c.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Descricao).HasMaxLength(2000);
        builder.Property(c => c.ValorMensal).HasPrecision(18, 2);
        builder.Property(c => c.PercentualReajustePadrao).HasPrecision(10, 4);
        builder.Property(c => c.IndiceReajuste).HasMaxLength(50);
        builder.Property(c => c.Observacoes).HasMaxLength(2000);

        builder.HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Servico)
            .WithMany()
            .HasForeignKey(c => c.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.FormaPagamento)
            .WithMany()
            .HasForeignKey(c => c.FormaPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Faturamentos)
            .WithOne(f => f.ContratoRecorrente)
            .HasForeignKey(f => f.ContratoRecorrenteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Reajustes)
            .WithOne(r => r.ContratoRecorrente)
            .HasForeignKey(r => r.ContratoRecorrenteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.EmpresaId, c.Numero }).IsUnique();
        builder.HasIndex(c => new { c.EmpresaId, c.Status, c.ProximaCobranca });
        builder.HasIndex(c => c.ClienteId);
    }
}

public class ContratoFaturamentoConfiguration : IEntityTypeConfiguration<ContratoFaturamento>
{
    public void Configure(EntityTypeBuilder<ContratoFaturamento> builder)
    {
        builder.ToTable("ContratoFaturamentos");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Competencia).HasMaxLength(20).IsRequired();
        builder.Property(f => f.ValorFaturado).HasPrecision(18, 2);

        builder.HasOne(f => f.ContaReceber)
            .WithMany()
            .HasForeignKey(f => f.ContaReceberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(f => f.ContratoRecorrenteId);
        builder.HasIndex(f => f.ContaReceberId);
    }
}

public class ContratoReajusteConfiguration : IEntityTypeConfiguration<ContratoReajuste>
{
    public void Configure(EntityTypeBuilder<ContratoReajuste> builder)
    {
        builder.ToTable("ContratoReajustes");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ValorAnterior).HasPrecision(18, 2);
        builder.Property(r => r.ValorNovo).HasPrecision(18, 2);
        builder.Property(r => r.PercentualAplicado).HasPrecision(10, 4);
        builder.Property(r => r.Indice).HasMaxLength(50);
        builder.Property(r => r.Observacao).HasMaxLength(500);

        builder.HasIndex(r => r.ContratoRecorrenteId);
    }
}
