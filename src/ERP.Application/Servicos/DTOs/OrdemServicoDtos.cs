using ERP.Domain.Core.Enums;

namespace ERP.Application.Servicos.DTOs;

// ==========================================
// DTOs de Itens (Peças e Serviços)
// ==========================================

public record OrdemServicoPecaInputDto(
    int? ProdutoId,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorDesconto,
    int? DepositoId
);

public record OrdemServicoPecaDto(
    int Id,
    int? ProdutoId,
    string? ProdutoCodigo,
    string? ProdutoNome,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorDesconto,
    decimal ValorTotal,
    decimal CustoUnitario,
    int? DepositoId,
    string? DepositoNome,
    bool BaixadoEstoque
);

public record OrdemServicoServicoInputDto(
    int? ServicoId,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorDesconto,
    string? Tecnico
);

public record OrdemServicoServicoDto(
    int Id,
    int? ServicoId,
    string? ServicoCodigo,
    string? ServicoNome,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorDesconto,
    decimal ValorTotal,
    string? Tecnico
);

public record OrdemServicoHistoricoDto(
    int Id,
    StatusOrdemServico? StatusAnterior,
    StatusOrdemServico StatusNovo,
    DateTime Data,
    string? UsuarioNome,
    string? Observacao
);

// ==========================================
// DTOs de Operações da Ordem de Serviço
// ==========================================

public record CriarOrdemServicoDto(
    int ClienteId,
    PrioridadeOrdemServico Prioridade,
    DateTime DataAbertura,
    DateTime? PrevisaoConclusao,
    string? TecnicoResponsavel,
    string Equipamento,
    string? Marca,
    string? Modelo,
    string? NumeroSerie,
    string? Acessorios,
    string? CondicoesGerais,
    string DefeitoRelatado,
    string? LaudoTecnico,
    string? SolucaoTecnica,
    decimal ValorDesconto,
    int? FormaPagamentoId,
    int NumeroParcelas,
    string? Observacoes,
    string? TermoGarantia,
    List<OrdemServicoPecaInputDto>? Pecas,
    List<OrdemServicoServicoInputDto>? Servicos
);

public record AtualizarOrdemServicoDto(
    int ClienteId,
    PrioridadeOrdemServico Prioridade,
    DateTime DataAbertura,
    DateTime? PrevisaoConclusao,
    string? TecnicoResponsavel,
    string Equipamento,
    string? Marca,
    string? Modelo,
    string? NumeroSerie,
    string? Acessorios,
    string? CondicoesGerais,
    string DefeitoRelatado,
    string? LaudoTecnico,
    string? SolucaoTecnica,
    decimal ValorDesconto,
    int? FormaPagamentoId,
    int NumeroParcelas,
    string? Observacoes,
    string? TermoGarantia,
    List<OrdemServicoPecaInputDto>? Pecas,
    List<OrdemServicoServicoInputDto>? Servicos
);

public record FaturarOrdemServicoDto(
    int Id,
    int FormaPagamentoId,
    int NumeroParcelas,
    int IntervaloDias,
    int DiasPrimeiroVencimento,
    string? Observacoes
);

public record MudarStatusOrdemServicoDto(
    int Id,
    StatusOrdemServico NovoStatus,
    string? Observacao
);

// ==========================================
// DTOs de Leitura / Resumo
// ==========================================

public record OrdemServicoListItemDto(
    int Id,
    string Numero,
    int ClienteId,
    string ClienteNome,
    string? ClienteTelefone,
    string Equipamento,
    string? MarcaModelo,
    string? NumeroSerie,
    StatusOrdemServico Status,
    PrioridadeOrdemServico Prioridade,
    DateTime DataAbertura,
    DateTime? PrevisaoConclusao,
    DateTime? DataConclusao,
    string? TecnicoResponsavel,
    decimal ValorServicos,
    decimal ValorPecas,
    decimal ValorDesconto,
    decimal ValorTotal
);

public record OrdemServicoDetalhesDto(
    int Id,
    string Numero,
    int ClienteId,
    string ClienteNome,
    string? ClienteCpfCnpj,
    string? ClienteTelefone,
    string? ClienteEmail,
    string? ClienteEndereco,
    StatusOrdemServico Status,
    PrioridadeOrdemServico Prioridade,
    DateTime DataAbertura,
    DateTime? PrevisaoConclusao,
    DateTime? DataConclusao,
    DateTime? DataEntrega,
    string? TecnicoResponsavel,
    string Equipamento,
    string? Marca,
    string? Modelo,
    string? NumeroSerie,
    string? Acessorios,
    string? CondicoesGerais,
    string DefeitoRelatado,
    string? LaudoTecnico,
    string? SolucaoTecnica,
    decimal ValorServicos,
    decimal ValorPecas,
    decimal ValorDesconto,
    decimal ValorTotal,
    int? FormaPagamentoId,
    string? FormaPagamentoNome,
    int NumeroParcelas,
    Guid? IdParcelamento,
    string? Observacoes,
    string? TermoGarantia,
    DateTime? DataCancelamento,
    string? MotivoCancelamento,
    List<OrdemServicoPecaDto> Pecas,
    List<OrdemServicoServicoDto> Servicos,
    List<OrdemServicoHistoricoDto> HistoricoStatus
);

public record ResumoDashboardOrdensServicoDto(
    int TotalGeral,
    int TotalAbertas,
    int TotalAguardandoAprovacao,
    int TotalEmExecucao,
    int TotalConcluidas,
    int TotalFaturadas,
    decimal ValorFaturadoMes
);

