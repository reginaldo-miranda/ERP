using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Fiscal;

public class XmlNotaFiscalConfiguration : IEntityTypeConfiguration<XmlNotaFiscal>
{
    public void Configure(EntityTypeBuilder<XmlNotaFiscal> builder)
    {
        builder.ToTable("XmlNotasFiscais");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.XmlConteudo).HasColumnType("text").IsRequired();
        builder.Property(x => x.ChaveAcesso).HasMaxLength(44);

        builder.HasIndex(x => x.NotaFiscalId);
        builder.HasIndex(x => new { x.EmpresaId, x.ChaveAcesso });
        builder.HasIndex(x => new { x.EmpresaId, x.TipoXml });
    }
}
