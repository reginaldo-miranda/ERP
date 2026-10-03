using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Interfaces;
using ERP.Application.Financeiro.Conciliacao.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.Conciliacao.Queries;

public record GetExtratosImportadosQuery(int? ContaBancariaId = null) : IRequest<List<ExtratoImportadoDto>>;

public record GetExtratoImportadoItensQuery(int ExtratoImportadoId, StatusConciliacao? Status = null) : IRequest<List<ExtratoImportadoItemDto>>;

public record GetSugestoesConciliacaoQuery(int ExtratoImportadoId) : IRequest<List<ItemConciliacaoDto>>;

public class GetConciliacaoQueriesHandler : 
    IRequestHandler<GetExtratosImportadosQuery, List<ExtratoImportadoDto>>,
    IRequestHandler<GetExtratoImportadoItensQuery, List<ExtratoImportadoItemDto>>,
    IRequestHandler<GetSugestoesConciliacaoQuery, List<ItemConciliacaoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public GetConciliacaoQueriesHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<List<ExtratoImportadoDto>> Handle(GetExtratosImportadosQuery request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        if (empresaId == 0) return new List<ExtratoImportadoDto>();

        var query = _context.ExtratosImportados
            .AsNoTracking()
            .Include(e => e.ContaBancaria)
            .Include(e => e.Itens)
            .Where(e => e.EmpresaId == empresaId);

        if (request.ContaBancariaId.HasValue)
            query = query.Where(e => e.ContaBancariaId == request.ContaBancariaId.Value);

        var list = await query
            .OrderByDescending(e => e.DataImportacao)
            .ToListAsync(cancellationToken);

        return list.Select(e => new ExtratoImportadoDto(
            Id: e.Id,
            ContaBancariaId: e.ContaBancariaId,
            ContaBancariaDescricao: e.ContaBancaria.Descricao,
            NomeArquivo: e.NomeArquivo,
            DataImportacao: e.DataImportacao,
            DataInicioExtrato: e.DataInicioExtrato,
            DataFimExtrato: e.DataFimExtrato,
            TotalRegistros: e.TotalRegistros,
            TotalCreditos: e.TotalCreditos,
            TotalDebitos: e.TotalDebitos,
            TotalPendentes: e.Itens.Count(i => i.StatusConciliacao == StatusConciliacao.Pendente),
            TotalConciliados: e.Itens.Count(i => i.StatusConciliacao == StatusConciliacao.Conciliado),
            TotalIgnorados: e.Itens.Count(i => i.StatusConciliacao == StatusConciliacao.Ignorado)
        )).ToList();
    }

    public async Task<List<ExtratoImportadoItemDto>> Handle(GetExtratoImportadoItensQuery request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        if (empresaId == 0) return new List<ExtratoImportadoItemDto>();

        var query = _context.ExtratosImportadosItens
            .AsNoTracking()
            .Where(i => i.ExtratoImportadoId == request.ExtratoImportadoId && i.EmpresaId == empresaId);

        if (request.Status.HasValue)
            query = query.Where(i => i.StatusConciliacao == request.Status.Value);

        var list = await query
            .OrderBy(i => i.Data)
            .ToListAsync(cancellationToken);

        return list.Select(i => new ExtratoImportadoItemDto(
            Id: i.Id,
            ExtratoImportadoId: i.ExtratoImportadoId,
            TransacaoId: i.TransacaoId,
            Data: i.Data,
            Valor: i.Valor,
            Descricao: i.Descricao,
            TipoTransacao: i.TipoTransacao,
            StatusConciliacao: i.StatusConciliacao,
            MovimentacaoFinanceiraId: i.MovimentacaoFinanceiraId,
            ContaPagarId: i.ContaPagarId,
            ContaReceberId: i.ContaReceberId,
            DataConciliacao: i.DataConciliacao,
            Observacoes: i.Observacoes
        )).ToList();
    }

    public async Task<List<ItemConciliacaoDto>> Handle(GetSugestoesConciliacaoQuery request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        if (empresaId == 0) return new List<ItemConciliacaoDto>();

        var extrato = await _context.ExtratosImportados
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExtratoImportadoId && e.EmpresaId == empresaId, cancellationToken);

        if (extrato == null) return new List<ItemConciliacaoDto>();

        var itens = await _context.ExtratosImportadosItens
            .AsNoTracking()
            .Where(i => i.ExtratoImportadoId == request.ExtratoImportadoId && i.EmpresaId == empresaId)
            .OrderBy(i => i.Data)
            .ToListAsync(cancellationToken);

        // Movimentações já vinculadas a algum item para não sugerir duplicado
        var movsList = await _context.ExtratosImportadosItens
            .AsNoTracking()
            .Where(i => i.EmpresaId == empresaId && i.MovimentacaoFinanceiraId != null)
            .Select(i => i.MovimentacaoFinanceiraId!.Value)
            .ToListAsync(cancellationToken);
        var movsConciliadasIds = movsList.ToHashSet();

        // Obter movimentações candidatas na mesma conta bancária
        var dataMin = itens.Any() ? itens.Min(i => i.Data).AddDays(-10) : DateTime.UtcNow.AddDays(-30);
        var dataMax = itens.Any() ? itens.Max(i => i.Data).AddDays(10) : DateTime.UtcNow.AddDays(30);

        var movimentacoesCandidatas = await _context.MovimentacoesFinanceiras
            .AsNoTracking()
            .Where(m => m.EmpresaId == empresaId 
                     && m.ContaBancariaId == extrato.ContaBancariaId
                     && m.DataMovimentacao >= dataMin
                     && m.DataMovimentacao <= dataMax)
            .ToListAsync(cancellationToken);

        // Obter Contas a Pagar pendentes candidatas
        var contasPagarCandidatas = await _context.ContasPagar
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId 
                     && c.Status != StatusContaFinanceira.Paga 
                     && c.Status != StatusContaFinanceira.Cancelada
                     && c.DataVencimento >= dataMin
                     && c.DataVencimento <= dataMax)
            .ToListAsync(cancellationToken);

        // Obter Contas a Receber pendentes candidatas
        var contasReceberCandidatas = await _context.ContasReceber
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId 
                     && c.Status != StatusContaFinanceira.Paga 
                     && c.Status != StatusContaFinanceira.Cancelada
                     && c.DataVencimento >= dataMin
                     && c.DataVencimento <= dataMax)
            .ToListAsync(cancellationToken);

        var resultado = new List<ItemConciliacaoDto>();

        foreach (var item in itens)
        {
            SugestaoMatchDto? melhorSugestao = null;

            if (item.StatusConciliacao == StatusConciliacao.Pendente)
            {
                var valorAbs = Math.Abs(item.Valor);
                var isCredito = item.Valor > 0;
                var sugestoes = new List<SugestaoMatchDto>();

                // 1. Procurar em Movimentações Financeiras
                var tipoMovEsperado = isCredito ? TipoOperacaoFinanceira.Entrada : TipoOperacaoFinanceira.Saida;
                var movMatches = movimentacoesCandidatas
                    .Where(m => !movsConciliadasIds.Contains(m.Id)
                             && m.Tipo == tipoMovEsperado
                             && Math.Abs(m.Valor - valorAbs) < 0.01m)
                    .ToList();

                foreach (var mov in movMatches)
                {
                    var diasDiff = Math.Abs((mov.DataMovimentacao.Date - item.Data.Date).Days);
                    int score;
                    if (diasDiff == 0) score = 100;
                    else if (diasDiff <= 1) score = 95;
                    else if (diasDiff <= 3) score = 85;
                    else if (diasDiff <= 5) score = 70;
                    else score = 50;

                    sugestoes.Add(new SugestaoMatchDto(
                        MovimentacaoId: mov.Id,
                        ContaPagarId: null,
                        ContaReceberId: null,
                        Data: mov.DataMovimentacao,
                        Valor: mov.Valor,
                        Descricao: mov.Descricao,
                        TipoOrigem: "Movimentacao",
                        Score: score
                    ));
                }

                // 2. Procurar em Contas a Pagar (se for débito no extrato)
                if (!isCredito)
                {
                    var cpMatches = contasPagarCandidatas
                        .Where(cp => Math.Abs(cp.SaldoRestante - valorAbs) < 0.01m)
                        .ToList();

                    foreach (var cp in cpMatches)
                    {
                        var diasDiff = Math.Abs((cp.DataVencimento.Date - item.Data.Date).Days);
                        int score;
                        if (diasDiff == 0) score = 95;
                        else if (diasDiff <= 2) score = 85;
                        else if (diasDiff <= 5) score = 75;
                        else score = 60;

                        sugestoes.Add(new SugestaoMatchDto(
                            MovimentacaoId: null,
                            ContaPagarId: cp.Id,
                            ContaReceberId: null,
                            Data: cp.DataVencimento,
                            Valor: cp.SaldoRestante,
                            Descricao: $"Conta a Pagar: {cp.Descricao}",
                            TipoOrigem: "ContaPagar",
                            Score: score
                        ));
                    }
                }
                // 3. Procurar em Contas a Receber (se for crédito no extrato)
                else
                {
                    var crMatches = contasReceberCandidatas
                        .Where(cr => Math.Abs(cr.SaldoRestante - valorAbs) < 0.01m)
                        .ToList();

                    foreach (var cr in crMatches)
                    {
                        var diasDiff = Math.Abs((cr.DataVencimento.Date - item.Data.Date).Days);
                        int score;
                        if (diasDiff == 0) score = 95;
                        else if (diasDiff <= 2) score = 85;
                        else if (diasDiff <= 5) score = 75;
                        else score = 60;

                        sugestoes.Add(new SugestaoMatchDto(
                            MovimentacaoId: null,
                            ContaPagarId: null,
                            ContaReceberId: cr.Id,
                            Data: cr.DataVencimento,
                            Valor: cr.SaldoRestante,
                            Descricao: $"Conta a Receber: {cr.Descricao}",
                            TipoOrigem: "ContaReceber",
                            Score: score
                        ));
                    }
                }

                melhorSugestao = sugestoes.OrderByDescending(s => s.Score).FirstOrDefault();
            }

            resultado.Add(new ItemConciliacaoDto(
                Id: item.Id,
                ExtratoImportadoId: item.ExtratoImportadoId,
                TransacaoId: item.TransacaoId,
                Data: item.Data,
                Valor: item.Valor,
                Descricao: item.Descricao,
                Status: item.StatusConciliacao,
                MovimentacaoFinanceiraId: item.MovimentacaoFinanceiraId,
                DataConciliacao: item.DataConciliacao,
                MelhorSugestao: melhorSugestao
            ));
        }

        return resultado;
    }
}
