using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Configuração de Regime Tributário e parâmetros de faturamento fiscal por Empresa.
/// </summary>
public class RegimeTributario : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Código de Regime Tributário (CRT): Simples Nacional, Regime Normal, etc.</summary>
    public RegimeTributarioEmpresa Regime { get; set; } = RegimeTributarioEmpresa.SimplesNacional;

    /// <summary>Ambiente padrão de emissão (Homologação ou Produção).</summary>
    public AmbienteFiscal Ambiente { get; set; } = AmbienteFiscal.Homologacao;

    /// <summary>Série padrão para emissão de NF-e (Modelo 55).</summary>
    public int SerieNFePadrao { get; set; } = 1;

    /// <summary>Último número de NF-e emitido e autorizado.</summary>
    public int UltimoNumeroNFe { get; set; } = 0;

    /// <summary>Série padrão para emissão de NFC-e (Modelo 65).</summary>
    public int SerieNFCePadrao { get; set; } = 1;

    /// <summary>Último número de NFC-e emitido e autorizado.</summary>
    public int UltimoNumeroNFCe { get; set; } = 0;

    /// <summary>Identificador do CSC para emissão de NFC-e (Código de Segurança do Contribuinte, ex.: "000001").</summary>
    public string? IdCscToken { get; set; }

    /// <summary>Código alfanumérico do CSC fornecido pela SEFAZ do estado.</summary>
    public string? CodigoCsc { get; set; }

    /// <summary>Alíquota permitida para aproveitamento de crédito de ICMS pelo adquirente (Simples Nacional).</summary>
    public decimal? AliquotaCreditoSimples { get; set; }

    /// <summary>Indica se a empresa é optante ou possui incentivo cultural.</summary>
    public bool IncentivadorCultural { get; set; }

    /// <summary>Indica se permite crédito de ICMS aos destinatários.</summary>
    public bool PermiteCreditoIcms { get; set; }

    /// <summary>Mensagem padrão de rodapé incluída em Informações Complementares das notas.</summary>
    public string? MensagemPadraoNotas { get; set; }
}
