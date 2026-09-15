using ERP.Domain.Core.Entities.Cadastros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Cadastros;

public class UnidadeMedidaConfiguration : IEntityTypeConfiguration<UnidadeMedida>
{
    public void Configure(EntityTypeBuilder<UnidadeMedida> builder)
    {
        builder.ToTable("UnidadesMedida");
        builder.HasIndex(u => new { u.Sigla, u.EmpresaId }).IsUnique();
        builder.Property(u => u.Sigla).HasMaxLength(10).IsRequired();
        builder.Property(u => u.Descricao).HasMaxLength(100).IsRequired();
    }
}
