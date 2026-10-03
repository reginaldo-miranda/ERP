using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class ExtratoImportadoConfiguration : IEntityTypeConfiguration<ExtratoImportado>
{
    public void Configure(EntityTypeBuilder<ExtratoImportado> builder)
    {
        builder.ToTable("ExtratosImportados");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.NomeArquivo).HasMaxLength(255).IsRequired();
        builder.Property(e => e.TotalCreditos).HasPrecision(18, 2);
        builder.Property(e => e.TotalDebitos).HasPrecision(18, 2);
        builder.Property(e => e.Observacoes).HasMaxLength(500);

        builder.HasOne(e => e.ContaBancaria)
            .WithMany()
            .HasForeignKey(e => e.ContaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Itens)
            .WithOne(i => i.ExtratoImportado)
            .HasForeignKey(i => i.ExtratoImportadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.EmpresaId, e.ContaBancariaId });
        builder.HasIndex(e => new { e.EmpresaId, e.DataImportacao });
    }
}
