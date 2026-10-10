using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Fiscal;

public class CartaCorrecaoConfiguration : IEntityTypeConfiguration<CartaCorrecao>
{
    public void Configure(EntityTypeBuilder<CartaCorrecao> builder)
    {
        builder.ToTable("CartasCorrecao");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CorrecaoTexto).HasMaxLength(1000).IsRequired();
        builder.Property(c => c.ProtocoloHomologacao).HasMaxLength(50);
        builder.Property(c => c.MotivoStatusSefaz).HasMaxLength(500);
        builder.Property(c => c.XmlEnvio).HasColumnType("text");
        builder.Property(c => c.XmlRetorno).HasColumnType("text");

        builder.HasIndex(c => new { c.NotaFiscalId, c.SequenciaEvento }).IsUnique();
        builder.HasIndex(c => c.EmpresaId);
    }
}
