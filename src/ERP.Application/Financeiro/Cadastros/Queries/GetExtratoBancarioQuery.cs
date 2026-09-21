using ERP.Application.Common.Interfaces;
using ERP.Application.Financeiro.Cadastros.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.Cadastros.Queries;

/// <summary>
/// Retorna o extrato bancário detalhado de uma conta para o período informado,
/// incluindo saldo inicial, movimentações com saldo acumulado e saldo final.
/// </summary>
public record GetExtratoBancarioQuery(
    int ContaBancariaId,
    DateTime DataInicio,
    DateTime DataFim,
    TipoOperacaoFinanceira? Tipo = null,
    int? PlanoContaId = null,
    int? CentroCustoId = null
) : IRequest<ExtratoBancarioDto>;

public class GetExtratoBancarioQueryHandler : IRequestHandler<GetExtratoBancarioQuery, ExtratoBancarioDto>
{
    private readonly IApplicationDbContext _context;

    public GetExtratoBancarioQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExtratoBancarioDto> Handle(GetExtratoBancarioQuery request, CancellationToken cancellationToken)
    {
        // Busca a conta bancária para obter o saldo inicial cadastrado
        var conta = await _context.ContasBancarias
            .AsNoTracking()
            .Where(c => c.Id == request.ContaBancariaId)
            .Select(c => new { c.Id, c.Descricao, c.SaldoInicial })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Conta bancária {request.ContaBancariaId} não encontrada.");

        // Calcula o saldo acumulado de todas as movimentações ANTES do período
        var movimentacoesAnteriores = await _context.MovimentacoesFinanceiras
            .AsNoTracking()
            .Where(m =>
                m.ContaBancariaId == request.ContaBancariaId &&
                m.DataMovimentacao < request.DataInicio.Date)
            .Select(m => new { m.Tipo, m.Valor })
            .ToListAsync(cancellationToken);

        var saldoAnterior = movimentacoesAnteriores.Sum(m =>
            m.Tipo == TipoOperacaoFinanceira.Entrada ? m.Valor : -m.Valor);

        // Saldo inicial do período = saldo inicial cadastrado + movimentações anteriores
        var saldoInicial = conta.SaldoInicial + saldoAnterior;

        // Monta a query de movimentações do período com filtros opcionais
        var query = _context.MovimentacoesFinanceiras
            .AsNoTracking()
            .Include(m => m.PlanoConta)
            .Include(m => m.CentroCusto)
            .Where(m =>
                m.ContaBancariaId == request.ContaBancariaId &&
                m.DataMovimentacao >= request.DataInicio.Date &&
                m.DataMovimentacao <= request.DataFim.Date);

        if (request.Tipo.HasValue)
            query = query.Where(m => m.Tipo == request.Tipo.Value);

        if (request.PlanoContaId.HasValue)
            query = query.Where(m => m.PlanoContaId == request.PlanoContaId.Value);

        if (request.CentroCustoId.HasValue)
            query = query.Where(m => m.CentroCustoId == request.CentroCustoId.Value);

        var movimentacoes = await query
            .OrderBy(m => m.DataMovimentacao)
            .ThenBy(m => m.Id)
            .Select(m => new
            {
                m.Id,
                m.DataMovimentacao,
                m.Descricao,
                m.Tipo,
                m.Valor,
                PlanoConta = m.PlanoConta != null ? m.PlanoConta.Descricao : null,
                CentroCusto = m.CentroCusto != null ? m.CentroCusto.Descricao : null,
                m.DocumentoReferencia,
                m.Observacoes
            })
            .ToListAsync(cancellationToken);

        // Calcula o saldo acumulado linha a linha
        var saldoAcumulado = saldoInicial;
        var itens = new List<ExtratoBancarioItemDto>(movimentacoes.Count);

        foreach (var mov in movimentacoes)
        {
            saldoAcumulado += mov.Tipo == TipoOperacaoFinanceira.Entrada ? mov.Valor : -mov.Valor;

            itens.Add(new ExtratoBancarioItemDto(
                mov.Id,
                mov.DataMovimentacao,
                mov.Descricao,
                mov.Tipo,
                mov.Valor,
                saldoAcumulado,
                mov.PlanoConta,
                mov.CentroCusto,
                mov.DocumentoReferencia,
                mov.Observacoes
            ));
        }

        var totalEntradas = movimentacoes
            .Where(m => m.Tipo == TipoOperacaoFinanceira.Entrada)
            .Sum(m => m.Valor);

        var totalSaidas = movimentacoes
            .Where(m => m.Tipo != TipoOperacaoFinanceira.Entrada)
            .Sum(m => m.Valor);

        var saldoFinal = saldoInicial + totalEntradas - totalSaidas;

        return new ExtratoBancarioDto(
            request.ContaBancariaId,
            conta.Descricao,
            request.DataInicio.Date,
            request.DataFim.Date,
            saldoInicial,
            saldoFinal,
            totalEntradas,
            totalSaidas,
            itens
        );
    }
}
