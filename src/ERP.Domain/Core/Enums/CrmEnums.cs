namespace ERP.Domain.Core.Enums;

/// <summary>Status de qualificação de um Lead no CRM.</summary>
public enum StatusLead
{
    Novo = 1,
    EmContato = 2,
    Qualificado = 3,
    Convertido = 4,
    Descartado = 5
}

/// <summary>Canal de origem de um Lead.</summary>
public enum OrigemLead
{
    Indicacao = 1,
    Site = 2,
    RedesSociais = 3,
    Telefone = 4,
    Email = 5,
    Evento = 6,
    ProspeccaoAtiva = 7,
    Outro = 99
}

/// <summary>Etapas padrão do funil de vendas (pipeline) do CRM.</summary>
public enum EtapaFunil
{
    Prospeccao = 1,
    Qualificacao = 2,
    Proposta = 3,
    Negociacao = 4,
    Ganha = 5,
    Perdida = 6
}

/// <summary>Tipo de atividade / follow-up registrado no CRM.</summary>
public enum TipoAtividadeCrm
{
    Ligacao = 1,
    WhatsApp = 2,
    Email = 3,
    Reuniao = 4,
    Tarefa = 5,
    Anotacao = 6
}
