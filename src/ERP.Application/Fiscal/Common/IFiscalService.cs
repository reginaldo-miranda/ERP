using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Entities.Fiscal;

namespace ERP.Application.Fiscal.Common;

/// <summary>
/// Provedor de serviços fiscais para geração de XML, assinatura, transmissão e eventos SEFAZ.
/// Suporta modo Homologação/Simulação completa para desenvolvimento e emissão real via SEFAZ.
/// </summary>
public interface IFiscalService
{
    /// <summary>
    /// Gera a chave de acesso de 44 dígitos com código numérico e dígito verificador módulo 11.
    /// </summary>
    string GerarChaveAcesso(
        string ufEmitente,
        DateTime dataEmissao,
        string cnpjEmitente,
        int modelo,
        int serie,
        int numero,
        int tipoEmissao,
        int codigoNumerico);

    /// <summary>
    /// Gera o XML oficial no layout SEFAZ para a Nota Fiscal.
    /// </summary>
    Task<string> GerarXmlNotaFiscalAsync(
        NotaFiscal notaFiscal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assina digitalmente o XML da nota fiscal com o Certificado Digital A1 da empresa.
    /// </summary>
    Task<string> AssinarXmlAsync(
        int empresaId,
        string xmlConteudo,
        string tagId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processa a emissão e autorização da Nota Fiscal junto à SEFAZ (ou simulador).
    /// </summary>
    Task<EmissaoFiscalResultDto> TransmitirNotaFiscalAsync(
        NotaFiscal notaFiscal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processa o cancelamento de uma nota fiscal autorizada.
    /// </summary>
    Task<CancelamentoFiscalResultDto> CancelarNotaFiscalAsync(
        NotaFiscal notaFiscal,
        string justificativa,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processa a transmissão de uma Carta de Correção Eletrônica (CC-e).
    /// </summary>
    Task<CartaCorrecaoResultDto> TransmitirCartaCorrecaoAsync(
        NotaFiscal notaFiscal,
        CartaCorrecao cartaCorrecao,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta a disponibilidade e status dos webservices da SEFAZ para a UF da empresa.
    /// </summary>
    Task<ConsultaStatusSefazResultDto> ConsultarStatusSefazAsync(
        int empresaId,
        CancellationToken cancellationToken = default);
}
