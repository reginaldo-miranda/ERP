using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;

namespace ERP.Domain.Core.Entities.Servicos;

/// <summary>
/// Mão de obra ou serviço executado na Ordem de Serviço.
/// </summary>
public class OrdemServicoServico : BaseEmpresaEntity
{
    public int OrdemServicoId { get; set; }
    public OrdemServico OrdemServico { get; set; } = null!;

    public int? ServicoId { get; set; }
    public Servico? Servico { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    public string? Tecnico { get; set; }

    public void CalcularTotal()
    {
        ValorTotal = Math.Max(0, (Quantidade * ValorUnitario) - ValorDesconto);
    }
}

