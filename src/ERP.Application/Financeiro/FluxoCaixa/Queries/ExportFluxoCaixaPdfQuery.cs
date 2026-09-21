using ERP.Application.Financeiro.FluxoCaixa.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Financeiro.FluxoCaixa.Queries;

public record ExportFluxoCaixaPdfQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? ContaBancariaId = null,
    TipoAgrupamentoFluxoCaixa Agrupamento = TipoAgrupamentoFluxoCaixa.Diario,
    bool IncluirProjetado = true
) : IRequest<byte[]>;

public class ExportFluxoCaixaPdfQueryHandler : IRequestHandler<ExportFluxoCaixaPdfQuery, byte[]>
{
    private readonly IMediator _mediator;

    public ExportFluxoCaixaPdfQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<byte[]> Handle(ExportFluxoCaixaPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var result = await _mediator.Send(new GetFluxoCaixaQuery(
            request.DataInicio,
            request.DataFim,
            request.ContaBancariaId,
            request.Agrupamento,
            request.IncluirProjetado
        ), cancellationToken);

        var culture = new CultureInfo("pt-BR");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(header => ComposeHeader(header, request, culture));
                page.Content().Element(content => ComposeContent(content, result, culture));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, ExportFluxoCaixaPdfQuery request, CultureInfo culture)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Fluxo de Caixa")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);

                column.Item().Text($"Período: {request.DataInicio.ToString("dd/MM/yyyy", culture)} a {request.DataFim.ToString("dd/MM/yyyy", culture)}")
                    .FontSize(12).FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private void ComposeContent(IContainer container, FluxoCaixaResultDto result, CultureInfo culture)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(20);

            // Resumo
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Total Entradas").SemiBold();
                    c.Item().Text(result.Resumo.TotalEntradas.ToString("C2", culture)).FontColor(Colors.Green.Darken2);
                });
                
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Total Saídas").SemiBold();
                    c.Item().Text(result.Resumo.TotalSaidas.ToString("C2", culture)).FontColor(Colors.Red.Darken2);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Saldo do Período").SemiBold();
                    var colorPeriodo = result.Resumo.SaldoPeriodo >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                    c.Item().Text(result.Resumo.SaldoPeriodo.ToString("C2", culture)).FontColor(colorPeriodo);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Saldo Projetado").SemiBold();
                    var colorProjetado = result.Resumo.SaldoProjetado >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;
                    c.Item().Text(result.Resumo.SaldoProjetado.ToString("C2", culture)).FontColor(colorProjetado);
                });
            });

            // Tabela
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(70); // Data
                    columns.RelativeColumn(2);  // Categoria
                    columns.RelativeColumn(3);  // Descrição
                    columns.RelativeColumn(1);  // Tipo
                    columns.ConstantColumn(80); // Entradas
                    columns.ConstantColumn(80); // Saídas
                    columns.ConstantColumn(90); // Saldo Acumulado
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Data");
                    header.Cell().Element(CellStyle).Text("Categoria");
                    header.Cell().Element(CellStyle).Text("Descrição");
                    header.Cell().Element(CellStyle).Text("Tipo");
                    header.Cell().Element(CellStyle).AlignRight().Text("Entradas");
                    header.Cell().Element(CellStyle).AlignRight().Text("Saídas");
                    header.Cell().Element(CellStyle).AlignRight().Text("Saldo Acumulado");

                    IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });

                foreach (var periodo in result.Periodos)
                {
                    foreach (var item in periodo.Itens)
                    {
                        table.Cell().Element(CellStyle).Text(item.Data.ToString("dd/MM/yyyy", culture));
                        table.Cell().Element(CellStyle).Text(item.Categoria ?? string.Empty);
                        table.Cell().Element(CellStyle).Text(item.Descricao ?? string.Empty);
                        table.Cell().Element(CellStyle).Text(item.Tipo ?? string.Empty);
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Entrada.ToString("N2", culture)).FontColor(Colors.Green.Darken2);
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Saida.ToString("N2", culture)).FontColor(Colors.Red.Darken2);
                        
                        var saldoColor = item.SaldoAcumulado >= 0 ? Colors.Black : Colors.Red.Darken2;
                        table.Cell().Element(CellStyle).AlignRight().Text(item.SaldoAcumulado.ToString("N2", culture)).FontColor(saldoColor);

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    }
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
            x.Span($" - Gerado em {DateTime.Now:dd/MM/yyyy HH:mm}");
        });
    }
}
