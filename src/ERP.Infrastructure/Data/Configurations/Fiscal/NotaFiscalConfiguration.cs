using ERP.Domain.Core.Entities.Fiscal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Data.Configurations.Fiscal;

public class NotaFiscalConfiguration : IEntityTypeConfiguration<NotaFiscal>
{
    public void Configure(EntityTypeBuilder<NotaFiscal> builder)
    {
        builder.ToTable("NotasFiscais");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.ChaveAcesso).HasMaxLength(44).IsRequired();
        builder.Property(n => n.NaturezaOperacao).HasMaxLength(100).IsRequired();

        builder.Property(n => n.DestinatarioCpfCnpj).HasMaxLength(20).IsRequired();
        builder.Property(n => n.DestinatarioRazaoSocial).HasMaxLength(200).IsRequired();
        builder.Property(n => n.DestinatarioInscricaoEstadual).HasMaxLength(20);
        builder.Property(n => n.DestinatarioEmail).HasMaxLength(150);
        builder.Property(n => n.DestinatarioTelefone).HasMaxLength(30);

        builder.Property(n => n.DestinatarioLogradouro).HasMaxLength(200);
        builder.Property(n => n.DestinatarioNumero).HasMaxLength(20);
        builder.Property(n => n.DestinatarioComplemento).HasMaxLength(100);
        builder.Property(n => n.DestinatarioBairro).HasMaxLength(100);
        builder.Property(n => n.DestinatarioMunicipio).HasMaxLength(100);
        builder.Property(n => n.DestinatarioCodigoIbgeMunicipio).HasMaxLength(10);
        builder.Property(n => n.DestinatarioUf).HasMaxLength(2).IsRequired();
        builder.Property(n => n.DestinatarioCep).HasMaxLength(10);

        // Valores decimais
        builder.Property(n => n.ValorProdutos).HasPrecision(18, 2);
        builder.Property(n => n.ValorFrete).HasPrecision(18, 2);
        builder.Property(n => n.ValorSeguro).HasPrecision(18, 2);
        builder.Property(n => n.ValorDesconto).HasPrecision(18, 2);
        builder.Property(n => n.ValorOutrasDespesas).HasPrecision(18, 2);
        builder.Property(n => n.ValorTotalNota).HasPrecision(18, 2);

        builder.Property(n => n.BaseCalculoIcms).HasPrecision(18, 2);
        builder.Property(n => n.ValorIcms).HasPrecision(18, 2);
        builder.Property(n => n.BaseCalculoIcmsSt).HasPrecision(18, 2);
        builder.Property(n => n.ValorIcmsSt).HasPrecision(18, 2);

        builder.Property(n => n.ValorPis).HasPrecision(18, 2);
        builder.Property(n => n.ValorCofins).HasPrecision(18, 2);
        builder.Property(n => n.ValorIpi).HasPrecision(18, 2);
        builder.Property(n => n.ValorIbs).HasPrecision(18, 2);
        builder.Property(n => n.ValorCbs).HasPrecision(18, 2);
        builder.Property(n => n.ValorAproximadoTributos).HasPrecision(18, 2);

        builder.Property(n => n.ProtocoloAutorizacao).HasMaxLength(50);
        builder.Property(n => n.MotivoStatusSefaz).HasMaxLength(500);
        builder.Property(n => n.MotivoCancelamento).HasMaxLength(500);
        builder.Property(n => n.ProtocoloCancelamento).HasMaxLength(50);

        builder.Property(n => n.InformacoesComplementares).HasMaxLength(5000);
        builder.Property(n => n.ObservacoesFisco).HasMaxLength(2000);

        // Relacionamentos
        builder.HasOne(n => n.Cliente)
            .WithMany()
            .HasForeignKey(n => n.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Fornecedor)
            .WithMany()
            .HasForeignKey(n => n.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.PedidoVenda)
            .WithMany()
            .HasForeignKey(n => n.PedidoVendaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.EntradaMercadoria)
            .WithMany()
            .HasForeignKey(n => n.EntradaMercadoriaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(n => n.Itens)
            .WithOne(i => i.NotaFiscal)
            .HasForeignKey(i => i.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.Xmls)
            .WithOne(x => x.NotaFiscal)
            .HasForeignKey(x => x.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(n => n.CartasCorrecao)
            .WithOne(c => c.NotaFiscal)
            .HasForeignKey(c => c.NotaFiscalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(n => new { n.EmpresaId, n.Modelo, n.Serie, n.Numero }).IsUnique();
        builder.HasIndex(n => new { n.EmpresaId, n.ChaveAcesso }).IsUnique();
        builder.HasIndex(n => new { n.EmpresaId, n.Status });
        builder.HasIndex(n => new { n.EmpresaId, n.DataEmissao });
        builder.HasIndex(n => n.PedidoVendaId);
        builder.HasIndex(n => n.FaturamentoId);
        builder.HasIndex(n => n.EntradaMercadoriaId);
    }
}

public class NotaFiscalItemConfiguration : IEntityTypeConfiguration<NotaFiscalItem>
{
    public void Configure(EntityTypeBuilder<NotaFiscalItem> builder)
    {
        builder.ToTable("NotaFiscalItens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.CodigoProduto).HasMaxLength(60).IsRequired();
        builder.Property(i => i.DescricaoProduto).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Ncm).HasMaxLength(8).IsRequired();
        builder.Property(i => i.Cfop).HasMaxLength(4).IsRequired();
        builder.Property(i => i.UnidadeMedida).HasMaxLength(10).IsRequired();

        builder.Property(i => i.Quantidade).HasPrecision(18, 4);
        builder.Property(i => i.ValorUnitario).HasPrecision(18, 4);
        builder.Property(i => i.ValorTotalBruto).HasPrecision(18, 2);
        builder.Property(i => i.ValorDesconto).HasPrecision(18, 2);
        builder.Property(i => i.ValorFrete).HasPrecision(18, 2);
        builder.Property(i => i.ValorSeguro).HasPrecision(18, 2);
        builder.Property(i => i.ValorOutrasDespesas).HasPrecision(18, 2);
        builder.Property(i => i.ValorTotalLiquido).HasPrecision(18, 2);

        builder.Property(i => i.CstIcms).HasMaxLength(3);
        builder.Property(i => i.Csosn).HasMaxLength(4);
        builder.Property(i => i.BaseCalculoIcms).HasPrecision(18, 2);
        builder.Property(i => i.AliquotaIcms).HasPrecision(18, 2);
        builder.Property(i => i.ValorIcms).HasPrecision(18, 2);
        builder.Property(i => i.PercentualReducaoBaseIcms).HasPrecision(18, 2);

        builder.Property(i => i.BaseCalculoIcmsSt).HasPrecision(18, 2);
        builder.Property(i => i.AliquotaIcmsSt).HasPrecision(18, 2);
        builder.Property(i => i.ValorIcmsSt).HasPrecision(18, 2);
        builder.Property(i => i.PercentualMvaSt).HasPrecision(18, 2);

        builder.Property(i => i.CstPis).HasMaxLength(3);
        builder.Property(i => i.BaseCalculoPis).HasPrecision(18, 2);
        builder.Property(i => i.AliquotaPis).HasPrecision(18, 2);
        builder.Property(i => i.ValorPis).HasPrecision(18, 2);

        builder.Property(i => i.CstCofins).HasMaxLength(3);
        builder.Property(i => i.BaseCalculoCofins).HasPrecision(18, 2);
        builder.Property(i => i.AliquotaCofins).HasPrecision(18, 2);
        builder.Property(i => i.ValorCofins).HasPrecision(18, 2);

        builder.Property(i => i.CstIpi).HasMaxLength(3);
        builder.Property(i => i.BaseCalculoIpi).HasPrecision(18, 2);
        builder.Property(i => i.AliquotaIpi).HasPrecision(18, 2);
        builder.Property(i => i.ValorIpi).HasPrecision(18, 2);

        builder.Property(i => i.AliquotaIbs).HasPrecision(18, 2);
        builder.Property(i => i.ValorIbs).HasPrecision(18, 2);
        builder.Property(i => i.AliquotaCbs).HasPrecision(18, 2);
        builder.Property(i => i.ValorCbs).HasPrecision(18, 2);
        builder.Property(i => i.ValorAproximadoTributos).HasPrecision(18, 2);

        builder.HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.NotaFiscalId);
        builder.HasIndex(i => i.ProdutoId);
        builder.HasIndex(i => i.EmpresaId);
    }
}
