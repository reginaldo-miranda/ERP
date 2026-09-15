using ERP.Domain.Core.Entities.Cadastros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Cadastros;

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedores");
        builder.HasIndex(f => new { f.Cnpj, f.EmpresaId }).IsUnique();
        builder.Property(f => f.RazaoSocial).HasMaxLength(200).IsRequired();
        builder.Property(f => f.NomeFantasia).HasMaxLength(200);
        builder.Property(f => f.Cnpj).HasMaxLength(14).IsRequired();
        builder.Property(f => f.Email).HasMaxLength(200);
        builder.Property(f => f.Telefone).HasMaxLength(20);
        builder.Property(f => f.Celular).HasMaxLength(20);
        builder.Property(f => f.Contato).HasMaxLength(100);
        builder.HasMany(f => f.Enderecos).WithOne(e => e.Fornecedor).HasForeignKey(e => e.FornecedorId).OnDelete(DeleteBehavior.Cascade);
    }
}
