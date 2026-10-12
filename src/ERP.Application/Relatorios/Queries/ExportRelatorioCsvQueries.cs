using ERP.Application.Relatorios.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using System.Globalization;
using System.Text;

namespace ERP.Application.Relatorios.Queries;

#region Query Records
public record ExportRelatorioVendasCsvQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? ClienteId = null,
    StatusPedidoVenda? Status = null
) : IRequest<byte[]>;

public record ExportRelatorioFinanceiroCsvQuery(
    DateTime DataInicio,
    DateTime DataFim,
    string Tipo = "Todos",
    StatusContaFinanceira? Status = null
) : IRequest<byte[]>;

public record ExportRelatorioEstoqueCsvQuery(
    int? CategoriaId = null,
    bool SomenteCriticos = false
) : IRequest<byte[]>;

public record ExportRelatorioComprasCsvQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? FornecedorId = null
) : IRequest<byte[]>;

public record ExportRelatorioFiscalCsvQuery(
    DateTime DataInicio,
    DateTime DataFim,
    StatusNotaFiscal? Status = null,
    ModeloDocumentoFiscal? Modelo = null
) : IRequest<byte[]>;
#endregion

public class ExportRelatorioCsvHandlers :
    IRequestHandler<ExportRelatorioVendasCsvQuery, byte[]>,
    IRequestHandler<ExportRelatorioFinanceiroCsvQuery, byte[]>,
    IRequestHandler<ExportRelatorioEstoqueCsvQuery, byte[]>,
    IRequestHandler<ExportRelatorioComprasCsvQuery, byte[]>,
    IRequestHandler<ExportRelatorioFiscalCsvQuery, byte[]>
{
    private readonly IMediator _mediator;
    private readonly CultureInfo _ptBr = new("pt-BR");

    public ExportRelatorioCsvHandlers(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<byte[]> Handle(ExportRelatorioVendasCsvQuery request, CancellationToken cancellationToken)
    {
        var dados = await _mediator.Send(new GetRelatorioVendasQuery(request.DataInicio, request.DataFim, request.ClienteId, request.Status), cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Número;Data;Cliente;Vendedor;Forma Pagamento;Qtd Itens;Valor Bruto;Desconto;Valor Total;Status");

        foreach (var item in dados.Itens)
        {
            sb.AppendLine($"{item.Numero};{item.DataPedido:dd/MM/yyyy HH:mm};\"{item.ClienteNome}\";\"{item.VendedorNome}\";\"{item.FormaPagamento}\";{item.QuantidadeItens};{item.ValorBruto.ToString("F2", _ptBr)};{item.ValorDesconto.ToString("F2", _ptBr)};{item.ValorTotal.ToString("F2", _ptBr)};{item.Status}");
        }

        return GerarBytesComBom(sb.ToString());
    }

    public async Task<byte[]> Handle(ExportRelatorioFinanceiroCsvQuery request, CancellationToken cancellationToken)
    {
        var dados = await _mediator.Send(new GetRelatorioFinanceiroQuery(request.DataInicio, request.DataFim, request.Tipo, request.Status), cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Tipo;Documento;Emissão;Vencimento;Pessoa / Razão Social;Descrição;Plano de Contas;Valor Original;Valor Baixado;Saldo Restante;Status;Vencido");

        foreach (var item in dados.Itens)
        {
            sb.AppendLine($"{item.Tipo};{item.Documento};{item.DataEmissao:dd/MM/yyyy};{item.DataVencimento:dd/MM/yyyy};\"{item.PessoaNome}\";\"{item.Descricao}\";\"{item.CategoriaPlano}\";{item.ValorOriginal.ToString("F2", _ptBr)};{item.ValorBaixado.ToString("F2", _ptBr)};{item.SaldoRestante.ToString("F2", _ptBr)};{item.Status};{(item.Vencido ? "SIM" : "NÃO")}");
        }

        return GerarBytesComBom(sb.ToString());
    }

    public async Task<byte[]> Handle(ExportRelatorioEstoqueCsvQuery request, CancellationToken cancellationToken)
    {
        var dados = await _mediator.Send(new GetRelatorioEstoqueQuery(request.CategoriaId, request.SomenteCriticos), cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Código;Produto;Categoria;Unidade;Saldo Físico;Custo Médio;Preço Venda;Valor Total Custo;Valor Total Venda;Estoque Mínimo;Status;Curva ABC;% Representatividade");

        foreach (var item in dados.Itens)
        {
            sb.AppendLine($"{item.Codigo};\"{item.Nome}\";\"{item.Categoria}\";{item.UnidadeMedida};{item.SaldoFisico.ToString("F2", _ptBr)};{item.CustoMedio.ToString("F2", _ptBr)};{item.PrecoVenda.ToString("F2", _ptBr)};{item.ValorTotalCusto.ToString("F2", _ptBr)};{item.ValorTotalVenda.ToString("F2", _ptBr)};{item.EstoqueMinimo.ToString("F2", _ptBr)};{item.StatusEstoque};{item.ClassificacaoAbc};{item.PercentualRepresentatividade.ToString("F2", _ptBr)}%");
        }

        return GerarBytesComBom(sb.ToString());
    }

    public async Task<byte[]> Handle(ExportRelatorioComprasCsvQuery request, CancellationToken cancellationToken)
    {
        var dados = await _mediator.Send(new GetRelatorioComprasQuery(request.DataInicio, request.DataFim, request.FornecedorId), cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Número Entrada;Data Entrada;Fornecedor;NF Fornecedor;Valor Produtos;Frete;Outras Despesas;Desconto;Valor Total;Status");

        foreach (var item in dados.Itens)
        {
            sb.AppendLine($"{item.Numero};{item.DataEntrada:dd/MM/yyyy};\"{item.FornecedorNome}\";{item.NumeroNotaFiscal};{item.ValorProdutos.ToString("F2", _ptBr)};{item.ValorFrete.ToString("F2", _ptBr)};{item.ValorOutros.ToString("F2", _ptBr)};{item.ValorDesconto.ToString("F2", _ptBr)};{item.ValorTotal.ToString("F2", _ptBr)};{item.Status}");
        }

        return GerarBytesComBom(sb.ToString());
    }

    public async Task<byte[]> Handle(ExportRelatorioFiscalCsvQuery request, CancellationToken cancellationToken)
    {
        var dados = await _mediator.Send(new GetRelatorioFiscalQuery(request.DataInicio, request.DataFim, request.Status, request.Modelo), cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Modelo;Série;Número;Data Emissão;Destinatário;Chave de Acesso;Valor Total;Base ICMS;Valor ICMS;Valor PIS;Valor COFINS;Valor IPI;Status");

        foreach (var item in dados.Itens)
        {
            sb.AppendLine($"Mod {item.Modelo};{item.Serie};{item.Numero};{item.DataEmissao:dd/MM/yyyy HH:mm};\"{item.DestinatarioNome}\";{item.ChaveAcesso};{item.ValorTotal.ToString("F2", _ptBr)};{item.BaseCalculoIcms.ToString("F2", _ptBr)};{item.ValorIcms.ToString("F2", _ptBr)};{item.ValorPis.ToString("F2", _ptBr)};{item.ValorCofins.ToString("F2", _ptBr)};{item.ValorIpi.ToString("F2", _ptBr)};{item.Status}");
        }

        return GerarBytesComBom(sb.ToString());
    }

    private static byte[] GerarBytesComBom(string csv)
    {
        var utf8 = new UTF8Encoding(true);
        return utf8.GetBytes(csv);
    }
}
