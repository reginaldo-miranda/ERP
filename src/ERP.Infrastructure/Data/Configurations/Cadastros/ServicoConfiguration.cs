using ERP.Domain.Core.Entities.Cadastros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Cadastros;

public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("Servicos");
        builder.HasIndex(s => new { s.Codigo, s.EmpresaId }).IsUnique();
        builder.Property(s => s.Codigo).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Nome).HasMaxLength(200).IsRequired();
        builder.Property(s => s.PrecoBase).HasPrecision(18, 4);
        builder.Property(s => s.AliquotaIss).HasPrecision(5, 2);
        builder.Property(s => s.Unidade).HasMaxLength(30);
        builder.HasOne(s => s.Categoria).WithMany(c => c.Servicos).HasForeignKey(s => s.CategoriaId).OnDelete(DeleteBehavior.SetNull);
    }
}
