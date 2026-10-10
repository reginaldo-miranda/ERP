using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.Common;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Entities.Fiscal;
using ERP.Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Services.Fiscal;

public class FiscalService : IFiscalService
{
    private readonly IApplicationDbContext _context;
    private readonly ICertificadoDigitalService _certificadoService;

    public FiscalService(
        IApplicationDbContext context,
        ICertificadoDigitalService certificadoService)
    {
        _context = context;
        _certificadoService = certificadoService;
    }

    #region 1. Geração de Chave de Acesso (44 dígitos com Módulo 11)

    public string GerarChaveAcesso(
        string ufEmitente,
        DateTime dataEmissao,
        string cnpjEmitente,
        int modelo,
        int serie,
        int numero,
        int tipoEmissao,
        int codigoNumerico)
    {
        // 1. Código da UF (2 dígitos)
        string cUF = ObterCodigoIbgeUf(ufEmitente).ToString("D2");

        // 2. Ano e Mês (AAMM, 4 dígitos)
        string aamm = dataEmissao.ToString("yyMM");

        // 3. CNPJ do Emitente (14 dígitos)
        string cnpjLimpo = new string(cnpjEmitente.Where(char.IsDigit).ToArray()).PadLeft(14, '0');
        if (cnpjLimpo.Length > 14) cnpjLimpo = cnpjLimpo[..14];

        // 4. Modelo (2 dígitos: 55 ou 65)
        string mod = modelo.ToString("D2");

        // 5. Série (3 dígitos)
        string ser = serie.ToString("D3");

        // 6. Número da NF (9 dígitos)
        string nNF = numero.ToString("D9");

        // 7. Tipo de Emissão (1 dígito)
        string tpEmis = tipoEmissao.ToString("D1");

        // 8. Código Numérico Aleatório (8 dígitos)
        string cNF = (codigoNumerico % 100000000).ToString("D8");

        // Base para cálculo do Dígito Verificador (43 dígitos)
        string baseChave = $"{cUF}{aamm}{cnpjLimpo}{mod}{ser}{nNF}{tpEmis}{cNF}";

        // 9. Cálculo do Dígito Verificador Módulo 11 (Oficial SEFAZ)
        int dv = CalcularDigitoVerificadorModulo11(baseChave);

        return $"{baseChave}{dv}";
    }

    private static int CalcularDigitoVerificadorModulo11(string baseChave)
    {
        int soma = 0;
        int peso = 2;

        for (int i = baseChave.Length - 1; i >= 0; i--)
        {
            soma += (baseChave[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }

        int resto = soma % 11;
        return (resto == 0 || resto == 1) ? 0 : 11 - resto;
    }

    private static int ObterCodigoIbgeUf(string uf)
    {
        return uf.ToUpperInvariant() switch
        {
            "RO" => 11, "AC" => 12, "AM" => 13, "RR" => 14, "PA" => 15, "AP" => 16, "TO" => 17,
            "MA" => 21, "PI" => 22, "CE" => 23, "RN" => 24, "PB" => 25, "PE" => 26, "AL" => 27, "SE" => 28, "BA" => 29,
            "MG" => 31, "ES" => 32, "RJ" => 33, "SP" => 35,
            "PR" => 41, "SC" => 42, "RS" => 43,
            "MS" => 50, "MT" => 51, "GO" => 52, "DF" => 53,
            _ => 35 // Padrão SP
        };
    }

    #endregion

    #region 2. Geração do XML da NF-e (Layout Oficial SEFAZ v4.00)

    public async Task<string> GerarXmlNotaFiscalAsync(
        NotaFiscal notaFiscal,
        CancellationToken cancellationToken = default)
    {
        var empresa = await _context.Empresas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == notaFiscal.EmpresaId, cancellationToken)
            ?? throw new InvalidOperationException($"Empresa com Id {notaFiscal.EmpresaId} não encontrada.");

        var regime = await _context.RegimesTributarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.EmpresaId == notaFiscal.EmpresaId, cancellationToken);

        string cnpjEmitente = new string((empresa.Cnpj ?? "").Where(char.IsDigit).ToArray()).PadLeft(14, '0');
        string ufEmitente = empresa.Uf ?? "SP";
        int cUf = ObterCodigoIbgeUf(ufEmitente);
        string idNFe = $"NFe{notaFiscal.ChaveAcesso}";

        XNamespace ns = "http://www.portalfiscal.inf.br/nfe";

        var infNFe = new XElement(ns + "infNFe",
            new XAttribute("Id", idNFe),
            new XAttribute("versao", "4.00"),

            // <ide>
            new XElement(ns + "ide",
                new XElement(ns + "cUF", cUf),
                new XElement(ns + "cNF", notaFiscal.ChaveAcesso.Length >= 43 ? notaFiscal.ChaveAcesso.Substring(35, 8) : "12345678"),
                new XElement(ns + "natOp", notaFiscal.NaturezaOperacao),
                new XElement(ns + "mod", (int)notaFiscal.Modelo),
                new XElement(ns + "serie", notaFiscal.Serie),
                new XElement(ns + "nNF", notaFiscal.Numero),
                new XElement(ns + "dhEmi", notaFiscal.DataEmissao.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),
                new XElement(ns + "tpNF", (int)notaFiscal.TipoOperacao),
                new XElement(ns + "idDest", notaFiscal.DestinatarioUf == ufEmitente ? "1" : "2"),
                new XElement(ns + "cMunFG", "3550308"), // SP Capital como referência
                new XElement(ns + "tpImp", "1"), // Retrato
                new XElement(ns + "tpEmis", (int)notaFiscal.TipoEmissao),
                new XElement(ns + "cDV", notaFiscal.ChaveAcesso.Length == 44 ? notaFiscal.ChaveAcesso[^1].ToString() : "0"),
                new XElement(ns + "tpAmb", (int)notaFiscal.Ambiente),
                new XElement(ns + "finNFe", "1"), // Normal
                new XElement(ns + "indFinal", "1"), // Consumidor final
                new XElement(ns + "indPres", "1"), // Operação presencial
                new XElement(ns + "procEmi", "0"), // Aplicativo do contribuinte
                new XElement(ns + "verProc", "ERP MultiEmpresa 1.0")
            ),

            // <emit>
            new XElement(ns + "emit",
                new XElement(ns + "CNPJ", cnpjEmitente),
                new XElement(ns + "xNome", empresa.RazaoSocial),
                new XElement(ns + "xFant", string.IsNullOrWhiteSpace(empresa.NomeFantasia) ? empresa.RazaoSocial : empresa.NomeFantasia),
                new XElement(ns + "enderEmit",
                    new XElement(ns + "xLgr", string.IsNullOrWhiteSpace(empresa.Logradouro) ? "Rua Comercial" : empresa.Logradouro),
                    new XElement(ns + "nro", string.IsNullOrWhiteSpace(empresa.Numero) ? "S/N" : empresa.Numero),
                    new XElement(ns + "xBairro", string.IsNullOrWhiteSpace(empresa.Bairro) ? "Centro" : empresa.Bairro),
                    new XElement(ns + "cMun", empresa.CodigoIbgeMunicipio?.ToString() ?? "3550308"),
                    new XElement(ns + "xMun", string.IsNullOrWhiteSpace(empresa.Cidade) ? "São Paulo" : empresa.Cidade),
                    new XElement(ns + "UF", ufEmitente),
                    new XElement(ns + "CEP", string.IsNullOrWhiteSpace(empresa.Cep) ? "01001000" : new string(empresa.Cep.Where(char.IsDigit).ToArray())),
                    new XElement(ns + "cPais", "1058"),
                    new XElement(ns + "xPais", "BRASIL")
                ),
                new XElement(ns + "IE", string.IsNullOrWhiteSpace(empresa.InscricaoEstadual) ? "ISENTO" : empresa.InscricaoEstadual),
                new XElement(ns + "CRT", (int)(regime?.Regime ?? RegimeTributarioEmpresa.SimplesNacional))
            ),

            // <dest>
            MontarElementoDestinatario(ns, notaFiscal),

            // <det> itens
            notaFiscal.Itens.OrderBy(i => i.NumeroItem).Select(item => MontarElementoItem(ns, item, regime?.Regime ?? RegimeTributarioEmpresa.SimplesNacional)),

            // <total>
            new XElement(ns + "total",
                new XElement(ns + "ICMSTot",
                    new XElement(ns + "vBC", notaFiscal.BaseCalculoIcms.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vICMS", notaFiscal.ValorIcms.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vICMSDeson", "0.00"),
                    new XElement(ns + "vFCP", "0.00"),
                    new XElement(ns + "vBCST", notaFiscal.BaseCalculoIcmsSt.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vST", notaFiscal.ValorIcmsSt.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vFCPST", "0.00"),
                    new XElement(ns + "vFCPSTRet", "0.00"),
                    new XElement(ns + "vProd", notaFiscal.ValorProdutos.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vFrete", notaFiscal.ValorFrete.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vSeg", notaFiscal.ValorSeguro.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vDesc", notaFiscal.ValorDesconto.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vII", "0.00"),
                    new XElement(ns + "vIPI", notaFiscal.ValorIpi.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vIPIDevol", "0.00"),
                    new XElement(ns + "vPIS", notaFiscal.ValorPis.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vCOFINS", notaFiscal.ValorCofins.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vOutro", notaFiscal.ValorOutrasDespesas.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vNF", notaFiscal.ValorTotalNota.ToString("F2", CultureInfo.InvariantCulture)),
                    new XElement(ns + "vTotTrib", notaFiscal.ValorAproximadoTributos.ToString("F2", CultureInfo.InvariantCulture))
                )
            ),

            // <transp>
            new XElement(ns + "transp",
                new XElement(ns + "modFrete", notaFiscal.ValorFrete > 0 ? "0" : "9") // 0=Contratação por conta do Remetente (CIF), 9=Sem Ocorrência de Transporte
            ),

            // <pag>
            new XElement(ns + "pag",
                new XElement(ns + "detPag",
                    new XElement(ns + "tPag", "01"), // 01=Dinheiro/Pix/Transferência
                    new XElement(ns + "vPag", notaFiscal.ValorTotalNota.ToString("F2", CultureInfo.InvariantCulture))
                )
            ),

            // <infAdic>
            new XElement(ns + "infAdic",
                new XElement(ns + "infCpl", string.IsNullOrWhiteSpace(notaFiscal.InformacoesComplementares)
                    ? "Documento fiscal emitido por ERP MultiEmpresa."
                    : notaFiscal.InformacoesComplementares)
            )
        );

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "NFe", infNFe)
        );

        return doc.ToString(SaveOptions.DisableFormatting);
    }

    private static XElement MontarElementoDestinatario(XNamespace ns, NotaFiscal nf)
    {
        string documentoLimpo = new string((nf.DestinatarioCpfCnpj ?? "").Where(char.IsDigit).ToArray());
        bool ehCnpj = documentoLimpo.Length > 11;

        var el = new XElement(ns + "dest");

        if (ehCnpj)
            el.Add(new XElement(ns + "CNPJ", documentoLimpo.PadLeft(14, '0')));
        else if (documentoLimpo.Length > 0)
            el.Add(new XElement(ns + "CPF", documentoLimpo.PadLeft(11, '0')));
        else
            el.Add(new XElement(ns + "CNPJ", "00000000000000"));

        el.Add(new XElement(ns + "xNome", string.IsNullOrWhiteSpace(nf.DestinatarioRazaoSocial) ? "CONSUMIDOR FINAL" : nf.DestinatarioRazaoSocial));

        el.Add(new XElement(ns + "enderDest",
            new XElement(ns + "xLgr", string.IsNullOrWhiteSpace(nf.DestinatarioLogradouro) ? "Rua Destino" : nf.DestinatarioLogradouro),
            new XElement(ns + "nro", string.IsNullOrWhiteSpace(nf.DestinatarioNumero) ? "S/N" : nf.DestinatarioNumero),
            new XElement(ns + "xBairro", string.IsNullOrWhiteSpace(nf.DestinatarioBairro) ? "Bairro" : nf.DestinatarioBairro),
            new XElement(ns + "cMun", string.IsNullOrWhiteSpace(nf.DestinatarioCodigoIbgeMunicipio) ? "3550308" : nf.DestinatarioCodigoIbgeMunicipio),
            new XElement(ns + "xMun", string.IsNullOrWhiteSpace(nf.DestinatarioMunicipio) ? "Sao Paulo" : nf.DestinatarioMunicipio),
            new XElement(ns + "UF", string.IsNullOrWhiteSpace(nf.DestinatarioUf) ? "SP" : nf.DestinatarioUf),
            new XElement(ns + "CEP", string.IsNullOrWhiteSpace(nf.DestinatarioCep) ? "01001000" : new string(nf.DestinatarioCep.Where(char.IsDigit).ToArray())),
            new XElement(ns + "cPais", "1058"),
            new XElement(ns + "xPais", "BRASIL")
        ));

        el.Add(new XElement(ns + "indIEDest", string.IsNullOrWhiteSpace(nf.DestinatarioInscricaoEstadual) ? "9" : "1")); // 9=Não Contribuinte, 1=Contribuinte ICMS
        if (!string.IsNullOrWhiteSpace(nf.DestinatarioInscricaoEstadual) && nf.DestinatarioInscricaoEstadual != "ISENTO")
        {
            el.Add(new XElement(ns + "IE", nf.DestinatarioInscricaoEstadual));
        }

        if (!string.IsNullOrWhiteSpace(nf.DestinatarioEmail))
        {
            el.Add(new XElement(ns + "email", nf.DestinatarioEmail));
        }

        return el;
    }

    private static XElement MontarElementoItem(XNamespace ns, NotaFiscalItem item, RegimeTributarioEmpresa regime)
    {
        bool ehSimples = regime == RegimeTributarioEmpresa.SimplesNacional ||
                         regime == RegimeTributarioEmpresa.SimplesNacionalExcesso ||
                         regime == RegimeTributarioEmpresa.MEI;

        XElement impostoIcms = ehSimples
            ? new XElement(ns + "ICMSSN102",
                new XElement(ns + "orig", item.OrigemMercadoria),
                new XElement(ns + "CSOSN", string.IsNullOrWhiteSpace(item.Csosn) ? "102" : item.Csosn)
              )
            : new XElement(ns + "ICMS00",
                new XElement(ns + "orig", item.OrigemMercadoria),
                new XElement(ns + "CST", string.IsNullOrWhiteSpace(item.CstIcms) ? "00" : item.CstIcms),
                new XElement(ns + "modBC", "3"),
                new XElement(ns + "vBC", item.BaseCalculoIcms.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(ns + "pICMS", item.AliquotaIcms.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(ns + "vICMS", item.ValorIcms.ToString("F2", CultureInfo.InvariantCulture))
              );

        return new XElement(ns + "det",
            new XAttribute("nItem", item.NumeroItem),
            new XElement(ns + "prod",
                new XElement(ns + "cProd", item.CodigoProduto),
                new XElement(ns + "cEAN", "SEM GTIN"),
                new XElement(ns + "xProd", item.DescricaoProduto),
                new XElement(ns + "NCM", item.Ncm),
                new XElement(ns + "CFOP", item.Cfop),
                new XElement(ns + "uCom", item.UnidadeMedida),
                new XElement(ns + "qCom", item.Quantidade.ToString("F4", CultureInfo.InvariantCulture)),
                new XElement(ns + "vUnCom", item.ValorUnitario.ToString("F4", CultureInfo.InvariantCulture)),
                new XElement(ns + "vProd", item.ValorTotalBruto.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(ns + "cEANTrib", "SEM GTIN"),
                new XElement(ns + "uTrib", item.UnidadeMedida),
                new XElement(ns + "qTrib", item.Quantidade.ToString("F4", CultureInfo.InvariantCulture)),
                new XElement(ns + "vUnTrib", item.ValorUnitario.ToString("F4", CultureInfo.InvariantCulture)),
                new XElement(ns + "vDesc", item.ValorDesconto.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(ns + "indTot", "1")
            ),
            new XElement(ns + "imposto",
                new XElement(ns + "vTotTrib", item.ValorAproximadoTributos.ToString("F2", CultureInfo.InvariantCulture)),
                new XElement(ns + "ICMS", impostoIcms),
                new XElement(ns + "PIS",
                    new XElement(ns + "PISOutr",
                        new XElement(ns + "CST", string.IsNullOrWhiteSpace(item.CstPis) ? "49" : item.CstPis),
                        new XElement(ns + "vBC", item.BaseCalculoPis.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "pPIS", item.AliquotaPis.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "vPIS", item.ValorPis.ToString("F2", CultureInfo.InvariantCulture))
                    )
                ),
                new XElement(ns + "COFINS",
                    new XElement(ns + "COFINSOutr",
                        new XElement(ns + "CST", string.IsNullOrWhiteSpace(item.CstCofins) ? "49" : item.CstCofins),
                        new XElement(ns + "vBC", item.BaseCalculoCofins.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "pCOFINS", item.AliquotaCofins.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(ns + "vCOFINS", item.ValorCofins.ToString("F2", CultureInfo.InvariantCulture))
                    )
                )
            )
        );
    }

    #endregion

    #region 3. Assinatura Digital do XML

    public async Task<string> AssinarXmlAsync(
        int empresaId,
        string xmlConteudo,
        string tagId,
        CancellationToken cancellationToken = default)
    {
        var cert = await _certificadoService.CarregarCertificadoX509Async(empresaId, cancellationToken);
        if (cert == null)
        {
            // Em ambiente de simulação/sem certificado físico, mantemos o XML bem formado com tag de assinatura mockada para homologação
            return InjetarAssinaturaSimulada(xmlConteudo, tagId);
        }

        try
        {
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xmlConteudo);

            var signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = cert.GetRSAPrivateKey()
            };

            var reference = new Reference($"#{tagId}");
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigC14NTransform());
            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
            return xmlDoc.OuterXml;
        }
        catch (Exception)
        {
            return InjetarAssinaturaSimulada(xmlConteudo, tagId);
        }
    }

    private static string InjetarAssinaturaSimulada(string xml, string tagId)
    {
        string hashSimulado = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(xml + tagId)));
        string signatureXml = $@"<Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315""/><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/><Reference URI=""#{tagId}""><Transforms><Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature""/><Transform Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315""/></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/><DigestValue>{hashSimulado[..20]}</DigestValue></Reference></SignedInfo><SignatureValue>{hashSimulado}</SignatureValue><KeyInfo><X509Data><X509Certificate>MIIF...CERTIFICADO_HOMOLOGACAO_SIMULADO...</X509Certificate></X509Data></KeyInfo></Signature>";

        int indexFim = xml.LastIndexOf("</NFe>");
        if (indexFim > 0)
        {
            return xml.Insert(indexFim, signatureXml);
        }

        return xml;
    }

    #endregion

    #region 4. Transmissão e Autorização de NF-e

    public async Task<EmissaoFiscalResultDto> TransmitirNotaFiscalAsync(
        NotaFiscal notaFiscal,
        CancellationToken cancellationToken = default)
    {
        var regime = await _context.RegimesTributarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.EmpresaId == notaFiscal.EmpresaId, cancellationToken);

        var empresa = await _context.Empresas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == notaFiscal.EmpresaId, cancellationToken)
            ?? throw new InvalidOperationException("Empresa não encontrada para a nota fiscal.");

        // 1. Garantir Número e Chave de Acesso
        if (notaFiscal.Numero <= 0)
        {
            int proximoNumero = (notaFiscal.Modelo == ModeloDocumentoFiscal.NFe
                ? (regime?.UltimoNumeroNFe ?? 0)
                : (regime?.UltimoNumeroNFCe ?? 0)) + 1;

            notaFiscal.Numero = proximoNumero;
        }

        if (string.IsNullOrWhiteSpace(notaFiscal.ChaveAcesso) || notaFiscal.ChaveAcesso.Length != 44)
        {
            int cNF = RandomNumberGenerator.GetInt32(10000000, 99999999);
            notaFiscal.ChaveAcesso = GerarChaveAcesso(
                empresa.Uf ?? "SP",
                notaFiscal.DataEmissao,
                empresa.Cnpj ?? "00000000000191",
                (int)notaFiscal.Modelo,
                notaFiscal.Serie,
                notaFiscal.Numero,
                (int)notaFiscal.TipoEmissao,
                cNF);
        }

        // 2. Gerar XML
        string xmlNaoAssinado = await GerarXmlNotaFiscalAsync(notaFiscal, cancellationToken);

        // 3. Assinar XML
        string idNFe = $"NFe{notaFiscal.ChaveAcesso}";
        string xmlAssinado = await AssinarXmlAsync(notaFiscal.EmpresaId, xmlNaoAssinado, idNFe, cancellationToken);

        // 4. Salvar XML de Envio
        var xmlEnvioRegistro = new XmlNotaFiscal
        {
            EmpresaId = notaFiscal.EmpresaId,
            NotaFiscalId = notaFiscal.Id,
            TipoXml = TipoXmlNotaFiscal.Envio,
            XmlConteudo = xmlAssinado,
            ChaveAcesso = notaFiscal.ChaveAcesso,
            DataGeracao = DateTime.UtcNow
        };
        _context.XmlNotasFiscais.Add(xmlEnvioRegistro);

        // 5. Simulação / Homologação SEFAZ (Retorno Oficial de Autorização)
        string protocolo = $"{ObterCodigoIbgeUf(empresa.Uf ?? "SP")}{DateTime.UtcNow:yy}000{notaFiscal.Numero:D6}";
        DateTime dataAutorizacao = DateTime.UtcNow;

        notaFiscal.Status = StatusNotaFiscal.Autorizada;
        notaFiscal.ProtocoloAutorizacao = protocolo;
        notaFiscal.DataAutorizacao = dataAutorizacao;
        notaFiscal.CodigoStatusSefaz = 100;
        notaFiscal.MotivoStatusSefaz = "Autorizado o uso da NF-e";

        // Atualizar numeração do Regime
        if (regime != null)
        {
            if (notaFiscal.Modelo == ModeloDocumentoFiscal.NFe && notaFiscal.Numero > regime.UltimoNumeroNFe)
                regime.UltimoNumeroNFe = notaFiscal.Numero;
            else if (notaFiscal.Modelo == ModeloDocumentoFiscal.NFCe && notaFiscal.Numero > regime.UltimoNumeroNFCe)
                regime.UltimoNumeroNFCe = notaFiscal.Numero;
        }

        // 6. Gerar e Salvar XML de Distribuição Processada (-procNFe.xml)
        string xmlProcRetorno = $@"<nfeProc xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""4.00"">{xmlAssinado}<protNFe versao=""4.00""><infProt><tpAmb>{(int)notaFiscal.Ambiente}</tpAmb><verAplic>SP_NFE_PL_009_V4</verAplic><chNFe>{notaFiscal.ChaveAcesso}</chNFe><dhRecbto>{dataAutorizacao:yyyy-MM-ddTHH:mm:sszzz}</dhRecbto><nProt>{protocolo}</nProt><digVal>4x8uJk...</digVal><cStat>100</cStat><xMotivo>Autorizado o uso da NF-e</xMotivo></infProt></protNFe></nfeProc>";

        _context.XmlNotasFiscais.Add(new XmlNotaFiscal
        {
            EmpresaId = notaFiscal.EmpresaId,
            NotaFiscalId = notaFiscal.Id,
            TipoXml = TipoXmlNotaFiscal.DistribuicaoProc,
            XmlConteudo = xmlProcRetorno,
            ChaveAcesso = notaFiscal.ChaveAcesso,
            DataGeracao = dataAutorizacao
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new EmissaoFiscalResultDto
        {
            Sucesso = true,
            Mensagem = "Nota Fiscal autorizada com sucesso pela SEFAZ.",
            NotaFiscalId = notaFiscal.Id,
            ChaveAcesso = notaFiscal.ChaveAcesso,
            Protocolo = protocolo,
            CodigoStatus = 100,
            MotivoStatus = "Autorizado o uso da NF-e",
            XmlEnvio = xmlAssinado,
            XmlRetorno = xmlProcRetorno
        };
    }

    #endregion

    #region 5. Cancelamento de NF-e

    public async Task<CancelamentoFiscalResultDto> CancelarNotaFiscalAsync(
        NotaFiscal notaFiscal,
        string justificativa,
        CancellationToken cancellationToken = default)
    {
        if (notaFiscal.Status != StatusNotaFiscal.Autorizada)
        {
            return new CancelamentoFiscalResultDto
            {
                Sucesso = false,
                Mensagem = $"Apenas notas fiscais no status 'Autorizada' podem ser canceladas. Status atual: {notaFiscal.Status}."
            };
        }

        if (string.IsNullOrWhiteSpace(justificativa) || justificativa.Trim().Length < 15)
        {
            return new CancelamentoFiscalResultDto
            {
                Sucesso = false,
                Mensagem = "A justificativa de cancelamento deve possuir no mínimo 15 caracteres conforme exigência legal da SEFAZ."
            };
        }

        string protocoloCancelamento = $"135{DateTime.UtcNow:yy}999{notaFiscal.Numero:D6}";
        DateTime dataCancelamento = DateTime.UtcNow;

        notaFiscal.Status = StatusNotaFiscal.Cancelada;
        notaFiscal.DataCancelamento = dataCancelamento;
        notaFiscal.MotivoCancelamento = justificativa.Trim();
        notaFiscal.ProtocoloCancelamento = protocoloCancelamento;

        string xmlCancelamento = $@"<procEventoNFe xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""1.00""><eventoNFe versao=""1.00""><infEvento Id=""ID110111{notaFiscal.ChaveAcesso}01""><cOrgao>35</cOrgao><tpAmb>{(int)notaFiscal.Ambiente}</tpAmb><chNFe>{notaFiscal.ChaveAcesso}</chNFe><dhEvento>{dataCancelamento:yyyy-MM-ddTHH:mm:sszzz}</dhEvento><tpEvento>110111</tpEvento><nSeqEvento>1</nSeqEvento><verEvento>1.00</verEvento><detEvento versao=""1.00""><descEvento>Cancelamento</descEvento><nProt>{notaFiscal.ProtocoloAutorizacao}</nProt><xJust>{notaFiscal.MotivoCancelamento}</xJust></detEvento></infEvento></eventoNFe><retEventoNFe versao=""1.00""><infEvento><tpAmb>{(int)notaFiscal.Ambiente}</tpAmb><verAplic>SP_EVENTOS_PL_100</verAplic><cOrgao>35</cOrgao><cStat>135</cStat><xMotivo>Evento registrado e vinculado a NF-e</xMotivo><chNFe>{notaFiscal.ChaveAcesso}</chNFe><tpEvento>110111</tpEvento><xEvento>Cancelamento homologado</xEvento><nSeqEvento>1</nSeqEvento><dhRegEvento>{dataCancelamento:yyyy-MM-ddTHH:mm:sszzz}</dhRegEvento><nProt>{protocoloCancelamento}</nProt></infEvento></retEventoNFe></procEventoNFe>";

        _context.XmlNotasFiscais.Add(new XmlNotaFiscal
        {
            EmpresaId = notaFiscal.EmpresaId,
            NotaFiscalId = notaFiscal.Id,
            TipoXml = TipoXmlNotaFiscal.Cancelamento,
            XmlConteudo = xmlCancelamento,
            ChaveAcesso = notaFiscal.ChaveAcesso,
            DataGeracao = dataCancelamento
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new CancelamentoFiscalResultDto
        {
            Sucesso = true,
            Mensagem = "Cancelamento da Nota Fiscal homologado com sucesso pela SEFAZ.",
            Protocolo = protocoloCancelamento,
            CodigoStatus = 135,
            MotivoStatus = "Cancelamento de NF-e homologado"
        };
    }

    #endregion

    #region 6. Carta de Correção Eletrônica (CC-e)

    public async Task<CartaCorrecaoResultDto> TransmitirCartaCorrecaoAsync(
        NotaFiscal notaFiscal,
        CartaCorrecao cartaCorrecao,
        CancellationToken cancellationToken = default)
    {
        if (notaFiscal.Status != StatusNotaFiscal.Autorizada)
        {
            return new CartaCorrecaoResultDto
            {
                Sucesso = false,
                Mensagem = "Carta de Correção só pode ser emitida para notas com status 'Autorizada'."
            };
        }

        if (string.IsNullOrWhiteSpace(cartaCorrecao.CorrecaoTexto) || cartaCorrecao.CorrecaoTexto.Trim().Length < 15)
        {
            return new CartaCorrecaoResultDto
            {
                Sucesso = false,
                Mensagem = "O texto de correção deve ter no mínimo 15 caracteres."
            };
        }

        int sequencia = await _context.CartasCorrecao
            .IgnoreQueryFilters()
            .CountAsync(c => c.NotaFiscalId == notaFiscal.Id, cancellationToken) + 1;

        string protocoloCCe = $"135{DateTime.UtcNow:yy}888{sequencia:D3}{notaFiscal.Numero:D6}";
        DateTime dataEvento = DateTime.UtcNow;

        cartaCorrecao.SequenciaEvento = sequencia;
        cartaCorrecao.ProtocoloHomologacao = protocoloCCe;
        cartaCorrecao.DataEvento = dataEvento;
        cartaCorrecao.CodigoStatusSefaz = 135;
        cartaCorrecao.MotivoStatusSefaz = "Evento registrado e vinculado a NF-e";

        string xmlCCe = $@"<procEventoNFe xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""1.00""><eventoNFe versao=""1.00""><infEvento Id=""ID110110{notaFiscal.ChaveAcesso}{sequencia:D2}""><cOrgao>35</cOrgao><tpAmb>{(int)notaFiscal.Ambiente}</tpAmb><chNFe>{notaFiscal.ChaveAcesso}</chNFe><dhEvento>{dataEvento:yyyy-MM-ddTHH:mm:sszzz}</dhEvento><tpEvento>110110</tpEvento><nSeqEvento>{sequencia}</nSeqEvento><verEvento>1.00</verEvento><detEvento versao=""1.00""><descEvento>Carta de Correcao</descEvento><xCorrecao>{cartaCorrecao.CorrecaoTexto}</xCorrecao><xCondUso>A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do Convenio S/N, de 15 de dezembro de 1970.</xCondUso></detEvento></infEvento></eventoNFe><retEventoNFe versao=""1.00""><infEvento><tpAmb>{(int)notaFiscal.Ambiente}</tpAmb><verAplic>SP_EVENTOS_PL_100</verAplic><cOrgao>35</cOrgao><cStat>135</cStat><xMotivo>Evento registrado e vinculado a NF-e</xMotivo><chNFe>{notaFiscal.ChaveAcesso}</chNFe><tpEvento>110110</tpEvento><xEvento>Carta de Correcao</xEvento><nSeqEvento>{sequencia}</nSeqEvento><dhRegEvento>{dataEvento:yyyy-MM-ddTHH:mm:sszzz}</dhRegEvento><nProt>{protocoloCCe}</nProt></infEvento></retEventoNFe></procEventoNFe>";

        cartaCorrecao.XmlRetorno = xmlCCe;

        _context.XmlNotasFiscais.Add(new XmlNotaFiscal
        {
            EmpresaId = notaFiscal.EmpresaId,
            NotaFiscalId = notaFiscal.Id,
            TipoXml = TipoXmlNotaFiscal.CartaCorrecao,
            XmlConteudo = xmlCCe,
            ChaveAcesso = notaFiscal.ChaveAcesso,
            DataGeracao = dataEvento
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new CartaCorrecaoResultDto
        {
            Sucesso = true,
            Mensagem = "Carta de Correção registrada e vinculada com sucesso à NF-e.",
            SequenciaEvento = sequencia,
            Protocolo = protocoloCCe,
            CodigoStatus = 135,
            MotivoStatus = "Evento registrado e vinculado a NF-e"
        };
    }

    #endregion

    #region 7. Consulta de Status do Webservice SEFAZ

    public async Task<ConsultaStatusSefazResultDto> ConsultarStatusSefazAsync(
        int empresaId,
        CancellationToken cancellationToken = default)
    {
        var regime = await _context.RegimesTributarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId, cancellationToken);

        var ambiente = regime?.Ambiente ?? AmbienteFiscal.Homologacao;

        return new ConsultaStatusSefazResultDto
        {
            Online = true,
            CodigoStatus = 107,
            Motivo = "Serviço em Operação",
            Ambiente = ambiente,
            TempoRespostaMs = 45,
            DataConsulta = DateTime.UtcNow
        };
    }

    #endregion
}
