using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Evento de Carta de Correção Eletrônica (CC-e) vinculado a uma NF-e autorizada.
/// </summary>
public class CartaCorrecao : BaseEmpresaEntity
{
    public int NotaFiscalId { get; set; }
    public NotaFiscal NotaFiscal { get; set; } = null!;

    /// <summary>Número sequencial do evento para a mesma nota fiscal (1 a 20).</summary>
    public int SequenciaEvento { get; set; } = 1;

    /// <summary>Texto da correção pretendida (conforme legislação, mínimo 15 e máximo 1000 caracteres).</summary>
    public string CorrecaoTexto { get; set; } = string.Empty;

    /// <summary>Data e hora do registro do evento.</summary>
    public DateTime DataEvento { get; set; } = DateTime.UtcNow;

    /// <summary>Número do protocolo de homologação do evento na SEFAZ.</summary>
    public string? ProtocoloHomologacao { get; set; }

    /// <summary>Código do status de retorno da SEFAZ (ex.: 135 - Evento registrado e vinculado a NF-e).</summary>
    public int? CodigoStatusSefaz { get; set; }

    /// <summary>Mensagem descritiva de retorno da SEFAZ.</summary>
    public string? MotivoStatusSefaz { get; set; }

    /// <summary>XML do evento de CC-e enviado à SEFAZ.</summary>
    public string? XmlEnvio { get; set; }

    /// <summary>XML de resposta e protocolo da SEFAZ.</summary>
    public string? XmlRetorno { get; set; }
}
