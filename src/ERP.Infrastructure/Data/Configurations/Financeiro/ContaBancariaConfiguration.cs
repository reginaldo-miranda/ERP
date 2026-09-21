using ERP.Domain.Core.Entities.Financeiro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Financeiro;

public class ContaBancariaConfiguration : IEntityTypeConfiguration<ContaBancaria>
{
    public void Configure(EntityTypeBuilder<ContaBancaria> builder)
    {
        builder.ToTable("ContasBancarias");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Descricao).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Agencia).HasMaxLength(20);
        builder.Property(c => c.AgenciaDigito).HasMaxLength(5);
        builder.Property(c => c.Conta).HasMaxLength(30);
        builder.Property(c => c.ContaDigito).HasMaxLength(5);
        builder.Property(c => c.SaldoInicial).HasPrecision(18, 2);
        builder.Property(c => c.SaldoAtual).HasPrecision(18, 2);
        builder.Property(c => c.Observacoes).HasMaxLength(500);

        builder.HasOne(c => c.Banco)
            .WithMany(b => b.ContasBancarias)
            .HasForeignKey(c => c.BancoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.EmpresaId, c.Descricao });
    }
}
