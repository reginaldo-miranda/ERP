using ERP.Application.Relatorios.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using Unit = QuestPDF.Infrastructure.Unit;

namespace ERP.Application.Relatorios.Queries;

#region Query Records
public record ExportRelatorioVendasPdfQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? ClienteId = null,
    StatusPedidoVenda? Status = null
) : IRequest<byte[]>;

public record ExportRelatorioFinanceiroPdfQuery(
    DateTime DataInicio,
    DateTime DataFim,
    string Tipo = "Todos",
    StatusContaFinanceira? Status = null
) : IRequest<byte[]>;

public record ExportRelatorioEstoquePdfQuery(
    int? CategoriaId = null,
    bool SomenteCriticos = false
) : IRequest<byte[]>;

public record ExportRelatorioComprasPdfQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? FornecedorId = null
) : IRequest<byte[]>;

public record ExportRelatorioFiscalPdfQuery(
    DateTime DataInicio,
    DateTime DataFim,
    StatusNotaFiscal? Status = null,
    ModeloDocumentoFiscal? Modelo = null
) : IRequest<byte[]>;
#endregion

public class ExportRelatorioPdfHandlers :
    IRequestHandler<ExportRelatorioVendasPdfQuery, byte[]>,
    IRequestHandler<ExportRelatorioFinanceiroPdfQuery, byte[]>,
    IRequestHandler<ExportRelatorioEstoquePdfQuery, byte[]>,
    IRequestHandler<ExportRelatorioComprasPdfQuery, byte[]>,
    IRequestHandler<ExportRelatorioFiscalPdfQuery, byte[]>
{
    private readonly IMediator _mediator;
    private readonly CultureInfo _ptBr = new("pt-BR");

    public ExportRelatorioPdfHandlers(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<byte[]> Handle(ExportRelatorioVendasPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var dados = await _mediator.Send(new GetRelatorioVendasQuery(request.DataInicio, request.DataFim, request.ClienteId, request.Status), cancellationToken);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                page.Header().Element(c => MontarCabecalho(c, "RELATÓRIO GERENCIAL DE VENDAS", $"Período: {request.DataInicio:dd/MM/yyyy} até {request.DataFim:dd/MM/yyyy}"));

                page.Content().Column(col =>
                {
                    // Cards de Totais
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL DE PEDIDOS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{dados.TotalPedidos}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("FATURAMENTO TOTAL").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.ValorTotalFaturado.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL DESCONTOS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.ValorTotalDescontos.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Orange.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TICKET MÉDIO").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.TicketMedio.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Purple.Darken2);
                        });
                    });

                    // Tabela de Itens
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);  // Número
                            columns.ConstantColumn(80);  // Data
                            columns.RelativeColumn(3);   // Cliente
                            columns.RelativeColumn(2);   // Vendedor
                            columns.RelativeColumn(2);   // Forma Pagto
                            columns.ConstantColumn(40);  // Itens
                            columns.ConstantColumn(75);  // Valor Total
                            columns.ConstantColumn(65);  // Status
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Número").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Data").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Cliente").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Vendedor").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Forma Pagto").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Itens").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Total").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text("Status").Bold();
                        });

                        foreach (var item in dados.Itens)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Numero);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.DataPedido.ToString("dd/MM/yyyy"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.ClienteNome);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.VendedorNome);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.FormaPagamento);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"{item.QuantidadeItens}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorTotal.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(item.Status);
                        }
                    });
                });

                page.Footer().Element(MontarRodape);
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<byte[]> Handle(ExportRelatorioFinanceiroPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var dados = await _mediator.Send(new GetRelatorioFinanceiroQuery(request.DataInicio, request.DataFim, request.Tipo, request.Status), cancellationToken);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                page.Header().Element(c => MontarCabecalho(c, "RELATÓRIO FINANCEIRO (CONTAS A PAGAR / RECEBER)", $"Período: {request.DataInicio:dd/MM/yyyy} até {request.DataFim:dd/MM/yyyy} | Filtro: {request.Tipo}"));

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL DE TÍTULOS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{dados.QuantidadeTitulos}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("VALOR TOTAL ORIGINAL").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.TotalOriginal.ToString("C2", _ptBr)).Bold().FontSize(12);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL BAIXADO").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.TotalBaixado.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("SALDO EM ABERTO").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.TotalEmAberto.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Orange.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL VENCIDO").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.TotalVencido.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Red.Darken2);
                        });
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(55);  // Tipo
                            columns.ConstantColumn(75);  // Documento
                            columns.ConstantColumn(65);  // Vencimento
                            columns.RelativeColumn(3);   // Pessoa
                            columns.RelativeColumn(3);   // Descrição
                            columns.ConstantColumn(70);  // Original
                            columns.ConstantColumn(70);  // Baixado
                            columns.ConstantColumn(70);  // Saldo
                            columns.ConstantColumn(60);  // Status
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Tipo").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Doc.").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Vencimento").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Cliente / Fornecedor").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Descrição").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Original").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Baixado").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Saldo").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text("Status").Bold();
                        });

                        foreach (var item in dados.Itens)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Tipo);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Documento);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.DataVencimento.ToString("dd/MM/yyyy"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.PessoaNome);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Descricao);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorOriginal.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorBaixado.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.SaldoRestante.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(item.Status);
                        }
                    });
                });

                page.Footer().Element(MontarRodape);
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<byte[]> Handle(ExportRelatorioEstoquePdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var dados = await _mediator.Send(new GetRelatorioEstoqueQuery(request.CategoriaId, request.SomenteCriticos), cancellationToken);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                page.Header().Element(c => MontarCabecalho(c, "POSIÇÃO FÍSICO-FINANCEIRA DE ESTOQUE & CURVA ABC", $"Data de Emissão: {DateTime.Now:dd/MM/yyyy HH:mm}"));

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("ITENS DISTINTOS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{dados.TotalItens}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("VALOR CUSTO TOTAL").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.ValorTotalCustoEstoque.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("POTENCIAL DE VENDA").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.ValorTotalPotencialVenda.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Teal.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("PRODUTOS CRÍTICOS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{dados.TotalProdutosCriticos}").Bold().FontSize(12).FontColor(Colors.Red.Darken2);
                        });
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(65);  // Código
                            columns.RelativeColumn(3);   // Produto
                            columns.RelativeColumn(2);   // Categoria
                            columns.ConstantColumn(35);  // UN
                            columns.ConstantColumn(50);  // Saldo
                            columns.ConstantColumn(65);  // Custo Médio
                            columns.ConstantColumn(75);  // Total Custo
                            columns.ConstantColumn(65);  // Preço Venda
                            columns.ConstantColumn(75);  // Total Venda
                            columns.ConstantColumn(30);  // ABC
                            columns.ConstantColumn(55);  // Status
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Código").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Produto").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Categoria").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text("UN").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Saldo").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Custo Unit").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Total Custo").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Preço").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Total Venda").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text("ABC").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text("Status").Bold();
                        });

                        foreach (var item in dados.Itens)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Codigo);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Nome);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Categoria);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(item.UnidadeMedida);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.SaldoFisico.ToString("N0", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.CustoMedio.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorTotalCusto.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.PrecoVenda.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorTotalVenda.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(item.ClassificacaoAbc);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(item.StatusEstoque);
                        }
                    });
                });

                page.Footer().Element(MontarRodape);
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<byte[]> Handle(ExportRelatorioComprasPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var dados = await _mediator.Send(new GetRelatorioComprasQuery(request.DataInicio, request.DataFim, request.FornecedorId), cancellationToken);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                page.Header().Element(c => MontarCabecalho(c, "RELATÓRIO DE COMPRAS & ENTRADA DE MERCADORIAS", $"Período: {request.DataInicio:dd/MM/yyyy} até {request.DataFim:dd/MM/yyyy}"));

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL ENTRADAS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{dados.TotalEntradas}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL EM COMPRAS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.ValorTotalCompras.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL FRETE").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.ValorTotalFrete.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Orange.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("MÉDIA POR COMPRA").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.MediaPorCompra.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Purple.Darken2);
                        });
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(85);  // Número
                            columns.ConstantColumn(70);  // Data
                            columns.RelativeColumn(3);   // Fornecedor
                            columns.ConstantColumn(80);  // NF
                            columns.ConstantColumn(75);  // Produtos
                            columns.ConstantColumn(65);  // Frete
                            columns.ConstantColumn(80);  // Total
                            columns.ConstantColumn(70);  // Status
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Número").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Data").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Fornecedor").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("NF Fornec.").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Produtos").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Frete").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Total").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text("Status").Bold();
                        });

                        foreach (var item in dados.Itens)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Numero);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.DataEntrada.ToString("dd/MM/yyyy"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.FornecedorNome);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.NumeroNotaFiscal);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorProdutos.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorFrete.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorTotal.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(item.Status);
                        }
                    });
                });

                page.Footer().Element(MontarRodape);
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<byte[]> Handle(ExportRelatorioFiscalPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var dados = await _mediator.Send(new GetRelatorioFiscalQuery(request.DataInicio, request.DataFim, request.Status, request.Modelo), cancellationToken);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                page.Header().Element(c => MontarCabecalho(c, "RELATÓRIO FISCAL & APURAÇÃO DE TRIBUTOS", $"Período: {request.DataInicio:dd/MM/yyyy} até {request.DataFim:dd/MM/yyyy}"));

                page.Content().Column(col =>
                {
                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("DOCUMENTOS EMITIDOS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{dados.TotalNotas}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("VALOR TOTAL FISCAL").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.ValorTotalDocumentos.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Green.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TOTAL ICMS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.TotalIcms.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Teal.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("PIS + COFINS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text((dados.TotalPis + dados.TotalCofins).ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Orange.Darken2);
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("TRIBUTOS CONSOLIDADOS").FontSize(8).FontColor(Colors.Grey.Medium);
                            c.Item().Text(dados.TotalTributosConsolidados.ToString("C2", _ptBr)).Bold().FontSize(12).FontColor(Colors.Red.Darken2);
                        });
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(45);  // Mod
                            columns.ConstantColumn(35);  // Série
                            columns.ConstantColumn(65);  // Número
                            columns.ConstantColumn(65);  // Data
                            columns.RelativeColumn(3);   // Destinatário
                            columns.ConstantColumn(75);  // Valor Total
                            columns.ConstantColumn(65);  // ICMS
                            columns.ConstantColumn(55);  // PIS
                            columns.ConstantColumn(55);  // COFINS
                            columns.ConstantColumn(55);  // IPI
                            columns.ConstantColumn(65);  // Status
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Mod").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Série").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Número").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Data").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Destinatário").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Total").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("ICMS").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("PIS").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("COFINS").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("IPI").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text("Status").Bold();
                        });

                        foreach (var item in dados.Itens)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.Modelo);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{item.Serie}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text($"{item.Numero}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.DataEmissao.ToString("dd/MM/yyyy"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(item.DestinatarioNome);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorTotal.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorIcms.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorPis.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorCofins.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(item.ValorIpi.ToString("C2", _ptBr));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text(item.Status);
                        }
                    });
                });

                page.Footer().Element(MontarRodape);
            });
        });

        return doc.GeneratePdf();
    }

    private static void MontarCabecalho(IContainer container, string titulo, string subtitulo)
    {
        container.PaddingBottom(12).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(titulo).Bold().FontSize(15).FontColor(Colors.Blue.Darken3);
                col.Item().Text(subtitulo).FontSize(9).FontColor(Colors.Grey.Darken1);
            });
            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("ERP Multi-Empresa").Bold().FontSize(11);
                col.Item().Text($"Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private static void MontarRodape(IContainer container)
    {
        container.PaddingTop(8).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Row(row =>
        {
            row.RelativeItem().Text("ERP Multi-Empresa — Módulo de Relatórios e Inteligência Operacional").FontSize(8).FontColor(Colors.Grey.Medium);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }
}
