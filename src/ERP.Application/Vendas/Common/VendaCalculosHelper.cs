using ERP.Application.Vendas.DTOs;
using ERP.Domain.Core.Enums;

namespace ERP.Application.Vendas.Common;

public static class VendaCalculosHelper
{
    public record ItemCalculado(
        decimal ValorDesconto,
        decimal SubTotal
    );

    public record TotaisVendaCalculados(
        decimal ValorBruto,
        decimal ValorDescontoGeral,
        decimal ValorTotal
    );

    public static ItemCalculado CalcularItem(
        decimal quantidade,
        decimal precoUnitario,
        TipoDescontoVenda descontoTipo,
        decimal desconto)
    {
        if (quantidade < 0) quantidade = 0;
        if (precoUnitario < 0) precoUnitario = 0;

        decimal totalBrutoItem = Math.Round(quantidade * precoUnitario, 2);
        decimal valorDesconto = 0m;

        if (desconto > 0)
        {
            if (descontoTipo == TipoDescontoVenda.Percentual)
            {
                decimal percentual = Math.Min(100m, Math.Max(0m, desconto));
                valorDesconto = Math.Round(totalBrutoItem * (percentual / 100m), 2);
            }
            else
            {
                valorDesconto = Math.Min(totalBrutoItem, Math.Round(desconto, 2));
            }
        }

        decimal subTotal = Math.Max(0m, totalBrutoItem - valorDesconto);
        return new ItemCalculado(valorDesconto, subTotal);
    }

    public static TotaisVendaCalculados CalcularTotais(
        IEnumerable<decimal> subTotaisItens,
        TipoDescontoVenda descontoGeralTipo,
        decimal descontoGeral)
    {
        decimal valorBruto = subTotaisItens.Sum();
        decimal valorDescontoGeral = 0m;

        if (descontoGeral > 0 && valorBruto > 0)
        {
            if (descontoGeralTipo == TipoDescontoVenda.Percentual)
            {
                decimal percentual = Math.Min(100m, Math.Max(0m, descontoGeral));
                valorDescontoGeral = Math.Round(valorBruto * (percentual / 100m), 2);
            }
            else
            {
                valorDescontoGeral = Math.Min(valorBruto, Math.Round(descontoGeral, 2));
            }
        }

        decimal valorTotal = Math.Max(0m, valorBruto - valorDescontoGeral);
        return new TotaisVendaCalculados(valorBruto, valorDescontoGeral, valorTotal);
    }
}
