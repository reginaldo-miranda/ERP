using ERP.Domain.Core.Entities.Fiscal;
using ERP.Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Data;

public static class DbInitializerFiscal
{
    public static async Task SeedFiscalAsync(ApplicationDbContext context)
    {
        // 1. CFOPs Universais
        var cfopsIniciais = new List<Cfop>
        {
            new() { Codigo = "5102", Descricao = "Venda de mercadoria adquirida ou recebida de terceiros (Operação Interna)", TipoOperacao = TipoOperacaoFiscal.Saida, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Venda de produtos de terceiros para dentro do mesmo estado" },
            new() { Codigo = "5405", Descricao = "Venda de mercadoria adquirida de terceiros com ST (Operação Interna)", TipoOperacao = TipoOperacaoFiscal.Saida, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Venda de produtos com Substituição Tributária já recolhida anteriormente" },
            new() { Codigo = "6102", Descricao = "Venda de mercadoria adquirida de terceiros (Operação Interestadual)", TipoOperacao = TipoOperacaoFiscal.Saida, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Venda de produtos de terceiros para outro estado" },
            new() { Codigo = "6404", Descricao = "Venda de mercadoria com ST (Operação Interestadual)", TipoOperacao = TipoOperacaoFiscal.Saida, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Venda interestadual com ICMS-ST recolhido" },
            new() { Codigo = "1102", Descricao = "Compra para comercialização (Operação Interna)", TipoOperacao = TipoOperacaoFiscal.Entrada, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Entrada de compras para revenda dentro do estado" },
            new() { Codigo = "1403", Descricao = "Compra para comercialização com ST (Operação Interna)", TipoOperacao = TipoOperacaoFiscal.Entrada, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Entrada de compras com mercadoria sujeita a ICMS-ST" },
            new() { Codigo = "2102", Descricao = "Compra para comercialização (Operação Interestadual)", TipoOperacao = TipoOperacaoFiscal.Entrada, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Entrada de mercadorias adquiridas em outro estado" },
            new() { Codigo = "5929", Descricao = "Lançamento decorrente de emissão anterior de Cupom Fiscal", TipoOperacao = TipoOperacaoFiscal.Saida, GeraFinanceiro = false, MovimentaEstoque = false, Aplicacao = "Emissão de NF-e referenciando NFC-e ou ECF já emitido" },
            new() { Codigo = "5202", Descricao = "Devolução de compra para comercialização", TipoOperacao = TipoOperacaoFiscal.Saida, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Devolução de compras ao fornecedor interno" },
            new() { Codigo = "1202", Descricao = "Devolução de venda de mercadoria adquirida de terceiros", TipoOperacao = TipoOperacaoFiscal.Entrada, GeraFinanceiro = true, MovimentaEstoque = true, Aplicacao = "Devolução de mercadoria devolvida pelo cliente interno" }
        };

        foreach (var c in cfopsIniciais)
        {
            if (!await context.Cfops.AnyAsync(x => x.Codigo == c.Codigo))
            {
                context.Cfops.Add(c);
            }
        }
        await context.SaveChangesAsync();

        // 2. NCMs Universais mais frequentes
        var ncmsIniciais = new List<Ncm>
        {
            new() { Codigo = "84713012", Descricao = "Máquinas automáticas para processamento de dados, portáteis (Notebooks)", AliquotaIpi = 0, AliquotaII = 16, AliquotaNacional = 13.45m, AliquotaEstadual = 18m },
            new() { Codigo = "85171300", Descricao = "Smartphones e telefones móveis celulares", AliquotaIpi = 0, AliquotaII = 16, AliquotaNacional = 12.50m, AliquotaEstadual = 18m },
            new() { Codigo = "84716052", Descricao = "Teclados para máquinas de processamento de dados", AliquotaIpi = 0, AliquotaII = 16, AliquotaNacional = 14.10m, AliquotaEstadual = 18m },
            new() { Codigo = "84716053", Descricao = "Mouses para máquinas de processamento de dados", AliquotaIpi = 0, AliquotaII = 16, AliquotaNacional = 14.10m, AliquotaEstadual = 18m },
            new() { Codigo = "85285200", Descricao = "Monitores com conexão para computadores", AliquotaIpi = 0, AliquotaII = 16, AliquotaNacional = 15.00m, AliquotaEstadual = 18m },
            new() { Codigo = "84433210", Descricao = "Impressoras a laser ou jato de tinta", AliquotaIpi = 0, AliquotaII = 16, AliquotaNacional = 13.80m, AliquotaEstadual = 18m },
            new() { Codigo = "48025610", Descricao = "Papel sulfite formato A4 cortado", AliquotaIpi = 0, AliquotaII = 10, AliquotaNacional = 11.20m, AliquotaEstadual = 18m },
            new() { Codigo = "99999999", Descricao = "Outros produtos e mercadorias diversas", AliquotaIpi = 0, AliquotaII = 10, AliquotaNacional = 10.00m, AliquotaEstadual = 18m }
        };

        foreach (var n in ncmsIniciais)
        {
            if (!await context.Ncms.AnyAsync(x => x.Codigo == n.Codigo))
            {
                context.Ncms.Add(n);
            }
        }
        await context.SaveChangesAsync();

        // 3. Regime Tributário e Regras por Empresa
        var empresas = await context.Empresas.IgnoreQueryFilters().ToListAsync();
        foreach (var emp in empresas)
        {
            // Regime Tributário
            var regime = await context.RegimesTributarios
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.EmpresaId == emp.Id);

            if (regime == null)
            {
                regime = new RegimeTributario
                {
                    EmpresaId = emp.Id,
                    Regime = RegimeTributarioEmpresa.SimplesNacional,
                    Ambiente = AmbienteFiscal.Homologacao,
                    SerieNFePadrao = 1,
                    UltimoNumeroNFe = 0,
                    SerieNFCePadrao = 1,
                    UltimoNumeroNFCe = 0,
                    IdCscToken = "000001",
                    CodigoCsc = "HOMOLOGACAOCSC1234567890",
                    MensagemPadraoNotas = "Documento emitido por ME ou EPP optante pelo Simples Nacional. Nao gera direito a credito fiscal de IPI.",
                    PermiteCreditoIcms = false,
                    Ativo = true
                };
                context.RegimesTributarios.Add(regime);
                await context.SaveChangesAsync();
            }

            // Regras Tributárias Básicas Padrão
            var temRegras = await context.RegrasTributarias
                .IgnoreQueryFilters()
                .AnyAsync(r => r.EmpresaId == emp.Id);

            if (!temRegras)
            {
                context.RegrasTributarias.AddRange(
                    new RegraTributaria
                    {
                        EmpresaId = emp.Id,
                        Descricao = "Venda Interna Padrão - Simples Nacional",
                        TipoOperacao = TipoOperacaoFiscal.Saida,
                        CfopCodigo = "5102",
                        UfOrigem = "**",
                        UfDestino = emp.Uf ?? "SP",
                        Csosn = "102",
                        AliquotaIcms = 0,
                        CstPis = "49",
                        AliquotaPis = 0,
                        CstCofins = "49",
                        AliquotaCofins = 0,
                        ObservacoesFiscais = "Operacao tributada pelo Simples Nacional (CSOSN 102).",
                        Ativo = true
                    },
                    new RegraTributaria
                    {
                        EmpresaId = emp.Id,
                        Descricao = "Venda Interna com Substituição Tributária (ST)",
                        TipoOperacao = TipoOperacaoFiscal.Saida,
                        CfopCodigo = "5405",
                        UfOrigem = "**",
                        UfDestino = emp.Uf ?? "SP",
                        Csosn = "500",
                        AliquotaIcms = 0,
                        CstPis = "49",
                        AliquotaPis = 0,
                        CstCofins = "49",
                        AliquotaCofins = 0,
                        ObservacoesFiscais = "ICMS cobrado anteriormente por substituicao tributaria (CSOSN 500).",
                        Ativo = true
                    },
                    new RegraTributaria
                    {
                        EmpresaId = emp.Id,
                        Descricao = "Venda Interestadual - Simples Nacional",
                        TipoOperacao = TipoOperacaoFiscal.Saida,
                        CfopCodigo = "6102",
                        UfOrigem = "**",
                        UfDestino = "**",
                        Csosn = "102",
                        AliquotaIcms = 0,
                        CstPis = "49",
                        AliquotaPis = 0,
                        CstCofins = "49",
                        AliquotaCofins = 0,
                        ObservacoesFiscais = "Operacao interestadual sem destaque de ICMS - Simples Nacional.",
                        Ativo = true
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
