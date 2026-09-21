using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Formas e meios de pagamento da empresa (Dinheiro, Boleto, Pix, Cartão, etc.).
/// </summary>
public class FormaPagamento : BaseEmpresaEntity, IAggregateRoot
{
    public string Nome { get; set; } = string.Empty;
    public TipoFormaPagamento Tipo { get; set; } = TipoFormaPagamento.Outro;
    public int DiasCompensacao { get; set; } = 0;
    public decimal TaxaPercentual { get; set; } = 0;
    public bool Ativa { get; set; } = true;
}
