using System.Security.Cryptography.X509Certificates;
using ERP.Application.Fiscal.DTOs;

namespace ERP.Application.Fiscal.Common;

/// <summary>
/// Serviço de gerenciamento, criptografia AES-256 e extração de certificados digitais A1.
/// </summary>
public interface ICertificadoDigitalService
{
    /// <summary>
    /// Salva ou substitui o certificado digital A1 da empresa com criptografia.
    /// Valida integridade, senha e prazo de expiração.
    /// </summary>
    Task<CertificadoDigitalDto> SalvarCertificadoAsync(
        int empresaId,
        SalvarCertificadoDigitalDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém os dados cadastrais e validade do certificado digital ativo da empresa.
    /// </summary>
    Task<CertificadoDigitalDto?> ObterCertificadoAtivoAsync(
        int empresaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega a instância de X509Certificate2 para assinatura de XML e requisições TLS SEFAZ.
    /// </summary>
    Task<X509Certificate2?> CarregarCertificadoX509Async(
        int empresaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove o certificado digital da empresa.
    /// </summary>
    Task RemoverCertificadoAsync(
        int empresaId,
        CancellationToken cancellationToken = default);
}
