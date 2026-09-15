using ERP.Domain.Core.Entities.Cadastros;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Cadastros;

public class EnderecoConfiguration : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("Enderecos");
        builder.Property(e => e.Logradouro).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Numero).HasMaxLength(10);
        builder.Property(e => e.Complemento).HasMaxLength(100);
        builder.Property(e => e.Bairro).HasMaxLength(100);
        builder.Property(e => e.Cidade).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Uf).HasMaxLength(2).IsRequired();
        builder.Property(e => e.Cep).HasMaxLength(9);
    }
}
