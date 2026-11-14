using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Pdv;

/// <summary>
/// Terminal físico/lógico de PDV (ex.: Caixa 01, Caixa 02, Balcão Pré-Venda 01).
/// </summary>
public class TerminalPdv : BaseEmpresaEntity, IAggregateRoot
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoTerminalPdv Tipo { get; set; } = TipoTerminalPdv.Hibrido;

    public int? DepositoId { get; set; }
    public Deposito? Deposito { get; set; }

    public int? ContaBancariaId { get; set; }
    public ContaBancaria? ContaBancaria { get; set; }

    public string? ImpressoraCupom { get; set; }

    public ICollection<SessaoCaixaPdv> Sessoes { get; set; } = new List<SessaoCaixaPdv>();
}
