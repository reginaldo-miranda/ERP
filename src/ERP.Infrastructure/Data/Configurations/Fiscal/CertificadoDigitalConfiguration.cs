using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Fiscal;

public class CertificadoDigitalConfiguration : IEntityTypeConfiguration<CertificadoDigital>
{
    public void Configure(EntityTypeBuilder<CertificadoDigital> builder)
    {
        builder.ToTable("CertificadosDigitais");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.NomeArquivo).HasMaxLength(255).IsRequired();
        builder.Property(c => c.ArquivoBytes).IsRequired();
        builder.Property(c => c.SenhaCriptografada).HasMaxLength(500).IsRequired();

        builder.Property(c => c.CnpjOuCpfTitular).HasMaxLength(20).IsRequired();
        builder.Property(c => c.RazaoSocialTitular).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Thumbprint).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Emissor).HasMaxLength(255).IsRequired();

        builder.HasIndex(c => new { c.EmpresaId, c.Thumbprint });
        builder.HasIndex(c => new { c.EmpresaId, c.ValidadeFim });
    }
}
