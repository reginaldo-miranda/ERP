using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Contas bancárias ou caixas físicos da empresa.
/// </summary>
public class ContaBancaria : BaseEmpresaEntity, IAggregateRoot
{
    public string Descricao { get; set; } = string.Empty;
    public TipoContaBancaria Tipo { get; set; } = TipoContaBancaria.Corrente;
    
    public int? BancoId { get; set; }
    public Banco? Banco { get; set; }

    public string? Agencia { get; set; }
    public string? AgenciaDigito { get; set; }
    public string? Conta { get; set; }
    public string? ContaDigito { get; set; }

    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public bool Ativa { get; set; } = true;
    public string? Observacoes { get; set; }

    // Navegações
    public ICollection<MovimentacaoFinanceira> Movimentacoes { get; set; } = new List<MovimentacaoFinanceira>();
}
