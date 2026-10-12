using System.Globalization;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Core.Entities.Fiscal;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Unit = QuestPDF.Infrastructure.Unit;

namespace ERP.Application.Fiscal.Queries;

public record GerarDanfePdfQuery(int Id) : IRequest<byte[]>;

public class GerarDanfePdfQueryHandler : IRequestHandler<GerarDanfePdfQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GerarDanfePdfQueryHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<byte[]> Handle(GerarDanfePdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var nota = await _context.NotasFiscais.AsNoTracking()
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Nota Fiscal #{request.Id} não encontrada.");

        var empresa = await _context.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId, cancellationToken)
            ?? throw new InvalidOperationException("Empresa emitente não encontrada.");

        if (nota.Modelo == ModeloDocumentoFiscal.NFCe)
        {
            return GerarDanfeNFCeBobina(nota, empresa);
        }

        return GerarDanfeNFeA4(nota, empresa);
    }

    #region Layout NF-e Modelo 55 (A4 Retrato Oficial)

    private static byte[] GerarDanfeNFeA4(NotaFiscal nf, Domain.Core.Entities.Empresa emp)
    {
        var culture = new CultureInfo("pt-BR");
        string chaveFormatada = FormatarChaveAcesso(nf.ChaveAcesso);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(8, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    // Cabeçalho Principal (3 Colunas)
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Row(row =>
                    {
                        // 1. Dados Emitente
                        row.RelativeItem(4.5f).Padding(4).Column(c =>
                        {
                            c.Item().Text(emp.RazaoSocial).Bold().FontSize(8);
                            if (!string.IsNullOrWhiteSpace(emp.NomeFantasia) && emp.NomeFantasia != emp.RazaoSocial)
                                c.Item().Text(emp.NomeFantasia).FontSize(7);

                            c.Item().Text($"{emp.Logradouro ?? "Rua"}, {emp.Numero ?? "S/N"} - {emp.Bairro ?? ""}");
                            c.Item().Text($"{emp.Cidade ?? "São Paulo"} - {emp.Uf ?? "SP"} - CEP: {emp.Cep ?? ""}");
                            c.Item().Text($"FONE: {emp.Telefone ?? ""} | CNPJ: {emp.Cnpj}");
                            c.Item().Text($"INSCRIÇÃO ESTADUAL: {emp.InscricaoEstadual ?? "ISENTO"}");
                        });

                        // 2. Quadro Central DANFE
                        row.RelativeItem(2.5f).BorderLeft(1).BorderRight(1).BorderColor(Colors.Grey.Medium).Padding(4).Column(c =>
                        {
                            c.Item().AlignCenter().Text("DANFE").Bold().FontSize(11);
                            c.Item().AlignCenter().Text("Documento Auxiliar da Nota").FontSize(6);
                            c.Item().AlignCenter().Text("Fiscal Eletrônica").FontSize(6);

                            c.Item().PaddingTop(3).Row(r =>
                            {
                                r.RelativeItem().Text(t =>
                                {
                                    t.Span("0 - ENTRADA\n").FontSize(6);
                                    t.Span("1 - SAÍDA").FontSize(6);
                                });
                                r.ConstantItem(18).Border(1).BorderColor(Colors.Grey.Medium).AlignCenter().AlignMiddle()
                                    .Text(((int)nf.TipoOperacao).ToString()).Bold().FontSize(9);
                            });

                            c.Item().PaddingTop(3).AlignCenter().Text($"Nº {nf.Numero:D9}").Bold().FontSize(8);
                            c.Item().AlignCenter().Text($"SÉRIE: {nf.Serie:D3} | FOLHA 1/1").FontSize(6.5f);
                        });

                        // 3. Chave de Acesso e Código de Barras
                        row.RelativeItem(5f).Padding(4).Column(c =>
                        {
                            c.Item().AlignCenter().Text("CHAVE DE ACESSO").Bold().FontSize(6.5f);
                            c.Item().PaddingVertical(2).AlignCenter().Text(chaveFormatada).Bold().FontSize(7.5f).FontColor(Colors.Grey.Darken3);

                            c.Item().PaddingTop(2).AlignCenter()
                                .Text("Consulta de autenticidade no portal nacional da NF-e\nwww.nfe.fazenda.gov.br/portal ou no site da Sefaz Autorizadora")
                                .FontSize(5.5f);

                            c.Item().PaddingTop(4).BorderTop(0.5f).BorderColor(Colors.Grey.Lighten1).Row(r =>
                            {
                                r.RelativeItem().Column(pc =>
                                {
                                    pc.Item().Text("PROTOCOLO DE AUTORIZAÇÃO DE USO").Bold().FontSize(6);
                                    string proto = string.IsNullOrWhiteSpace(nf.ProtocoloAutorizacao)
                                        ? "EM HOMOLOGAÇÃO / SIMULAÇÃO"
                                        : $"{nf.ProtocoloAutorizacao} - {(nf.DataAutorizacao ?? nf.DataEmissao):dd/MM/yyyy HH:mm:ss}";
                                    pc.Item().Text(proto).FontSize(6.5f);
                                });
                            });
                        });
                    });

                    // Natureza da Operação
                    col.Item().PaddingTop(2).Border(1).BorderColor(Colors.Grey.Medium).Padding(3).Row(r =>
                    {
                        r.RelativeItem(8).Column(c =>
                        {
                            c.Item().Text("NATUREZA DA OPERAÇÃO").FontSize(5.5f).Bold();
                            c.Item().Text(nf.NaturezaOperacao).FontSize(7);
                        });
                        r.RelativeItem(4).BorderLeft(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingLeft(4).Column(c =>
                        {
                            c.Item().Text("PROTOCOLO / AMBIENTE").FontSize(5.5f).Bold();
                            c.Item().Text($"{nf.Ambiente.ToString().ToUpperInvariant()} - Status: {nf.Status}").FontSize(6.5f);
                        });
                    });

                    // DESTINATÁRIO / REMETENTE
                    col.Item().PaddingTop(2).Border(1).BorderColor(Colors.Grey.Medium).Column(c =>
                    {
                        c.Item().Background(Colors.Grey.Lighten3).Padding(2).Text("DESTINATÁRIO / REMETENTE").Bold().FontSize(6.5f);
                        c.Item().Padding(3).Column(dc =>
                        {
                            dc.Item().Row(r =>
                            {
                                r.RelativeItem(7).Text($"NOME / RAZÃO SOCIAL: {nf.DestinatarioRazaoSocial}").Bold();
                                r.RelativeItem(3).Text($"CNPJ / CPF: {nf.DestinatarioCpfCnpj}");
                                r.RelativeItem(2).Text($"DATA DA EMISSÃO: {nf.DataEmissao:dd/MM/yyyy}");
                            });
                            dc.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem(6).Text($"ENDEREÇO: {nf.DestinatarioLogradouro ?? ""}, {nf.DestinatarioNumero ?? "S/N"}");
                                r.RelativeItem(3).Text($"BAIRRO / DISTRITO: {nf.DestinatarioBairro ?? ""}");
                                r.RelativeItem(3).Text($"CEP: {nf.DestinatarioCep ?? ""}");
                            });
                            dc.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem(5).Text($"MUNICÍPIO: {nf.DestinatarioMunicipio ?? ""}");
                                r.RelativeItem(1).Text($"UF: {nf.DestinatarioUf}");
                                r.RelativeItem(3).Text($"FONE / FAX: {nf.DestinatarioTelefone ?? ""}");
                                r.RelativeItem(3).Text($"INSCRIÇÃO ESTADUAL: {nf.DestinatarioInscricaoEstadual ?? "ISENTO"}");
                            });
                        });
                    });

                    // CÁLCULO DO IMPOSTO
                    col.Item().PaddingTop(2).Border(1).BorderColor(Colors.Grey.Medium).Column(c =>
                    {
                        c.Item().Background(Colors.Grey.Lighten3).Padding(2).Text("CÁLCULO DO IMPOSTO").Bold().FontSize(6.5f);
                        c.Item().Padding(3).Row(r =>
                        {
                            r.RelativeItem().Column(sc => { sc.Item().Text("BASE DE CÁLC. DO ICMS").FontSize(5.5f); sc.Item().Text(nf.BaseCalculoIcms.ToString("C2", culture)).Bold(); });
                            r.RelativeItem().Column(sc => { sc.Item().Text("VALOR DO ICMS").FontSize(5.5f); sc.Item().Text(nf.ValorIcms.ToString("C2", culture)).Bold(); });
                            r.RelativeItem().Column(sc => { sc.Item().Text("BASE CÁLC. ICMS S.T.").FontSize(5.5f); sc.Item().Text(nf.BaseCalculoIcmsSt.ToString("C2", culture)).Bold(); });
                            r.RelativeItem().Column(sc => { sc.Item().Text("VALOR DO ICMS S.T.").FontSize(5.5f); sc.Item().Text(nf.ValorIcmsSt.ToString("C2", culture)).Bold(); });
                            r.RelativeItem().Column(sc => { sc.Item().Text("V. TOTAL PRODUTOS").FontSize(5.5f); sc.Item().Text(nf.ValorProdutos.ToString("C2", culture)).Bold(); });
                        });
                        c.Item().PaddingHorizontal(3).PaddingBottom(3).Row(r =>
                        {
                            r.RelativeItem().Column(sc => { sc.Item().Text("VALOR DO FRETE").FontSize(5.5f); sc.Item().Text(nf.ValorFrete.ToString("C2", culture)); });
                            r.RelativeItem().Column(sc => { sc.Item().Text("VALOR DO SEGURO").FontSize(5.5f); sc.Item().Text(nf.ValorSeguro.ToString("C2", culture)); });
                            r.RelativeItem().Column(sc => { sc.Item().Text("DESCONTO").FontSize(5.5f); sc.Item().Text(nf.ValorDesconto.ToString("C2", culture)); });
                            r.RelativeItem().Column(sc => { sc.Item().Text("OUTRAS DESPESAS").FontSize(5.5f); sc.Item().Text(nf.ValorOutrasDespesas.ToString("C2", culture)); });
                            r.RelativeItem().Column(sc => { sc.Item().Text("VALOR TOTAL DA NOTA").FontSize(5.5f).Bold(); sc.Item().Text(nf.ValorTotalNota.ToString("C2", culture)).Bold().FontSize(8); });
                        });
                    });
                });

                // DADOS DOS PRODUTOS / SERVIÇOS
                page.Content().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Medium).Column(col =>
                {
                    col.Item().Background(Colors.Grey.Lighten3).Padding(2).Text("DADOS DOS PRODUTOS / SERVIÇOS").Bold().FontSize(6.5f);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);   // Código
                            columns.RelativeColumn(5);    // Descrição
                            columns.ConstantColumn(42);   // NCM
                            columns.ConstantColumn(24);   // CST
                            columns.ConstantColumn(24);   // CFOP
                            columns.ConstantColumn(18);   // UN
                            columns.ConstantColumn(28);   // QTD
                            columns.ConstantColumn(40);   // V. UNIT
                            columns.ConstantColumn(45);   // V. TOTAL
                            columns.ConstantColumn(38);   // BC ICMS
                            columns.ConstantColumn(24);   // ALQ ICMS
                            columns.ConstantColumn(38);   // V. ICMS
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("CÓD.").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("DESCRIÇÃO DO PRODUTO").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("NCM/SH").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("CST").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("CFOP").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Text("UN").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignRight().Text("QTD").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignRight().Text("VL. UNIT").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignRight().Text("VL. TOTAL").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignRight().Text("BC ICMS").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignRight().Text("% ICMS").Bold().FontSize(6);
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).AlignRight().Text("VL. ICMS").Bold().FontSize(6);
                        });

                        foreach (var it in nf.Itens.OrderBy(i => i.NumeroItem))
                        {
                            string cstExibicao = !string.IsNullOrWhiteSpace(it.Csosn) ? it.Csosn : (it.CstIcms ?? "00");

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Text(it.CodigoProduto).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Text(it.DescricaoProduto).FontSize(6.5f);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Text(it.Ncm).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Text(cstExibicao).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Text(it.Cfop).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Text(it.UnidadeMedida).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).AlignRight().Text(it.Quantidade.ToString("N2", culture)).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).AlignRight().Text(it.ValorUnitario.ToString("N2", culture)).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).AlignRight().Text(it.ValorTotalLiquido.ToString("N2", culture)).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).AlignRight().Text(it.BaseCalculoIcms.ToString("N2", culture)).FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).AlignRight().Text(it.AliquotaIcms.ToString("N0", culture) + "%").FontSize(6);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).AlignRight().Text(it.ValorIcms.ToString("N2", culture)).FontSize(6);
                        }
                    });
                });

                // DADOS ADICIONAIS / INFORMAÇÕES COMPLEMENTARES
                page.Footer().Column(col =>
                {
                    col.Item().PaddingTop(2).Border(1).BorderColor(Colors.Grey.Medium).Column(c =>
                    {
                        c.Item().Background(Colors.Grey.Lighten3).Padding(2).Text("DADOS ADICIONAIS / INFORMAÇÕES COMPLEMENTARES").Bold().FontSize(6.5f);
                        c.Item().Padding(3).Text(string.IsNullOrWhiteSpace(nf.InformacoesComplementares)
                            ? "Documento emitido por ERP MultiEmpresa."
                            : nf.InformacoesComplementares).FontSize(6);
                    });

                    col.Item().PaddingTop(2).Row(r =>
                    {
                        r.RelativeItem().Text($"NF-e Nº {nf.Numero} Série {nf.Serie} | Emitida em {nf.DataEmissao:dd/MM/yyyy HH:mm:ss}").FontSize(6).FontColor(Colors.Grey.Darken1);
                        r.RelativeItem().AlignRight().Text("Sistema ERP MultiEmpresa").FontSize(6).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    #endregion

    #region Layout NFC-e Modelo 65 (Cupom / Bobina 80mm)

    private static byte[] GerarDanfeNFCeBobina(NotaFiscal nf, Domain.Core.Entities.Empresa emp)
    {
        var culture = new CultureInfo("pt-BR");
        string chaveFormatada = FormatarChaveAcesso(nf.ChaveAcesso);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(80, 200, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.Margin(4, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Courier New"));

                page.Content().Column(col =>
                {
                    // Cabeçalho Emitente
                    col.Item().AlignCenter().Text(emp.RazaoSocial).Bold().FontSize(8.5f);
                    col.Item().AlignCenter().Text($"CNPJ: {emp.Cnpj} IE: {emp.InscricaoEstadual ?? "ISENTO"}").FontSize(6.5f);
                    col.Item().AlignCenter().Text($"{emp.Logradouro ?? ""}, {emp.Numero ?? ""} - {emp.Cidade ?? ""}/{emp.Uf ?? ""}").FontSize(6);

                    col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                    col.Item().AlignCenter().Text("DANFE NFC-e - Documento Auxiliar").Bold().FontSize(7.5f);
                    col.Item().AlignCenter().Text("da Nota Fiscal de Consumidor Eletrônica").FontSize(6.5f);
                    col.Item().AlignCenter().Text("Não permite aproveitamento de crédito de ICMS").Italic().FontSize(5.5f);

                    col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                    // Lista de Itens
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(5);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2.5f);
                            cols.RelativeColumn(2.5f);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Text("DESCRIÇÃO").Bold().FontSize(6);
                            h.Cell().AlignRight().Text("QTD").Bold().FontSize(6);
                            h.Cell().AlignRight().Text("VL.UN").Bold().FontSize(6);
                            h.Cell().AlignRight().Text("TOTAL").Bold().FontSize(6);
                        });

                        foreach (var it in nf.Itens.OrderBy(i => i.NumeroItem))
                        {
                            table.Cell().Text(it.DescricaoProduto).FontSize(6);
                            table.Cell().AlignRight().Text(it.Quantidade.ToString("N0", culture)).FontSize(6);
                            table.Cell().AlignRight().Text(it.ValorUnitario.ToString("N2", culture)).FontSize(6);
                            table.Cell().AlignRight().Text(it.ValorTotalLiquido.ToString("N2", culture)).FontSize(6);
                        }
                    });

                    col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                    // Totais
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Qtd. Total de Itens:").Bold();
                        r.RelativeItem().AlignRight().Text(nf.Itens.Count.ToString());
                    });
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("VALOR TOTAL R$:").Bold().FontSize(8.5f);
                        r.RelativeItem().AlignRight().Text(nf.ValorTotalNota.ToString("N2", culture)).Bold().FontSize(8.5f);
                    });

                    col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

                    // Consumidor
                    string cons = !string.IsNullOrWhiteSpace(nf.DestinatarioCpfCnpj) && nf.DestinatarioCpfCnpj != "00000000000"
                        ? $"CONSUMIDOR: {nf.DestinatarioRazaoSocial} - CPF/CNPJ: {nf.DestinatarioCpfCnpj}"
                        : "CONSUMIDOR NÃO IDENTIFICADO";
                    col.Item().Text(cons).FontSize(6);

                    // Informações Fiscais
                    col.Item().PaddingTop(2).Text($"NFC-e Nº {nf.Numero:D9} Série {nf.Serie:D3}").Bold().FontSize(7);
                    col.Item().Text($"Emissão: {nf.DataEmissao:dd/MM/yyyy HH:mm:ss}").FontSize(6);
                    col.Item().Text($"Protocolo: {nf.ProtocoloAutorizacao ?? "EM HOMOLOGACAO"}").FontSize(6);

                    col.Item().PaddingTop(3).AlignCenter().Text("Consulte pela Chave de Acesso em:").FontSize(5.5f);
                    col.Item().AlignCenter().Text("http://www.fazenda.sp.gov.br/nfce/consulta").FontSize(5.5f);
                    col.Item().PaddingTop(2).AlignCenter().Text(chaveFormatada).Bold().FontSize(6);

                    if (nf.ValorAproximadoTributos > 0)
                    {
                        col.Item().PaddingTop(2).AlignCenter()
                            .Text($"Tributos Incidentes (Lei 12.741/2012): {nf.ValorAproximadoTributos.ToString("C2", culture)}")
                            .FontSize(5.5f);
                    }
                });
            });
        });

        return doc.GeneratePdf();
    }

    #endregion

    private static string FormatarChaveAcesso(string chave)
    {
        if (string.IsNullOrWhiteSpace(chave) || chave.Length < 44) return chave ?? "";

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < chave.Length; i++)
        {
            if (i > 0 && i % 4 == 0) sb.Append(' ');
            sb.Append(chave[i]);
        }
        return sb.ToString();
    }
}
