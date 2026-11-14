namespace ERP.Domain.Core.Enums;

/// <summary>
/// Status do ciclo de vida de uma Ordem de Serviço (OS).
/// </summary>
public enum StatusOrdemServico
{
    /// <summary>OS aberta, em triagem ou avaliação inicial.</summary>
    Aberta = 1,

    /// <summary>Orçamento gerado e aguardando aprovação do cliente.</summary>
    AguardandoAprovacao = 2,

    /// <summary>Orçamento aprovado pelo cliente, em execução na bancada/campo.</summary>
    Aprovada = 3,

    /// <summary>Execução pausada aguardando chegada de peças/componentes.</summary>
    AguardandoPecas = 4,

    /// <summary>Serviço concluído com sucesso, pronto para retirada/entrega ao cliente.</summary>
    Concluida = 5,

    /// <summary>OS entregue e faturada (gerou Contas a Receber e baixa de estoque definitiva).</summary>
    Faturada = 6,

    /// <summary>OS cancelada ou orçamento recusado.</summary>
    Cancelada = 7
}

