using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Fiscal;

public class RegraTributariaConfiguration : IEntityTypeConfiguration<RegraTributaria>
{
    public void Configure(EntityTypeBuilder<RegraTributaria> builder)
    {
        builder.ToTable("RegrasTributarias");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Descricao).HasMaxLength(200).IsRequired();
        builder.Property(r => r.NcmCodigo).HasMaxLength(8);
        builder.Property(r => r.CfopCodigo).HasMaxLength(4).IsRequired();
        builder.Property(r => r.UfOrigem).HasMaxLength(2).IsRequired();
        builder.Property(r => r.UfDestino).HasMaxLength(2).IsRequired();

        builder.Property(r => r.CstIcms).HasMaxLength(3);
        builder.Property(r => r.Csosn).HasMaxLength(4);
        builder.Property(r => r.AliquotaIcms).HasPrecision(18, 2);
        builder.Property(r => r.ReducaoBaseIcms).HasPrecision(18, 2);
        builder.Property(r => r.AliquotaIcmsSt).HasPrecision(18, 2);
        builder.Property(r => r.MvaIcmsSt).HasPrecision(18, 2);
        builder.Property(r => r.ReducaoBaseIcmsSt).HasPrecision(18, 2);

        builder.Property(r => r.CstPis).HasMaxLength(3);
        builder.Property(r => r.AliquotaPis).HasPrecision(18, 2);

        builder.Property(r => r.CstCofins).HasMaxLength(3);
        builder.Property(r => r.AliquotaCofins).HasPrecision(18, 2);

        builder.Property(r => r.CstIpi).HasMaxLength(3);
        builder.Property(r => r.AliquotaIpi).HasPrecision(18, 2);

        builder.Property(r => r.AliquotaIbs).HasPrecision(18, 2);
        builder.Property(r => r.AliquotaCbs).HasPrecision(18, 2);

        builder.Property(r => r.ObservacoesFiscais).HasMaxLength(1000);

        builder.HasIndex(r => new { r.EmpresaId, r.TipoOperacao, r.CfopCodigo });
        builder.HasIndex(r => new { r.EmpresaId, r.NcmCodigo });
        builder.HasIndex(r => new { r.EmpresaId, r.UfDestino });
    }
}
