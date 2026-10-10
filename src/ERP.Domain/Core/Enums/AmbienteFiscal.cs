namespace ERP.Domain.Core.Enums;

/// <summary>
/// Ambiente de destino da transmissão SEFAZ (tpAmb).
/// </summary>
public enum AmbienteFiscal
{
    /// <summary>1 - Produção (com valor fiscal e jurídico real)</summary>
    Producao = 1,

    /// <summary>2 - Homologação / Testes (sem valor fiscal)</summary>
    Homologacao = 2
}
