using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Certificado Digital A1 (.pfx/.p12) utilizado para assinatura de XML e comunicação segura com a SEFAZ.
/// O arquivo binário e a senha são armazenados de forma criptografada (AES-256).
/// </summary>
public class CertificadoDigital : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Nome original do arquivo enviado pelo usuário.</summary>
    public string NomeArquivo { get; set; } = string.Empty;

    /// <summary>Conteúdo em bytes do arquivo PFX criptografado.</summary>
    public byte[] ArquivoBytes { get; set; } = Array.Empty<byte>();

    /// <summary>Senha do certificado protegida com criptografia simétrica AES-256.</summary>
    public string SenhaCriptografada { get; set; } = string.Empty;

    /// <summary>Data de início da validade do certificado digital.</summary>
    public DateTime ValidadeInicio { get; set; }

    /// <summary>Data de expiração do certificado digital.</summary>
    public DateTime ValidadeFim { get; set; }

    /// <summary>CNPJ ou CPF do titular extraído do certificado.</summary>
    public string CnpjOuCpfTitular { get; set; } = string.Empty;

    /// <summary>Razão social / Nome do titular extraído do Subject do certificado.</summary>
    public string RazaoSocialTitular { get; set; } = string.Empty;

    /// <summary>Impressão digital SHA-1 (Thumbprint) única do certificado.</summary>
    public string Thumbprint { get; set; } = string.Empty;

    /// <summary>Autoridade Certificadora emissora (ex.: Certisign, Serasa, Soluti).</summary>
    public string Emissor { get; set; } = string.Empty;

    /// <summary>Indica se o certificado está atualmente expirado.</summary>
    public bool EstaExpirado => DateTime.UtcNow > ValidadeFim;

    /// <summary>Dias restantes até a expiração do certificado.</summary>
    public int DiasParaExpirar => (ValidadeFim.Date - DateTime.UtcNow.Date).Days;
}
