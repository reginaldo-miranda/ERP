using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Estoque;

namespace ERP.Domain.Core.Entities.Servicos;

/// <summary>
/// Peça ou material utilizado na Ordem de Serviço.
/// </summary>
public class OrdemServicoPeca : BaseEmpresaEntity
{
    public int OrdemServicoId { get; set; }
    public OrdemServico OrdemServico { get; set; } = null!;

    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    /// <summary>Custo médio vigente do produto para apuração de margem/CMV.</summary>
    public decimal CustoUnitario { get; set; }

    /// <summary>Depósito de onde saiu a peça.</summary>
    public int? DepositoId { get; set; }
    public Deposito? Deposito { get; set; }

    /// <summary>Indica se a peça já teve baixa física efetuada no estoque.</summary>
    public bool BaixadoEstoque { get; set; }

    public void CalcularTotal()
    {
        ValorTotal = Math.Max(0, (Quantidade * ValorUnitario) - ValorDesconto);
    }
}

