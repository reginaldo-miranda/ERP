namespace ERP.Domain.Core.Enums;

/// <summary>
/// Tipo de emissão da NF-e / NFC-e (tpEmis).
/// </summary>
public enum TipoEmissaoFiscal
{
    /// <summary>1 - Emissão normal</summary>
    Normal = 1,

    /// <summary>2 - Contingência FS-IA</summary>
    ContingenciaFSIA = 2,

    /// <summary>3 - Contingência SCAN (descontinuado)</summary>
    ContingenciaSCAN = 3,

    /// <summary>4 - Contingência DPEC</summary>
    ContingenciaDPEC = 4,

    /// <summary>5 - Contingência FS-DA</summary>
    ContingenciaFSDA = 5,

    /// <summary>6 - Contingência SVC-AN</summary>
    ContingenciaSVCAN = 6,

    /// <summary>7 - Contingência SVC-RS</summary>
    ContingenciaSVCRS = 7,

    /// <summary>9 - Contingência off-line da NFC-e</summary>
    ContingenciaOfflineNFCe = 9
}
