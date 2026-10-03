using System;
using System.Collections.Generic;
using ERP.Domain.Core.Enums;

namespace ERP.Application.Financeiro.Conciliacao.DTOs;

public record ExtratoImportadoDto(
    int Id,
    int ContaBancariaId,
    string ContaBancariaDescricao,
    string NomeArquivo,
    DateTime DataImportacao,
    DateTime DataInicioExtrato,
    DateTime DataFimExtrato,
    int TotalRegistros,
    decimal TotalCreditos,
    decimal TotalDebitos,
    int TotalPendentes,
    int TotalConciliados,
    int TotalIgnorados
);

public record ExtratoImportadoItemDto(
    int Id,
    int ExtratoImportadoId,
    string? TransacaoId,
    DateTime Data,
    decimal Valor,
    string Descricao,
    string? TipoTransacao,
    StatusConciliacao StatusConciliacao,
    int? MovimentacaoFinanceiraId,
    int? ContaPagarId,
    int? ContaReceberId,
    DateTime? DataConciliacao,
    string? Observacoes
);

public record ItemConciliacaoDto(
    int Id,
    int ExtratoImportadoId,
    string? TransacaoId,
    DateTime Data,
    decimal Valor,
    string Descricao,
    StatusConciliacao Status,
    int? MovimentacaoFinanceiraId,
    DateTime? DataConciliacao,
    SugestaoMatchDto? MelhorSugestao
);

public record SugestaoMatchDto(
    int? MovimentacaoId,
    int? ContaPagarId,
    int? ContaReceberId,
    DateTime Data,
    decimal Valor,
    string Descricao,
    string TipoOrigem, // "Movimentacao", "ContaPagar", "ContaReceber"
    int Score
);

public record ImportacaoOfxResultDto(
    int ExtratoImportadoId,
    int TotalTransacoes,
    int TransacoesNovas,
    int TransacoesDuplicadas,
    decimal TotalCreditos,
    decimal TotalDebitos,
    DateTime DataInicio,
    DateTime DataFim
);
