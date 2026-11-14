using ERP.Domain.Core.Entities.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Servicos;

public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("OrdensServico");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Numero).HasMaxLength(30).IsRequired();
        builder.Property(o => o.TecnicoResponsavel).HasMaxLength(150);
        builder.Property(o => o.Equipamento).HasMaxLength(150).IsRequired();
        builder.Property(o => o.Marca).HasMaxLength(100);
        builder.Property(o => o.Modelo).HasMaxLength(100);
        builder.Property(o => o.NumeroSerie).HasMaxLength(100);
        builder.Property(o => o.Acessorios).HasMaxLength(500);
        builder.Property(o => o.CondicoesGerais).HasMaxLength(500);
        builder.Property(o => o.DefeitoRelatado).HasMaxLength(2000).IsRequired();
        builder.Property(o => o.LaudoTecnico).HasMaxLength(4000);
        builder.Property(o => o.SolucaoTecnica).HasMaxLength(4000);
        builder.Property(o => o.Observacoes).HasMaxLength(1000);
        builder.Property(o => o.TermoGarantia).HasMaxLength(2000);
        builder.Property(o => o.MotivoCancelamento).HasMaxLength(500);

        builder.Property(o => o.ValorServicos).HasPrecision(18, 2);
        builder.Property(o => o.ValorPecas).HasPrecision(18, 2);
        builder.Property(o => o.ValorDesconto).HasPrecision(18, 2);
        builder.Property(o => o.ValorTotal).HasPrecision(18, 2);

        builder.HasOne(o => o.Cliente)
            .WithMany()
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.FormaPagamento)
            .WithMany()
            .HasForeignKey(o => o.FormaPagamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.ItensPecas)
            .WithOne(p => p.OrdemServico)
            .HasForeignKey(p => p.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.ItensServicos)
            .WithOne(s => s.OrdemServico)
            .HasForeignKey(s => s.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.HistoricoStatus)
            .WithOne(h => h.OrdemServico)
            .HasForeignKey(h => h.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.EmpresaId, o.Numero }).IsUnique();
        builder.HasIndex(o => new { o.EmpresaId, o.Status });
        builder.HasIndex(o => o.ClienteId);
        builder.HasIndex(o => o.Equipamento);
        builder.HasIndex(o => o.NumeroSerie);
    }
}

public class OrdemServicoPecaConfiguration : IEntityTypeConfiguration<OrdemServicoPeca>
{
    public void Configure(EntityTypeBuilder<OrdemServicoPeca> builder)
    {
        builder.ToTable("OrdemServicoPecas");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Descricao).HasMaxLength(250).IsRequired();
        builder.Property(p => p.Quantidade).HasPrecision(18, 4);
        builder.Property(p => p.ValorUnitario).HasPrecision(18, 2);
        builder.Property(p => p.ValorDesconto).HasPrecision(18, 2);
        builder.Property(p => p.ValorTotal).HasPrecision(18, 2);
        builder.Property(p => p.CustoUnitario).HasPrecision(18, 4);

        builder.HasOne(p => p.Produto)
            .WithMany()
            .HasForeignKey(p => p.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Deposito)
            .WithMany()
            .HasForeignKey(p => p.DepositoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.OrdemServicoId);
        builder.HasIndex(p => p.ProdutoId);
    }
}

public class OrdemServicoServicoConfiguration : IEntityTypeConfiguration<OrdemServicoServico>
{
    public void Configure(EntityTypeBuilder<OrdemServicoServico> builder)
    {
        builder.ToTable("OrdemServicoServicos");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Descricao).HasMaxLength(250).IsRequired();
        builder.Property(s => s.Quantidade).HasPrecision(18, 4);
        builder.Property(s => s.ValorUnitario).HasPrecision(18, 2);
        builder.Property(s => s.ValorDesconto).HasPrecision(18, 2);
        builder.Property(s => s.ValorTotal).HasPrecision(18, 2);
        builder.Property(s => s.Tecnico).HasMaxLength(150);

        builder.HasOne(s => s.Servico)
            .WithMany()
            .HasForeignKey(s => s.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.OrdemServicoId);
        builder.HasIndex(s => s.ServicoId);
    }
}

public class OrdemServicoHistoricoConfiguration : IEntityTypeConfiguration<OrdemServicoHistorico>
{
    public void Configure(EntityTypeBuilder<OrdemServicoHistorico> builder)
    {
        builder.ToTable("OrdemServicoHistoricos");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.UsuarioNome).HasMaxLength(150);
        builder.Property(h => h.Observacao).HasMaxLength(1000);

        builder.HasIndex(h => h.OrdemServicoId);
        builder.HasIndex(h => h.Data);
    }
}

