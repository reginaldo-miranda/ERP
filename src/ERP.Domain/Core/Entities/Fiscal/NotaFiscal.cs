using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Compras;
using ERP.Domain.Core.Entities.Vendas;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Cabeçalho da Nota Fiscal Eletrônica (NF-e modelo 55) ou Nota Fiscal de Consumidor (NFC-e modelo 65).
/// </summary>
public class NotaFiscal : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Modelo de Documento Fiscal (55=NF-e, 65=NFC-e).</summary>
    public ModeloDocumentoFiscal Modelo { get; set; } = ModeloDocumentoFiscal.NFe;

    /// <summary>Série da nota fiscal.</summary>
    public int Serie { get; set; } = 1;

    /// <summary>Número sequencial da nota fiscal.</summary>
    public int Numero { get; set; }

    /// <summary>Chave de acesso de 44 dígitos numéricos.</summary>
    public string ChaveAcesso { get; set; } = string.Empty;

    /// <summary>Natureza da Operação (ex.: "VENDA DE MERCADORIAS", "REMESSA PARA CONSERTO").</summary>
    public string NaturezaOperacao { get; set; } = "VENDA DE MERCADORIAS";

    /// <summary>Tipo de Operação (0=Entrada, 1=Saída).</summary>
    public TipoOperacaoFiscal TipoOperacao { get; set; } = TipoOperacaoFiscal.Saida;

    /// <summary>Tipo de Emissão (1=Normal, 9=Contingência Offline NFC-e, etc.).</summary>
    public TipoEmissaoFiscal TipoEmissao { get; set; } = TipoEmissaoFiscal.Normal;

    /// <summary>Ambiente SEFAZ (1=Produção, 2=Homologação).</summary>
    public AmbienteFiscal Ambiente { get; set; } = AmbienteFiscal.Homologacao;

    /// <summary>Status do processamento e autorização da nota.</summary>
    public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Rascunho;

    /// <summary>Data e hora de emissão da nota fiscal.</summary>
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;

    /// <summary>Data e hora de saída ou entrada da mercadoria.</summary>
    public DateTime? DataSaidaEntrada { get; set; }

    // --- Destinatário / Tomador ---
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public string DestinatarioCpfCnpj { get; set; } = string.Empty;
    public string DestinatarioRazaoSocial { get; set; } = string.Empty;
    public string? DestinatarioInscricaoEstadual { get; set; }
    public string? DestinatarioEmail { get; set; }
    public string? DestinatarioTelefone { get; set; }

    // Endereço do Destinatário
    public string? DestinatarioLogradouro { get; set; }
    public string? DestinatarioNumero { get; set; }
    public string? DestinatarioComplemento { get; set; }
    public string? DestinatarioBairro { get; set; }
    public string? DestinatarioMunicipio { get; set; }
    public string? DestinatarioCodigoIbgeMunicipio { get; set; }
    public string DestinatarioUf { get; set; } = "SP";
    public string? DestinatarioCep { get; set; }

    // --- Vínculos operacionais do ERP ---
    public int? PedidoVendaId { get; set; }
    public PedidoVenda? PedidoVenda { get; set; }

    public int? FaturamentoId { get; set; }

    public int? EntradaMercadoriaId { get; set; }
    public EntradaMercadoria? EntradaMercadoria { get; set; }

    // --- Totais e Bases Tributárias da Nota ---
    public decimal ValorProdutos { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorSeguro { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorOutrasDespesas { get; set; }
    public decimal ValorTotalNota { get; set; }

    // ICMS / ST
    public decimal BaseCalculoIcms { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal BaseCalculoIcmsSt { get; set; }
    public decimal ValorIcmsSt { get; set; }

    // PIS / COFINS / IPI
    public decimal ValorPis { get; set; }
    public decimal ValorCofins { get; set; }
    public decimal ValorIpi { get; set; }

    // Reforma Tributária (IBS / CBS)
    public decimal ValorIbs { get; set; }
    public decimal ValorCbs { get; set; }

    // Lei 12.741/2012 (Transparência de Impostos)
    public decimal ValorAproximadoTributos { get; set; }

    // --- Retorno e Homologação SEFAZ ---
    public string? ProtocoloAutorizacao { get; set; }
    public DateTime? DataAutorizacao { get; set; }
    public int? CodigoStatusSefaz { get; set; }
    public string? MotivoStatusSefaz { get; set; }

    // --- Cancelamento ---
    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }
    public string? ProtocoloCancelamento { get; set; }

    // --- Textos Adicionais ---
    public string? InformacoesComplementares { get; set; }
    public string? ObservacoesFisco { get; set; }

    // Relacionamentos filhos
    public ICollection<NotaFiscalItem> Itens { get; set; } = new List<NotaFiscalItem>();
    public ICollection<XmlNotaFiscal> Xmls { get; set; } = new List<XmlNotaFiscal>();
    public ICollection<CartaCorrecao> CartasCorrecao { get; set; } = new List<CartaCorrecao>();
}
