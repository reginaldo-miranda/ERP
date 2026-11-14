using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Crm;

/// <summary>
/// Prospect / Lead comercial que pode ser qualificado e convertido em Cliente.
/// </summary>
public class Lead : BaseEmpresaEntity, IAggregateRoot
{
    public string Nome { get; set; } = string.Empty;
    public string? EmpresaContato { get; set; }
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Fisica;
    public string? CpfCnpj { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Cargo { get; set; }

    public OrigemLead Origem { get; set; } = OrigemLead.Indicacao;
    public StatusLead Status { get; set; } = StatusLead.Novo;
    public string? Observacoes { get; set; }

    /// <summary>Id do usuário (Identity) responsável pelo atendimento do Lead.</summary>
    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }

    /// <summary>Preenchido quando o Lead é convertido em Cliente.</summary>
    public int? ClienteConvertidoId { get; set; }
    public Cliente? ClienteConvertido { get; set; }
    public DateTime? DataConversao { get; set; }

    public ICollection<Oportunidade> Oportunidades { get; set; } = new List<Oportunidade>();
    public ICollection<AtividadeCrm> Atividades { get; set; } = new List<AtividadeCrm>();
}
