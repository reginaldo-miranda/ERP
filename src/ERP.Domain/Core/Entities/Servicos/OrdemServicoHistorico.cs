using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Servicos;

/// <summary>
/// Histórico de mudanças de status e observações de evolução da Ordem de Serviço.
/// </summary>
public class OrdemServicoHistorico : BaseEmpresaEntity
{
    public int OrdemServicoId { get; set; }
    public OrdemServico OrdemServico { get; set; } = null!;

    public StatusOrdemServico? StatusAnterior { get; set; }
    public StatusOrdemServico StatusNovo { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
    public string? UsuarioNome { get; set; }
    public string? Observacao { get; set; }
}

