namespace ERP.Application.Relatorios.DTOs;

#region Relatório de Vendas
public class RelatorioVendasItemDto
{
    public int PedidoId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string VendedorNome { get; set; } = string.Empty;
    public string FormaPagamento { get; set; } = string.Empty;
    public int QuantidadeItens { get; set; }
    public decimal ValorBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RelatorioVendasResultDto
{
    public List<RelatorioVendasItemDto> Itens { get; set; } = new();
    public int TotalPedidos { get; set; }
    public decimal ValorTotalFaturado { get; set; }
    public decimal ValorTotalDescontos { get; set; }
    public decimal TicketMedio { get; set; }
}
#endregion

#region Relatório Financeiro
public class RelatorioFinanceiroItemDto
{
    public int TituloId { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Receber" ou "Pagar"
    public string Documento { get; set; } = string.Empty;
    public DateTime DataEmissao { get; set; }
    public DateTime DataVencimento { get; set; }
    public string PessoaNome { get; set; } = string.Empty; // Cliente ou Fornecedor
    public string Descricao { get; set; } = string.Empty;
    public string CategoriaPlano { get; set; } = string.Empty;
    public decimal ValorOriginal { get; set; }
    public decimal ValorBaixado { get; set; }
    public decimal SaldoRestante { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Vencido { get; set; }
}

public class RelatorioFinanceiroResultDto
{
    public List<RelatorioFinanceiroItemDto> Itens { get; set; } = new();
    public decimal TotalOriginal { get; set; }
    public decimal TotalBaixado { get; set; }
    public decimal TotalEmAberto { get; set; }
    public decimal TotalVencido { get; set; }
    public int QuantidadeTitulos { get; set; }
}
#endregion

#region Relatório de Estoque
public class RelatorioEstoqueItemDto
{
    public int ProdutoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
    public decimal SaldoFisico { get; set; }
    public decimal CustoMedio { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal ValorTotalCusto { get; set; }
    public decimal ValorTotalVenda { get; set; }
    public decimal EstoqueMinimo { get; set; }
    public string StatusEstoque { get; set; } = "Normal"; // "Crítico" ou "Normal"
    public string ClassificacaoAbc { get; set; } = "C"; // "A", "B" ou "C"
    public decimal PercentualRepresentatividade { get; set; }
}

public class RelatorioEstoqueResultDto
{
    public List<RelatorioEstoqueItemDto> Itens { get; set; } = new();
    public int TotalItens { get; set; }
    public decimal QuantidadePecasTotal { get; set; }
    public decimal ValorTotalCustoEstoque { get; set; }
    public decimal ValorTotalPotencialVenda { get; set; }
    public int TotalProdutosCriticos { get; set; }
}
#endregion

#region Relatório de Compras
public class RelatorioComprasItemDto
{
    public int EntradaId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public string FornecedorNome { get; set; } = string.Empty;
    public string NumeroNotaFiscal { get; set; } = string.Empty;
    public decimal ValorProdutos { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorOutros { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RelatorioComprasResultDto
{
    public List<RelatorioComprasItemDto> Itens { get; set; } = new();
    public int TotalEntradas { get; set; }
    public decimal ValorTotalCompras { get; set; }
    public decimal ValorTotalFrete { get; set; }
    public decimal MediaPorCompra { get; set; }
}
#endregion

#region Relatório Fiscal
public class RelatorioFiscalItemDto
{
    public int NotaFiscalId { get; set; }
    public string Modelo { get; set; } = string.Empty; // 55 ou 65
    public int Serie { get; set; }
    public long Numero { get; set; }
    public DateTime DataEmissao { get; set; }
    public string DestinatarioNome { get; set; } = string.Empty;
    public string ChaveAcesso { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public decimal BaseCalculoIcms { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal ValorPis { get; set; }
    public decimal ValorCofins { get; set; }
    public decimal ValorIpi { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RelatorioFiscalResultDto
{
    public List<RelatorioFiscalItemDto> Itens { get; set; } = new();
    public int TotalNotas { get; set; }
    public decimal ValorTotalDocumentos { get; set; }
    public decimal TotalIcms { get; set; }
    public decimal TotalPis { get; set; }
    public decimal TotalCofins { get; set; }
    public decimal TotalIpi { get; set; }
    public decimal TotalTributosConsolidados { get; set; }
}
#endregion
