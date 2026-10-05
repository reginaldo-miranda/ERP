using System.Globalization;
using ERP.Application.Common.Interfaces;
using ERP.Application.Vendas.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Unit = QuestPDF.Infrastructure.Unit;

namespace ERP.Application.Vendas.Queries;

public record GerarOrcamentoPdfQuery(int Id) : IRequest<byte[]>;
public record GerarPedidoVendaPdfQuery(int Id) : IRequest<byte[]>;

public class GerarDocumentoPdfQueriesHandler :
    IRequestHandler<GerarOrcamentoPdfQuery, byte[]>,
    IRequestHandler<GerarPedidoVendaPdfQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GerarDocumentoPdfQueriesHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<byte[]> Handle(GerarOrcamentoPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var orcamento = await _context.Orcamentos.AsNoTracking()
            .Include(o => o.Cliente)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Servico)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (orcamento == null)
            throw new KeyNotFoundException($"Orçamento {request.Id} não encontrado.");

        int empresaId = _currentEmpresaService.EmpresaId ?? orcamento.EmpresaId;
        var empresa = await _context.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId, cancellationToken);

        var culture = new CultureInfo("pt-BR");
        var hoje = DateTime.UtcNow.Date;
        string statusTexto = orcamento.Status == StatusOrcamento.Aberto && orcamento.DataValidade.Date < hoje
            ? "EXPIRADO"
            : orcamento.Status.ToString().ToUpperInvariant();

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(empresa?.NomeFantasia ?? empresa?.RazaoSocial ?? "ERP Multi-Empresa")
                                .FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                            if (!string.IsNullOrWhiteSpace(empresa?.Cnpj))
                                c.Item().Text($"CNPJ: {empresa.Cnpj}").FontSize(8).FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("ORÇAMENTO DE VENDA")
                                .FontSize(14).Bold().FontColor(Colors.Grey.Darken4);
                            c.Item().Text($"Nº {orcamento.Numero}")
                                .FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Status: {statusTexto}")
                                .FontSize(9).Bold().FontColor(statusTexto == "EXPIRADO" ? Colors.Red.Darken2 : Colors.Grey.Darken2);
                        });
                    });

                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);

                    // Dados do Cliente e da Operação
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("DADOS DO CLIENTE").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(orcamento.Cliente.Nome).Bold().FontSize(10);
                            if (!string.IsNullOrWhiteSpace(orcamento.Cliente.CpfCnpj))
                                c.Item().Text($"CPF/CNPJ: {orcamento.Cliente.CpfCnpj}");
                            if (!string.IsNullOrWhiteSpace(orcamento.Cliente.Email))
                                c.Item().Text($"E-mail: {orcamento.Cliente.Email}");
                            if (!string.IsNullOrWhiteSpace(orcamento.Cliente.Telefone))
                                c.Item().Text($"Telefone: {orcamento.Cliente.Telefone}");
                        });

                        row.ConstantItem(10);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("INFORMAÇÕES GERAIS").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Data de Emissão: {orcamento.DataOrcamento.ToString("dd/MM/yyyy", culture)}");
                            c.Item().Text($"Validade da Proposta: {orcamento.DataValidade.ToString("dd/MM/yyyy", culture)}").Bold();
                            if (!string.IsNullOrWhiteSpace(orcamento.VendedorNome))
                                c.Item().Text($"Vendedor: {orcamento.VendedorNome}");
                        });
                    });

                    // Tabela de Itens
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(25); // Item
                            cols.ConstantColumn(50); // Tipo
                            cols.RelativeColumn(3);  // Descrição
                            cols.ConstantColumn(45); // Qtd
                            cols.ConstantColumn(70); // Preço Unit.
                            cols.ConstantColumn(60); // Desc.
                            cols.ConstantColumn(75); // Subtotal
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text("Tipo");
                            header.Cell().Element(HeaderStyle).Text("Item / Descrição");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Qtd");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Unitário");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Desc.");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Subtotal");

                            static IContainer HeaderStyle(IContainer cont) =>
                                cont.Background(Colors.Grey.Lighten3).Padding(5).DefaultTextStyle(x => x.Bold().FontSize(8));
                        });

                        int seq = 1;
                        foreach (var it in orcamento.Itens)
                        {
                            string tipo = it.TipoItem == TipoItemVenda.Produto ? "Prod" : "Serv";
                            string codigo = it.Produto?.Codigo ?? it.Servico?.Codigo ?? "";
                            string desc = string.IsNullOrWhiteSpace(codigo) ? it.Descricao : $"[{codigo}] {it.Descricao}";

                            table.Cell().Element(CellStyle).Text(seq.ToString());
                            table.Cell().Element(CellStyle).Text(tipo);
                            table.Cell().Element(CellStyle).Text(desc);
                            table.Cell().Element(CellStyle).AlignRight().Text(it.Quantidade.ToString("N2", culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(it.PrecoUnitario.ToString("C2", culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(it.ValorDesconto.ToString("C2", culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(it.SubTotal.ToString("C2", culture)).Bold();

                            seq++;
                        }

                        static IContainer CellStyle(IContainer cont) =>
                            cont.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.FontSize(8));
                    });

                    // Totais
                    col.Item().AlignRight().Width(240).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Spacing(3);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Bruto:");
                            r.RelativeItem().AlignRight().Text(orcamento.ValorBruto.ToString("C2", culture));
                        });
                        if (orcamento.ValorDesconto > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Desconto Geral:").FontColor(Colors.Red.Darken2);
                                r.RelativeItem().AlignRight().Text($"- {orcamento.ValorDesconto.ToString("C2", culture)}").FontColor(Colors.Red.Darken2);
                            });
                        }
                        c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("VALOR TOTAL:").Bold().FontSize(10);
                            r.RelativeItem().AlignRight().Text(orcamento.ValorTotal.ToString("C2", culture)).Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                        });
                    });

                    // Observações
                    if (!string.IsNullOrWhiteSpace(orcamento.Observacao))
                    {
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("OBSERVAÇÕES E CONDIÇÕES").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(orcamento.Observacao).FontSize(8);
                        });
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text($"Emitido em {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC").FontSize(7).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
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

    public async Task<byte[]> Handle(GerarPedidoVendaPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var pedido = await _context.PedidosVenda.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Deposito)
            .Include(p => p.FormaPagamento)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Servico)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pedido == null)
            throw new KeyNotFoundException($"Pedido de venda {request.Id} não encontrado.");

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
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(empresa?.NomeFantasia ?? empresa?.RazaoSocial ?? "ERP Multi-Empresa")
                                .FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                            if (!string.IsNullOrWhiteSpace(empresa?.Cnpj))
                                c.Item().Text($"CNPJ: {empresa.Cnpj}").FontSize(8).FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("PEDIDO DE VENDA")
                                .FontSize(14).Bold().FontColor(Colors.Grey.Darken4);
                            c.Item().Text($"Nº {pedido.Numero}")
                                .FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text($"Status: {statusTexto}")
                                .FontSize(9).Bold().FontColor(Colors.Green.Darken2);
                        });
                    });

                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);

                    // Dados do Cliente e Operação
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("DADOS DO CLIENTE").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(pedido.Cliente.Nome).Bold().FontSize(10);
                            if (!string.IsNullOrWhiteSpace(pedido.Cliente.CpfCnpj))
                                c.Item().Text($"CPF/CNPJ: {pedido.Cliente.CpfCnpj}");
                            if (!string.IsNullOrWhiteSpace(pedido.Cliente.Email))
                                c.Item().Text($"E-mail: {pedido.Cliente.Email}");
                            if (!string.IsNullOrWhiteSpace(pedido.Cliente.Telefone))
                                c.Item().Text($"Telefone: {pedido.Cliente.Telefone}");
                        });

                        row.ConstantItem(10);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("INFORMAÇÕES DE FATURAMENTO").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text($"Data do Pedido: {pedido.DataPedido.ToString("dd/MM/yyyy", culture)}");
                            if (pedido.FormaPagamento != null)
                                c.Item().Text($"Forma de Pagto: {pedido.FormaPagamento.Nome}");
                            c.Item().Text($"Condição: {pedido.NumeroParcelas}x (intervalo: {pedido.IntervaloDias} dias)");
                            if (pedido.Deposito != null)
                                c.Item().Text($"Depósito de Saída: {pedido.Deposito.Nome}");
                            if (!string.IsNullOrWhiteSpace(pedido.VendedorNome))
                                c.Item().Text($"Vendedor: {pedido.VendedorNome}");
                        });
                    });

                    // Itens do Pedido
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(25); // Item
                            cols.ConstantColumn(50); // Tipo
                            cols.RelativeColumn(3);  // Descrição
                            cols.ConstantColumn(45); // Qtd
                            cols.ConstantColumn(70); // Preço Unit.
                            cols.ConstantColumn(60); // Desc.
                            cols.ConstantColumn(75); // Subtotal
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("#");
                            header.Cell().Element(HeaderStyle).Text("Tipo");
                            header.Cell().Element(HeaderStyle).Text("Item / Descrição");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Qtd");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Unitário");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Desc.");
                            header.Cell().Element(HeaderStyle).AlignRight().Text("Subtotal");

                            static IContainer HeaderStyle(IContainer cont) =>
                                cont.Background(Colors.Grey.Lighten3).Padding(5).DefaultTextStyle(x => x.Bold().FontSize(8));
                        });

                        int seq = 1;
                        foreach (var it in pedido.Itens)
                        {
                            string tipo = it.TipoItem == TipoItemVenda.Produto ? "Prod" : "Serv";
                            string codigo = it.Produto?.Codigo ?? it.Servico?.Codigo ?? "";
                            string desc = string.IsNullOrWhiteSpace(codigo) ? it.Descricao : $"[{codigo}] {it.Descricao}";

                            table.Cell().Element(CellStyle).Text(seq.ToString());
                            table.Cell().Element(CellStyle).Text(tipo);
                            table.Cell().Element(CellStyle).Text(desc);
                            table.Cell().Element(CellStyle).AlignRight().Text(it.Quantidade.ToString("N2", culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(it.PrecoUnitario.ToString("C2", culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(it.ValorDesconto.ToString("C2", culture));
                            table.Cell().Element(CellStyle).AlignRight().Text(it.SubTotal.ToString("C2", culture)).Bold();

                            seq++;
                        }

                        static IContainer CellStyle(IContainer cont) =>
                            cont.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.FontSize(8));
                    });

                    // Totais
                    col.Item().AlignRight().Width(240).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                    {
                        c.Spacing(3);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Bruto:");
                            r.RelativeItem().AlignRight().Text(pedido.ValorBruto.ToString("C2", culture));
                        });
                        if (pedido.ValorDesconto > 0)
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Desconto Geral:").FontColor(Colors.Red.Darken2);
                                r.RelativeItem().AlignRight().Text($"- {pedido.ValorDesconto.ToString("C2", culture)}").FontColor(Colors.Red.Darken2);
                            });
                        }
                        c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("VALOR TOTAL:").Bold().FontSize(10);
                            r.RelativeItem().AlignRight().Text(pedido.ValorTotal.ToString("C2", culture)).Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                        });
                    });

                    // Observações
                    if (!string.IsNullOrWhiteSpace(pedido.Observacao))
                    {
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("OBSERVAÇÕES DO PEDIDO").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(pedido.Observacao).FontSize(8);
                        });
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text($"Emitido em {DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm")} UTC").FontSize(7).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
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
