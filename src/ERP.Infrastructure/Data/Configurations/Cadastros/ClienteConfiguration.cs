using ERP.Domain.Core.Entities.Cadastros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Cadastros;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasIndex(c => new { c.CpfCnpj, c.EmpresaId }).IsUnique();
        builder.Property(c => c.Nome).HasMaxLength(200).IsRequired();
        builder.Property(c => c.CpfCnpj).HasMaxLength(18).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Telefone).HasMaxLength(20);
        builder.Property(c => c.Celular).HasMaxLength(20);
        builder.Property(c => c.NomeFantasia).HasMaxLength(200);
        builder.Property(c => c.InscricaoEstadual).HasMaxLength(30);
        builder.Property(c => c.LimiteCredito).HasPrecision(18, 2);
        builder.HasMany(c => c.Enderecos).WithOne(e => e.Cliente).HasForeignKey(e => e.ClienteId).OnDelete(DeleteBehavior.Cascade);
    }
}
