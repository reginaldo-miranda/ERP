using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Enums;

namespace ERP.Application.Fiscal.Common;

/// <summary>
/// Motor de resolução e cálculo de tributos (ICMS, ICMS-ST, PIS, COFINS, IPI, IBS e CBS).
/// </summary>
public interface ICalculoTributarioService
{
    /// <summary>
    /// Calcula os tributos de um conjunto de itens para uma determinada operação comercial.
    /// Aplica a regra hierárquica por NCM, CFOP, UF Destino e regime tributário da empresa.
    /// </summary>
    Task<CalculoTributarioTotalResultDto> CalcularTributosAsync(
        int empresaId,
        TipoOperacaoFiscal tipoOperacao,
        string ufDestino,
        IEnumerable<CalculoItemInputDto> itens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula os tributos de um item individual aplicando as regras fiscais vigentes.
    /// </summary>
    Task<CalculoTributarioItemResultDto> CalcularItemAsync(
        int empresaId,
        TipoOperacaoFiscal tipoOperacao,
        string ufDestino,
        int numeroItem,
        CalculoItemInputDto item,
        CancellationToken cancellationToken = default);
}
