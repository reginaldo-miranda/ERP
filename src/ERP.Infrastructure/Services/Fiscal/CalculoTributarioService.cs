using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.Common;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Entities.Fiscal;
using ERP.Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services.Fiscal;

public class CalculoTributarioService : ICalculoTributarioService
{
    private readonly IApplicationDbContext _context;

    public CalculoTributarioService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CalculoTributarioTotalResultDto> CalcularTributosAsync(
        int empresaId,
        TipoOperacaoFiscal tipoOperacao,
        string ufDestino,
        IEnumerable<CalculoItemInputDto> itens,
        CancellationToken cancellationToken = default)
    {
        var total = new CalculoTributarioTotalResultDto();
        var listaItens = itens.ToList();
        var observacoesSugeridas = new HashSet<string>();

        for (int i = 0; i < listaItens.Count; i++)
        {
            var item = listaItens[i];
            var itemCalc = await CalcularItemAsync(empresaId, tipoOperacao, ufDestino, i + 1, item, cancellationToken);
            total.Itens.Add(itemCalc);

            total.ValorProdutos += itemCalc.ValorTotalBruto;
            total.ValorFrete += itemCalc.ValorFrete;
            total.ValorSeguro += itemCalc.ValorSeguro;
            total.ValorDesconto += itemCalc.ValorDesconto;
            total.ValorOutrasDespesas += itemCalc.ValorOutrasDespesas;
            total.ValorTotalNota += itemCalc.ValorTotalLiquido;

            total.BaseCalculoIcms += itemCalc.BaseCalculoIcms;
            total.ValorIcms += itemCalc.ValorIcms;
            total.BaseCalculoIcmsSt += itemCalc.BaseCalculoIcmsSt;
            total.ValorIcmsSt += itemCalc.ValorIcmsSt;

            total.ValorPis += itemCalc.ValorPis;
            total.ValorCofins += itemCalc.ValorCofins;
            total.ValorIpi += itemCalc.ValorIpi;

            total.ValorIbs += itemCalc.ValorIbs;
            total.ValorCbs += itemCalc.ValorCbs;
            total.ValorAproximadoTributos += itemCalc.ValorAproximadoTributos;
        }

        // Recuperar Regime da Empresa para sugestão de mensagens fiscais
        var regime = await _context.RegimesTributarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId, cancellationToken);

        if (regime != null && !string.IsNullOrWhiteSpace(regime.MensagemPadraoNotas))
        {
            observacoesSugeridas.Add(regime.MensagemPadraoNotas);
        }

        if (total.ValorAproximadoTributos > 0)
        {
            observacoesSugeridas.Add($"Valor aprox. tributos: R$ {total.ValorAproximadoTributos:N2} (Fonte: IBPT/Lei 12.741/2012).");
        }

        total.InformacoesComplementaresSugeridas = string.Join(" ", observacoesSugeridas);

        return total;
    }

    public async Task<CalculoTributarioItemResultDto> CalcularItemAsync(
        int empresaId,
        TipoOperacaoFiscal tipoOperacao,
        string ufDestino,
        int numeroItem,
        CalculoItemInputDto item,
        CancellationToken cancellationToken = default)
    {
        string ncmNormalizado = !string.IsNullOrWhiteSpace(item.Ncm) ? item.Ncm.Replace(".", "").Trim() : "00000000";
        string cfopNormalizado = !string.IsNullOrWhiteSpace(item.Cfop) ? item.Cfop.Trim() : (tipoOperacao == TipoOperacaoFiscal.Saida ? "5102" : "1102");
        string ufDestinoNormalizada = string.IsNullOrWhiteSpace(ufDestino) ? "SP" : ufDestino.ToUpper().Trim();

        // 1. Carregar Regras da Empresa
        var regras = await _context.RegrasTributarias
            .IgnoreQueryFilters()
            .Where(r => r.EmpresaId == empresaId && r.Ativo && r.TipoOperacao == tipoOperacao)
            .ToListAsync(cancellationToken);

        // 2. Busca hierárquica por prioridade
        var regra = regras.FirstOrDefault(r => r.NcmCodigo == ncmNormalizado && (r.UfDestino == ufDestinoNormalizada || r.UfDestino == "**"))
                 ?? regras.FirstOrDefault(r => r.CfopCodigo == cfopNormalizado && r.UfDestino == ufDestinoNormalizada)
                 ?? regras.FirstOrDefault(r => r.CfopCodigo == cfopNormalizado && r.UfDestino == "**")
                 ?? regras.FirstOrDefault();

        // 3. Regime Tributário da empresa para defaults
        var regimeEmpresa = await _context.RegimesTributarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId, cancellationToken);

        bool ehSimples = regimeEmpresa?.Regime == RegimeTributarioEmpresa.SimplesNacional ||
                         regimeEmpresa?.Regime == RegimeTributarioEmpresa.SimplesNacionalExcesso ||
                         regimeEmpresa?.Regime == RegimeTributarioEmpresa.MEI ||
                         regimeEmpresa == null;

        decimal valorBruto = Math.Round(item.Quantidade * item.ValorUnitario, 2);
        decimal valorLiquido = Math.Max(0, Math.Round(valorBruto - item.Desconto + item.Frete + item.Seguro + item.OutrasDespesas, 2));

        var result = new CalculoTributarioItemResultDto
        {
            NumeroItem = numeroItem,
            ProdutoId = item.ProdutoId,
            CodigoProduto = item.Codigo,
            DescricaoProduto = item.Descricao,
            Ncm = ncmNormalizado,
            Cfop = regra?.CfopCodigo ?? cfopNormalizado,
            UnidadeMedida = string.IsNullOrWhiteSpace(item.UnidadeMedida) ? "UN" : item.UnidadeMedida,
            Quantidade = item.Quantidade,
            ValorUnitario = item.ValorUnitario,
            ValorTotalBruto = valorBruto,
            ValorDesconto = item.Desconto,
            ValorFrete = item.Frete,
            ValorSeguro = item.Seguro,
            ValorOutrasDespesas = item.OutrasDespesas,
            ValorTotalLiquido = valorLiquido,
            OrigemMercadoria = item.OrigemMercadoria
        };

        if (ehSimples)
        {
            result.Csosn = regra?.Csosn ?? "102";
            result.CstIcms = null;

            if (result.Csosn == "101" || result.Csosn == "201")
            {
                result.AliquotaIcms = regra?.AliquotaIcms ?? regimeEmpresa?.AliquotaCreditoSimples ?? 0;
                result.BaseCalculoIcms = result.AliquotaIcms > 0 ? valorLiquido : 0;
                result.ValorIcms = Math.Round(result.BaseCalculoIcms * (result.AliquotaIcms / 100m), 2);
            }
            else
            {
                result.AliquotaIcms = 0;
                result.BaseCalculoIcms = 0;
                result.ValorIcms = 0;
            }
        }
        else
        {
            result.CstIcms = regra?.CstIcms ?? "00";
            result.Csosn = null;
            result.AliquotaIcms = regra?.AliquotaIcms ?? 18m;
            result.PercentualReducaoBaseIcms = regra?.ReducaoBaseIcms ?? 0;

            decimal fatorReducao = 1m - (result.PercentualReducaoBaseIcms / 100m);
            result.BaseCalculoIcms = Math.Round(valorLiquido * fatorReducao, 2);
            result.ValorIcms = Math.Round(result.BaseCalculoIcms * (result.AliquotaIcms / 100m), 2);
        }

        // ICMS-ST
        if (regra != null && regra.AliquotaIcmsSt > 0)
        {
            result.AliquotaIcmsSt = regra.AliquotaIcmsSt;
            result.PercentualMvaSt = regra.MvaIcmsSt;
            decimal fatorMva = 1m + (regra.MvaIcmsSt / 100m);
            decimal fatorReducaoSt = 1m - (regra.ReducaoBaseIcmsSt / 100m);

            result.BaseCalculoIcmsSt = Math.Round(valorLiquido * fatorMva * fatorReducaoSt, 2);
            decimal icmsStBruto = Math.Round(result.BaseCalculoIcmsSt * (result.AliquotaIcmsSt / 100m), 2);
            result.ValorIcmsSt = Math.Max(0, icmsStBruto - result.ValorIcms);
        }

        // PIS & COFINS
        result.CstPis = regra?.CstPis ?? (ehSimples ? "49" : "01");
        result.AliquotaPis = regra?.AliquotaPis ?? (ehSimples ? 0 : 1.65m);
        result.BaseCalculoPis = result.AliquotaPis > 0 ? valorLiquido : 0;
        result.ValorPis = Math.Round(result.BaseCalculoPis * (result.AliquotaPis / 100m), 2);

        result.CstCofins = regra?.CstCofins ?? (ehSimples ? "49" : "01");
        result.AliquotaCofins = regra?.AliquotaCofins ?? (ehSimples ? 0 : 7.60m);
        result.BaseCalculoCofins = result.AliquotaCofins > 0 ? valorLiquido : 0;
        result.ValorCofins = Math.Round(result.BaseCalculoCofins * (result.AliquotaCofins / 100m), 2);

        // IPI
        result.CstIpi = regra?.CstIpi ?? "99";
        result.AliquotaIpi = regra?.AliquotaIpi ?? 0;
        result.BaseCalculoIpi = result.AliquotaIpi > 0 ? valorLiquido : 0;
        result.ValorIpi = Math.Round(result.BaseCalculoIpi * (result.AliquotaIpi / 100m), 2);

        // Reforma Tributária (IBS e CBS)
        result.AliquotaIbs = regra?.AliquotaIbs ?? 0;
        result.ValorIbs = Math.Round(valorLiquido * (result.AliquotaIbs / 100m), 2);
        result.AliquotaCbs = regra?.AliquotaCbs ?? 0;
        result.ValorCbs = Math.Round(valorLiquido * (result.AliquotaCbs / 100m), 2);

        // IBPT (Transparência Lei 12.741/2012)
        var ncmDb = await _context.Ncms
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(n => n.Codigo == ncmNormalizado, cancellationToken);

        if (ncmDb != null && (ncmDb.AliquotaNacional > 0 || ncmDb.AliquotaEstadual > 0))
        {
            decimal aliqTotalIbpt = ncmDb.AliquotaNacional + ncmDb.AliquotaEstadual;
            result.ValorAproximadoTributos = Math.Round(valorLiquido * (aliqTotalIbpt / 100m), 2);
        }
        else
        {
            result.ValorAproximadoTributos = Math.Round(valorLiquido * 0.15m, 2); // 15% estimativa média
        }

        return result;
    }
}
