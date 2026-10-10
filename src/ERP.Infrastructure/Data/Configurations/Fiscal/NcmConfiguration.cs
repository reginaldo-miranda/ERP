using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Fiscal;

public class NcmConfiguration : IEntityTypeConfiguration<Ncm>
{
    public void Configure(EntityTypeBuilder<Ncm> builder)
    {
        builder.ToTable("Ncms");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Codigo).HasMaxLength(8).IsRequired();
        builder.Property(n => n.Descricao).HasMaxLength(500).IsRequired();

        builder.Property(n => n.AliquotaIpi).HasPrecision(18, 2);
        builder.Property(n => n.AliquotaII).HasPrecision(18, 2);
        builder.Property(n => n.AliquotaNacional).HasPrecision(18, 2);
        builder.Property(n => n.AliquotaImportado).HasPrecision(18, 2);
        builder.Property(n => n.AliquotaEstadual).HasPrecision(18, 2);
        builder.Property(n => n.AliquotaMunicipal).HasPrecision(18, 2);

        builder.HasIndex(n => n.Codigo).IsUnique();
        builder.HasIndex(n => n.Descricao);
    }
}
