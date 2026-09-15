using ERP.Domain.Core.Entities.Cadastros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Cadastros;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.Property(c => c.Nome).HasMaxLength(100).IsRequired();
        builder.HasOne(c => c.CategoriaPai).WithMany(c => c.Filhos).HasForeignKey(c => c.CategoriaPaiId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => new { c.Nome, c.EmpresaId, c.CategoriaPaiId });
    }
}
