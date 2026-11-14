using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Pdv;

/// <summary>
/// Sangria ou suprimento registrado dentro de uma sessão de caixa aberta.
/// </summary>
public class MovimentoCaixaPdv : BaseEmpresaEntity
{
    public int SessaoCaixaPdvId { get; set; }
    public SessaoCaixaPdv SessaoCaixaPdv { get; set; } = null!;

    public TipoMovimentoCaixa Tipo { get; set; }
    public decimal Valor { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime DataMovimento { get; set; } = DateTime.UtcNow;
    public string? OperadorNome { get; set; }
}
