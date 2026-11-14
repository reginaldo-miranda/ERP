using ERP.Domain.Core.Entities.Comissoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Comissoes;

public class VendedorConfiguration : IEntityTypeConfiguration<Vendedor>
{
    public void Configure(EntityTypeBuilder<Vendedor> builder)
    {
        builder.ToTable("Vendedores");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.UsuarioId).HasMaxLength(100);
        builder.Property(v => v.Nome).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Email).HasMaxLength(200);
        builder.Property(v => v.Telefone).HasMaxLength(30);
        builder.Property(v => v.Cpf).HasMaxLength(20);
        builder.Property(v => v.PercentualPadrao).HasPrecision(10, 2);
        builder.Property(v => v.ValorFixoPadrao).HasPrecision(18, 2);
        builder.Property(v => v.Observacoes).HasMaxLength(1000);

        builder.HasIndex(v => new { v.EmpresaId, v.UsuarioId });
        builder.HasIndex(v => new { v.EmpresaId, v.Nome });
    }
}

public class RegraComissaoConfiguration : IEntityTypeConfiguration<RegraComissao>
{
    public void Configure(EntityTypeBuilder<RegraComissao> builder)
    {
        builder.ToTable("RegrasComissao");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nome).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Percentual).HasPrecision(10, 2);
        builder.Property(r => r.ValorFixo).HasPrecision(18, 2);
        builder.Property(r => r.ValorMinimoVenda).HasPrecision(18, 2);

        builder.HasOne(r => r.Vendedor)
            .WithMany(v => v.Regras)
            .HasForeignKey(r => r.VendedorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.EmpresaId, r.VendedorId });
    }
}

public class ComissaoVendaConfiguration : IEntityTypeConfiguration<ComissaoVenda>
{
    public void Configure(EntityTypeBuilder<ComissaoVenda> builder)
    {
        builder.ToTable("ComissoesVenda");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ValorBaseVenda).HasPrecision(18, 2);
        builder.Property(c => c.PercentualAplicado).HasPrecision(10, 2);
        builder.Property(c => c.ValorComissao).HasPrecision(18, 2);
        builder.Property(c => c.Observacoes).HasMaxLength(500);

        builder.HasOne(c => c.Vendedor)
            .WithMany(v => v.Comissoes)
            .HasForeignKey(c => c.VendedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PedidoVenda)
            .WithMany()
            .HasForeignKey(c => c.PedidoVendaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Faturamento)
            .WithMany()
            .HasForeignKey(c => c.FaturamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.RegraComissao)
            .WithMany()
            .HasForeignKey(c => c.RegraComissaoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => new { c.EmpresaId, c.PedidoVendaId }).IsUnique();
        builder.HasIndex(c => new { c.EmpresaId, c.VendedorId, c.Status });
        builder.HasIndex(c => c.DataVenda);
    }
}
