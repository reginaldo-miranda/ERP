using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Crm;

/// <summary>
/// Registro de atividade / follow-up (ligação, WhatsApp, e-mail, reunião, tarefa)
/// associado a um Lead e/ou Oportunidade.
/// </summary>
public class AtividadeCrm : BaseEmpresaEntity
{
    public TipoAtividadeCrm Tipo { get; set; } = TipoAtividadeCrm.Ligacao;
    public string Assunto { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public DateTime DataPrevista { get; set; } = DateTime.UtcNow;
    public DateTime? DataConclusao { get; set; }
    public bool Concluida { get; set; }

    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }

    public int? OportunidadeId { get; set; }
    public Oportunidade? Oportunidade { get; set; }

    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }
}
