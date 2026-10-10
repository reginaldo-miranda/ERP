namespace ERP.Domain.Core.Enums;

/// <summary>
/// Código de Regime Tributário da Empresa (CRT conforme layout SEFAZ).
/// </summary>
public enum RegimeTributarioEmpresa
{
    /// <summary>1 - Simples Nacional</summary>
    SimplesNacional = 1,

    /// <summary>2 - Simples Nacional - excesso de sublimite da receita bruta</summary>
    SimplesNacionalExcesso = 2,

    /// <summary>3 - Regime Normal (Lucro Presumido ou Lucro Real)</summary>
    RegimeNormal = 3,

    /// <summary>4 - Microempreendedor Individual (MEI)</summary>
    MEI = 4
}
