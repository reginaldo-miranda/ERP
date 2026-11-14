using ERP.Domain.Core.Enums;

namespace ERP.Application.Crm.DTOs;

public class LeadDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? EmpresaContato { get; set; }
    public TipoPessoa TipoPessoa { get; set; }
    public string? CpfCnpj { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Cargo { get; set; }
    public OrigemLead Origem { get; set; }
    public StatusLead Status { get; set; }
    public string? Observacoes { get; set; }
    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }
    public int? ClienteConvertidoId { get; set; }
    public string? ClienteConvertidoNome { get; set; }
    public DateTime? DataConversao { get; set; }
    public DateTime CriadoEm { get; set; }
    public int TotalOportunidades { get; set; }
    public int AtividadesPendentes { get; set; }
}

public class CriarLeadDto
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
    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }
}

public class AtualizarLeadDto : CriarLeadDto
{
}

public class OportunidadeDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? LeadId { get; set; }
    public string? LeadNome { get; set; }
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public EtapaFunil Etapa { get; set; }
    public decimal ValorEstimado { get; set; }
    public int Probabilidade { get; set; }
    public DateTime? DataFechamentoPrevista { get; set; }
    public DateTime? DataFechamentoReal { get; set; }
    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }
    public string? MotivoPerda { get; set; }
    public int? OrcamentoId { get; set; }
    public string? OrcamentoNumero { get; set; }
    public int? PedidoVendaId { get; set; }
    public string? PedidoVendaNumero { get; set; }
    public DateTime CriadoEm { get; set; }
    public int AtividadesPendentes { get; set; }
    public bool TemAtividadeAtrasada { get; set; }
}

public class CriarOportunidadeDto
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? LeadId { get; set; }
    public int? ClienteId { get; set; }
    public EtapaFunil Etapa { get; set; } = EtapaFunil.Prospeccao;
    public decimal ValorEstimado { get; set; }
    public int Probabilidade { get; set; } = 20;
    public DateTime? DataFechamentoPrevista { get; set; }
    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }
}

public class AtualizarOportunidadeDto : CriarOportunidadeDto
{
    public string? MotivoPerda { get; set; }
}

public class AtividadeCrmDto
{
    public int Id { get; set; }
    public TipoAtividadeCrm Tipo { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataPrevista { get; set; }
    public DateTime? DataConclusao { get; set; }
    public bool Concluida { get; set; }
    public bool EstaAtrasada => !Concluida && DataPrevista.Date < DateTime.UtcNow.Date;
    public int? LeadId { get; set; }
    public string? LeadNome { get; set; }
    public int? OportunidadeId { get; set; }
    public string? OportunidadeTitulo { get; set; }
    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }
    public DateTime CriadoEm { get; set; }
}

public class CriarAtividadeCrmDto
{
    public TipoAtividadeCrm Tipo { get; set; } = TipoAtividadeCrm.Ligacao;
    public string Assunto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataPrevista { get; set; } = DateTime.Today;
    public int? LeadId { get; set; }
    public int? OportunidadeId { get; set; }
    public string? ResponsavelId { get; set; }
    public string? ResponsavelNome { get; set; }
}

public class CrmResumoKpiDto
{
    public int TotalLeads { get; set; }
    public int LeadsNovosOuEmContato { get; set; }
    public int OportunidadesAbertas { get; set; }
    public decimal ValorPipeline { get; set; }
    public decimal ValorGanhoMes { get; set; }
    public decimal TaxaConversaoPercent { get; set; }
    public int AtividadesAtrasadas { get; set; }
    public int AtividadesHoje { get; set; }
}
