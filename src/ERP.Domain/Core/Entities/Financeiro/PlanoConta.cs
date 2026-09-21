using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Financeiro;

/// <summary>
/// Estrutura do Plano de Contas financeiro (Receitas e Despesas) com hierarquia pai/filho.
/// </summary>
public class PlanoConta : BaseEmpresaEntity, IAggregateRoot
{
    public string Codigo { get; set; } = string.Empty; // ex: "1.1.01"
    public string Descricao { get; set; } = string.Empty;
    public TipoPlanoConta Tipo { get; set; } = TipoPlanoConta.Despesa;
    public bool Sintetica { get; set; } = false; // true = Grupo/Totalizador; false = Analítica (aceita lançamentos)
    public bool Ativo { get; set; } = true;

    // Auto-relacionamento hierárquico
    public int? PlanoContaPaiId { get; set; }
    public PlanoConta? PlanoContaPai { get; set; }
    public ICollection<PlanoConta> SubContas { get; set; } = new List<PlanoConta>();
}
