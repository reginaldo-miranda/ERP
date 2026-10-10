using ERP.Domain.Core.Enums;

namespace ERP.Application.Fiscal.DTOs;

#region DTOs de Cálculo Tributário

public class CalculoItemInputDto
{
    public int? ProdutoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? Ncm { get; set; }
    public string? Cfop { get; set; }
    public string UnidadeMedida { get; set; } = "UN";
    public decimal Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
    public decimal Desconto { get; set; }
    public decimal Frete { get; set; }
    public decimal Seguro { get; set; }
    public decimal OutrasDespesas { get; set; }
    public int OrigemMercadoria { get; set; } = 0;
}

public class CalculoTributarioItemResultDto
{
    public int NumeroItem { get; set; }
    public int? ProdutoId { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public string Ncm { get; set; } = "00000000";
    public string Cfop { get; set; } = "5102";
    public string UnidadeMedida { get; set; } = "UN";

    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotalBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorSeguro { get; set; }
    public decimal ValorOutrasDespesas { get; set; }
    public decimal ValorTotalLiquido { get; set; }

    // ICMS
    public int OrigemMercadoria { get; set; }
    public string? CstIcms { get; set; }
    public string? Csosn { get; set; }
    public decimal BaseCalculoIcms { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal PercentualReducaoBaseIcms { get; set; }

    // ICMS-ST
    public decimal BaseCalculoIcmsSt { get; set; }
    public decimal AliquotaIcmsSt { get; set; }
    public decimal ValorIcmsSt { get; set; }
    public decimal PercentualMvaSt { get; set; }

    // PIS
    public string? CstPis { get; set; }
    public decimal BaseCalculoPis { get; set; }
    public decimal AliquotaPis { get; set; }
    public decimal ValorPis { get; set; }

    // COFINS
    public string? CstCofins { get; set; }
    public decimal BaseCalculoCofins { get; set; }
    public decimal AliquotaCofins { get; set; }
    public decimal ValorCofins { get; set; }

    // IPI
    public string? CstIpi { get; set; }
    public decimal BaseCalculoIpi { get; set; }
    public decimal AliquotaIpi { get; set; }
    public decimal ValorIpi { get; set; }

    // Reforma Tributária (IBS / CBS)
    public decimal AliquotaIbs { get; set; }
    public decimal ValorIbs { get; set; }
    public decimal AliquotaCbs { get; set; }
    public decimal ValorCbs { get; set; }

    // Lei 12.741/2012
    public decimal ValorAproximadoTributos { get; set; }
}

public class CalculoTributarioTotalResultDto
{
    public decimal ValorProdutos { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorSeguro { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorOutrasDespesas { get; set; }
    public decimal ValorTotalNota { get; set; }

    public decimal BaseCalculoIcms { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal BaseCalculoIcmsSt { get; set; }
    public decimal ValorIcmsSt { get; set; }

    public decimal ValorPis { get; set; }
    public decimal ValorCofins { get; set; }
    public decimal ValorIpi { get; set; }

    public decimal ValorIbs { get; set; }
    public decimal ValorCbs { get; set; }
    public decimal ValorAproximadoTributos { get; set; }

    public List<CalculoTributarioItemResultDto> Itens { get; set; } = new();
    public string? InformacoesComplementaresSugeridas { get; set; }
}

#endregion

#region DTOs de Certificado Digital

public class CertificadoDigitalDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string NomeArquivo { get; set; } = string.Empty;
    public string CnpjOuCpfTitular { get; set; } = string.Empty;
    public string RazaoSocialTitular { get; set; } = string.Empty;
    public string Thumbprint { get; set; } = string.Empty;
    public string Emissor { get; set; } = string.Empty;
    public DateTime ValidadeInicio { get; set; }
    public DateTime ValidadeFim { get; set; }
    public bool EstaExpirado { get; set; }
    public int DiasParaExpirar { get; set; }
}

public class SalvarCertificadoDigitalDto
{
    public string NomeArquivo { get; set; } = string.Empty;
    public byte[] ArquivoBytes { get; set; } = Array.Empty<byte>();
    public string Senha { get; set; } = string.Empty;
}

#endregion

#region DTOs de Regime e Regras Tributárias

public class RegimeTributarioDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public RegimeTributarioEmpresa Regime { get; set; } = RegimeTributarioEmpresa.SimplesNacional;
    public AmbienteFiscal Ambiente { get; set; } = AmbienteFiscal.Homologacao;
    public int SerieNFePadrao { get; set; } = 1;
    public int UltimoNumeroNFe { get; set; }
    public int SerieNFCePadrao { get; set; } = 1;
    public int UltimoNumeroNFCe { get; set; }
    public string? IdCscToken { get; set; }
    public string? CodigoCsc { get; set; }
    public decimal? AliquotaCreditoSimples { get; set; }
    public bool IncentivadorCultural { get; set; }
    public bool PermiteCreditoIcms { get; set; }
    public string? MensagemPadraoNotas { get; set; }
}

public class RegraTributariaDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public TipoOperacaoFiscal TipoOperacao { get; set; } = TipoOperacaoFiscal.Saida;
    public string? NcmCodigo { get; set; }
    public string CfopCodigo { get; set; } = "5102";
    public string UfOrigem { get; set; } = "**";
    public string UfDestino { get; set; } = "**";

    public string? CstIcms { get; set; }
    public string? Csosn { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal ReducaoBaseIcms { get; set; }
    public decimal AliquotaIcmsSt { get; set; }
    public decimal MvaIcmsSt { get; set; }
    public decimal ReducaoBaseIcmsSt { get; set; }

    public string? CstPis { get; set; }
    public decimal AliquotaPis { get; set; }
    public string? CstCofins { get; set; }
    public decimal AliquotaCofins { get; set; }

    public string? CstIpi { get; set; }
    public decimal AliquotaIpi { get; set; }

    public decimal AliquotaIbs { get; set; }
    public decimal AliquotaCbs { get; set; }
    public string? ObservacoesFiscais { get; set; }
    public bool Ativo { get; set; } = true;
}

public class NcmDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal AliquotaIpi { get; set; }
    public decimal AliquotaII { get; set; }
    public decimal AliquotaNacional { get; set; }
    public decimal AliquotaImportado { get; set; }
    public decimal AliquotaEstadual { get; set; }
    public decimal AliquotaMunicipal { get; set; }
}

public class CfopDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TipoOperacaoFiscal TipoOperacao { get; set; }
    public string? Aplicacao { get; set; }
    public bool GeraFinanceiro { get; set; }
    public bool MovimentaEstoque { get; set; }
}

#endregion

#region DTOs de Notas Fiscais

public class NotaFiscalDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public ModeloDocumentoFiscal Modelo { get; set; }
    public int Serie { get; set; }
    public int Numero { get; set; }
    public string ChaveAcesso { get; set; } = string.Empty;
    public string NaturezaOperacao { get; set; } = string.Empty;
    public TipoOperacaoFiscal TipoOperacao { get; set; }
    public AmbienteFiscal Ambiente { get; set; }
    public StatusNotaFiscal Status { get; set; }
    public DateTime DataEmissao { get; set; }

    public string DestinatarioCpfCnpj { get; set; } = string.Empty;
    public string DestinatarioRazaoSocial { get; set; } = string.Empty;
    public string DestinatarioUf { get; set; } = string.Empty;

    public decimal ValorProdutos { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotalNota { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal ValorPis { get; set; }
    public decimal ValorCofins { get; set; }

    public string? ProtocoloAutorizacao { get; set; }
    public DateTime? DataAutorizacao { get; set; }
    public int? CodigoStatusSefaz { get; set; }
    public string? MotivoStatusSefaz { get; set; }

    public int? PedidoVendaId { get; set; }
    public int? FaturamentoId { get; set; }
    public int? EntradaMercadoriaId { get; set; }
    public int QuantidadeItens { get; set; }
}

public class NotaFiscalDetalhesDto : NotaFiscalDto
{
    public string? DestinatarioInscricaoEstadual { get; set; }
    public string? DestinatarioEmail { get; set; }
    public string? DestinatarioTelefone { get; set; }
    public string? DestinatarioLogradouro { get; set; }
    public string? DestinatarioNumero { get; set; }
    public string? DestinatarioComplemento { get; set; }
    public string? DestinatarioBairro { get; set; }
    public string? DestinatarioMunicipio { get; set; }
    public string? DestinatarioCep { get; set; }

    public decimal BaseCalculoIcms { get; set; }
    public decimal BaseCalculoIcmsSt { get; set; }
    public decimal ValorIcmsSt { get; set; }
    public decimal ValorIpi { get; set; }
    public decimal ValorIbs { get; set; }
    public decimal ValorCbs { get; set; }
    public decimal ValorAproximadoTributos { get; set; }

    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }
    public string? ProtocoloCancelamento { get; set; }

    public string? InformacoesComplementares { get; set; }
    public string? ObservacoesFisco { get; set; }

    public List<NotaFiscalItemDto> Itens { get; set; } = new();
    public List<XmlNotaFiscalDto> Xmls { get; set; } = new();
    public List<CartaCorrecaoDto> CartasCorrecao { get; set; } = new();
}

public class NotaFiscalItemDto
{
    public int Id { get; set; }
    public int NumeroItem { get; set; }
    public int? ProdutoId { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public string Ncm { get; set; } = string.Empty;
    public string Cfop { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;

    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotalBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotalLiquido { get; set; }

    public int OrigemMercadoria { get; set; }
    public string? CstIcms { get; set; }
    public string? Csosn { get; set; }
    public decimal BaseCalculoIcms { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal ValorIcms { get; set; }

    public string? CstPis { get; set; }
    public decimal AliquotaPis { get; set; }
    public decimal ValorPis { get; set; }

    public string? CstCofins { get; set; }
    public decimal AliquotaCofins { get; set; }
    public decimal ValorCofins { get; set; }

    public decimal AliquotaIbs { get; set; }
    public decimal ValorIbs { get; set; }
    public decimal AliquotaCbs { get; set; }
    public decimal ValorCbs { get; set; }
}

public class XmlNotaFiscalDto
{
    public int Id { get; set; }
    public TipoXmlNotaFiscal TipoXml { get; set; }
    public string XmlConteudo { get; set; } = string.Empty;
    public string? ChaveAcesso { get; set; }
    public DateTime DataGeracao { get; set; }
}

public class CartaCorrecaoDto
{
    public int Id { get; set; }
    public int SequenciaEvento { get; set; }
    public string CorrecaoTexto { get; set; } = string.Empty;
    public DateTime DataEvento { get; set; }
    public string? ProtocoloHomologacao { get; set; }
    public int? CodigoStatusSefaz { get; set; }
    public string? MotivoStatusSefaz { get; set; }
}

#endregion

#region DTOs de Operações Fiscais

public class EmissaoFiscalResultDto
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int NotaFiscalId { get; set; }
    public string ChaveAcesso { get; set; } = string.Empty;
    public string? Protocolo { get; set; }
    public int CodigoStatus { get; set; }
    public string MotivoStatus { get; set; } = string.Empty;
    public string? XmlEnvio { get; set; }
    public string? XmlRetorno { get; set; }
}

public class CancelamentoFiscalResultDto
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string? Protocolo { get; set; }
    public int CodigoStatus { get; set; }
    public string MotivoStatus { get; set; } = string.Empty;
}

public class CartaCorrecaoResultDto
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int SequenciaEvento { get; set; }
    public string? Protocolo { get; set; }
    public int CodigoStatus { get; set; }
    public string MotivoStatus { get; set; } = string.Empty;
}

public class ConsultaStatusSefazResultDto
{
    public bool Online { get; set; }
    public int CodigoStatus { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public AmbienteFiscal Ambiente { get; set; }
    public long TempoRespostaMs { get; set; }
    public DateTime DataConsulta { get; set; } = DateTime.UtcNow;
}

#endregion
