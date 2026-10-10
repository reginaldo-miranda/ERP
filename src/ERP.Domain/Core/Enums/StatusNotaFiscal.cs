namespace ERP.Domain.Core.Enums;

/// <summary>
/// Ciclo de vida e status de processamento da Nota Fiscal junto à SEFAZ.
/// </summary>
public enum StatusNotaFiscal
{
    /// <summary>Criada internamente no ERP, aguardando validação ou envio.</summary>
    Rascunho = 0,

    /// <summary>XML gerado e assinado digitalmente com certificado A1.</summary>
    Assinada = 1,

    /// <summary>Transmitida ao webservice da SEFAZ, aguardando lote/recibo.</summary>
    Transmitida = 2,

    /// <summary>Autorizado o uso pela SEFAZ (status 100).</summary>
    Autorizada = 3,

    /// <summary>Rejeitada pela SEFAZ com código de erro específico.</summary>
    Rejeitada = 4,

    /// <summary>Cancelada formalmente junto à SEFAZ (status 101/135).</summary>
    Cancelada = 5,

    /// <summary>Uso denegado pela SEFAZ por irregularidade fiscal (status 110/301/302).</summary>
    Denegada = 6
}
