using System.Globalization;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Unit = QuestPDF.Infrastructure.Unit;

namespace ERP.Application.Compras.Queries;

public record GerarPedidoCompraPdfQuery(int Id) : IRequest<byte[]>;

public class GerarPedidoCompraPdfQueryHandler : IRequestHandler<GerarPedidoCompraPdfQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GerarPedidoCompraPdfQueryHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<byte[]> Handle(GerarPedidoCompraPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var pedido = await _context.PedidosCompra.AsNoTracking()
            .Include(p => p.Fornecedor)
            .Include(p => p.DepositoDestino)
            .Include(p => p.FormaPagamento)
            .Include(p => p.SolicitacaoCompra)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                    .ThenInclude(prod => prod.UnidadeMedida)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pedido == null)
            throw new KeyNotFoundException($"Pedido de compra {request.Id} não encontrado.");

        int empresaId = _currentEmpresaService.EmpresaId ?? pedido.EmpresaId;
        var empresa = await _context.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId, cancellationToken);

        var culture = new CultureInfo("pt-BR");
        string statusTexto = pedido.Status.ToString().ToUpperInvariant();

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28, Unit.Point);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9).FontColor(Colors.Grey.Darken3));

                // CABEÇALHO
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(3).Column(c =>
                        {
                            c.Item().Text(empresa?.NomeFantasia ?? empresa?.RazaoSocial ?? "ERP Multi-Empresa")
                                .FontSize(15).Bold().FontColor(Colors.Blue.Darken3);
                            if (empresa != null)
                            {
                                c.Item().Text($"CNPJ: {empresa.Cnpj}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                if (!string.IsNullOrWhiteSpace(empresa.Logradouro))
                                    c.Item().Text($"{empresa.Logradouro}, {empresa.Numero} - {empresa.Cidade}/{empresa.Uf}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            }
                        });

                        row.RelativeItem(2).AlignRight().Column(c =>
                        {
                            c.Item().Text("PEDIDO DE COMPRA").FontSize(14).Bold().FontColor(Colors.Blue.Darken4);
                            c.Item().Text($"Nº: {pedido.Numero}").FontSize(12).Bold();
                            c.Item().Text($"Emissão: {pedido.DataPedido:dd/MM/yyyy}").FontSize(8);
                            c.Item().Text($"Status: {statusTexto}").FontSize(8).Bold().FontColor(Colors.Blue.Medium);
                            if (pedido.PrevisaoEntrega.HasValue)
                                c.Item().Text($"Previsão Entrega: {pedido.PrevisaoEntrega.Value:dd/MM/yyyy}").FontSize(8).FontColor(Colors.Orange.Darken3);
                        });
                    });

                    col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                // CONTEÚDO
                page.Content().PaddingVertical(8).Column(col =>
                {
                    // BLOCO FORNECEDOR & ENTREGA
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("DADOS DO FORNECEDOR").Bold().FontSize(9).FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Razão Social: {pedido.Fornecedor.RazaoSocial}").FontSize(8.5f);
                            c.Item().Text($"Nome Fantasia: {pedido.Fornecedor.NomeFantasia ?? pedido.Fornecedor.RazaoSocial}").FontSize(8.5f);
                            c.Item().Text($"CNPJ: {pedido.Fornecedor.Cnpj}").FontSize(8.5f);
                            if (!string.IsNullOrWhiteSpace(pedido.Fornecedor.Email))
                                c.Item().Text($"E-mail: {pedido.Fornecedor.Email}").FontSize(8.5f);
                            if (!string.IsNullOrWhiteSpace(pedido.Fornecedor.Telefone))
                                c.Item().Text($"Telefone: {pedido.Fornecedor.Telefone}").FontSize(8.5f);
                        });

                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("LOCAL DE ENTREGA & CONDIÇÕES").Bold().FontSize(9).FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Depósito de Destino: {pedido.DepositoDestino.Nome}").FontSize(8.5f);
                            c.Item().Text($"Condição de Pagamento: {pedido.NumeroParcelas}x (Intervalo: {pedido.IntervaloDias} dias)").FontSize(8.5f);
                            if (pedido.FormaPagamento != null)
                                c.Item().Text($"Forma de Pagamento: {pedido.FormaPagamento.Nome}").FontSize(8.5f);
                            if (pedido.SolicitacaoCompra != null)
                                c.Item().Text($"Origem: Solicitação {pedido.SolicitacaoCompra.Numero}").FontSize(8.5f);
                            if (!string.IsNullOrWhiteSpace(pedido.CompradorNome))
                                c.Item().Text($"Comprador: {pedido.CompradorNome}").FontSize(8.5f);
                        });
                    });

                    col.Item().Height(10);

                    // TABELA DE ITENS
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(24); // #
                            columns.RelativeColumn(4);  // Produto
                            columns.ConstantColumn(40); // UN
                            columns.RelativeColumn(1.2f); // Qtd
                            columns.RelativeColumn(1.5f); // Preço Unit
                            columns.RelativeColumn(1.2f); // Desc
                            columns.RelativeColumn(1.6f); // Total
                        });

                        t.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("#").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Produto / Descrição").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("UN").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Qtd").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Preço Unit.").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Desc.").Bold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Subtotal").Bold();
                        });

                        int seq = 1;
                        foreach (var it in pedido.Itens)
                        {
                            var bg = seq % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                            t.Cell().Background(bg).Padding(4).Text(seq.ToString());
                            t.Cell().Background(bg).Padding(4).Column(c =>
                            {
                                c.Item().Text(it.Produto.Nome).Bold();
                                c.Item().Text($"Cód: {it.Produto.Codigo}").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            });
                            t.Cell().Background(bg).Padding(4).Text(it.Produto.UnidadeMedida?.Sigla ?? "UN");
                            t.Cell().Background(bg).Padding(4).AlignRight().Text(it.Quantidade.ToString("N2", culture));
                            t.Cell().Background(bg).Padding(4).AlignRight().Text(it.PrecoUnitario.ToString("C2", culture));
                            t.Cell().Background(bg).Padding(4).AlignRight().Text(it.ValorDesconto > 0 ? it.ValorDesconto.ToString("C2", culture) : "-");
                            t.Cell().Background(bg).Padding(4).AlignRight().Text(it.SubTotal.ToString("C2", culture)).Bold();

                            seq++;
                        }
                    });

                    col.Item().Height(10);

                    // TOTAIS
                    col.Item().AlignRight().Width(240).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total dos Produtos:").FontSize(8.5f);
                            r.RelativeItem().AlignRight().Text(pedido.ValorBruto.ToString("C2", culture));
                        });

                        if (pedido.ValorDesconto > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Desconto:").FontSize(8.5f).FontColor(Colors.Red.Medium);
                                r.RelativeItem().AlignRight().Text($"- {pedido.ValorDesconto.ToString("C2", culture)}").FontColor(Colors.Red.Medium);
                            });
                        }

                        if (pedido.ValorFrete > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Frete:").FontSize(8.5f);
                                r.RelativeItem().AlignRight().Text(pedido.ValorFrete.ToString("C2", culture));
                            });
                        }

                        if (pedido.ValorSeguro + pedido.ValorOutrasDespesas > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Outras Despesas:").FontSize(8.5f);
                                r.RelativeItem().AlignRight().Text((pedido.ValorSeguro + pedido.ValorOutrasDespesas).ToString("C2", culture));
                            });
                        }

                        c.Item().PaddingVertical(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("VALOR TOTAL:").Bold().FontSize(10.5f).FontColor(Colors.Blue.Darken3);
                            r.RelativeItem().AlignRight().Text(pedido.ValorTotal.ToString("C2", culture)).Bold().FontSize(10.5f).FontColor(Colors.Blue.Darken3);
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(pedido.Observacoes))
                    {
                        col.Item().Height(8);
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                        {
                            c.Item().Text("OBSERVAÇÕES:").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                            c.Item().Text(pedido.Observacoes).FontSize(8);
                        });
                    }
                });

                // RODAPÉ
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(3).Row(r =>
                    {
                        r.RelativeItem().Text($"Gerado em {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC — ERP Multi-Empresa").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        r.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }
}
