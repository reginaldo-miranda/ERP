using ERP.Domain.Core.Entities.Pdv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Pdv;

public class TerminalPdvConfiguration : IEntityTypeConfiguration<TerminalPdv>
{
    public void Configure(EntityTypeBuilder<TerminalPdv> builder)
    {
        builder.ToTable("TerminaisPdv");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Codigo).HasMaxLength(20).IsRequired();
        builder.Property(t => t.Nome).HasMaxLength(150).IsRequired();
        builder.Property(t => t.ImpressoraCupom).HasMaxLength(150);

        builder.HasOne(t => t.Deposito)
            .WithMany()
            .HasForeignKey(t => t.DepositoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ContaBancaria)
            .WithMany()
            .HasForeignKey(t => t.ContaBancariaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.EmpresaId, t.Codigo }).IsUnique();
    }
}

public class SessaoCaixaPdvConfiguration : IEntityTypeConfiguration<SessaoCaixaPdv>
{
    public void Configure(EntityTypeBuilder<SessaoCaixaPdv> builder)
    {
        builder.ToTable("SessoesCaixaPdv");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.OperadorId).HasMaxLength(100);
        builder.Property(s => s.OperadorNome).HasMaxLength(150).IsRequired();
        builder.Property(s => s.ValorAbertura).HasPrecision(18, 2);
        builder.Property(s => s.ValorFechamentoInformado).HasPrecision(18, 2);
        builder.Property(s => s.ValorFechamentoCalculado).HasPrecision(18, 2);
        builder.Property(s => s.DiferencaQuebra).HasPrecision(18, 2);
        builder.Property(s => s.ObservacoesFechamento).HasMaxLength(1000);

        builder.HasOne(s => s.TerminalPdv)
            .WithMany(t => t.Sessoes)
            .HasForeignKey(s => s.TerminalPdvId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Movimentos)
            .WithOne(m => m.SessaoCaixaPdv)
            .HasForeignKey(m => m.SessaoCaixaPdvId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.EmpresaId, s.TerminalPdvId, s.Status });
    }
}

public class MovimentoCaixaPdvConfiguration : IEntityTypeConfiguration<MovimentoCaixaPdv>
{
    public void Configure(EntityTypeBuilder<MovimentoCaixaPdv> builder)
    {
        builder.ToTable("MovimentosCaixaPdv");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Valor).HasPrecision(18, 2);
        builder.Property(m => m.Motivo).HasMaxLength(300).IsRequired();
        builder.Property(m => m.OperadorNome).HasMaxLength(150);
    }
}

public class VendaPdvConfiguration : IEntityTypeConfiguration<VendaPdv>
{
    public void Configure(EntityTypeBuilder<VendaPdv> builder)
    {
        builder.ToTable("VendasPdv");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Numero).HasMaxLength(30).IsRequired();
        builder.Property(v => v.ConsumidorNome).HasMaxLength(200);
        builder.Property(v => v.ConsumidorCpf).HasMaxLength(20);
        builder.Property(v => v.VendedorNome).HasMaxLength(150);
        builder.Property(v => v.ValorBruto).HasPrecision(18, 2);
        builder.Property(v => v.ValorDesconto).HasPrecision(18, 2);
        builder.Property(v => v.ValorTotal).HasPrecision(18, 2);
        builder.Property(v => v.Troco).HasPrecision(18, 2);
        builder.Property(v => v.Observacoes).HasMaxLength(500);

        builder.HasOne(v => v.TerminalPdv)
            .WithMany()
            .HasForeignKey(v => v.TerminalPdvId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.SessaoCaixaPdv)
            .WithMany(s => s.Vendas)
            .HasForeignKey(v => v.SessaoCaixaPdvId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Cliente)
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Vendedor)
            .WithMany()
            .HasForeignKey(v => v.VendedorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(v => v.Itens)
            .WithOne(i => i.VendaPdv)
            .HasForeignKey(i => i.VendaPdvId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Pagamentos)
            .WithOne(p => p.VendaPdv)
            .HasForeignKey(p => p.VendaPdvId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.EmpresaId, v.Numero }).IsUnique();
        builder.HasIndex(v => new { v.EmpresaId, v.Status });
        builder.HasIndex(v => v.SessaoCaixaPdvId);
    }
}

public class VendaPdvItemConfiguration : IEntityTypeConfiguration<VendaPdvItem>
{
    public void Configure(EntityTypeBuilder<VendaPdvItem> builder)
    {
        builder.ToTable("VendaPdvItens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.CodigoProduto).HasMaxLength(50).IsRequired();
        builder.Property(i => i.NomeProduto).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Quantidade).HasPrecision(18, 4);
        builder.Property(i => i.ValorUnitario).HasPrecision(18, 2);
        builder.Property(i => i.ValorDesconto).HasPrecision(18, 2);
        builder.Property(i => i.ValorTotal).HasPrecision(18, 2);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class VendaPdvPagamentoConfiguration : IEntityTypeConfiguration<VendaPdvPagamento>
{
    public void Configure(EntityTypeBuilder<VendaPdvPagamento> builder)
    {
        builder.ToTable("VendaPdvPagamentos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DescricaoForma).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Valor).HasPrecision(18, 2);

        builder.HasOne(p => p.FormaPagamento)
            .WithMany()
            .HasForeignKey(p => p.FormaPagamentoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
