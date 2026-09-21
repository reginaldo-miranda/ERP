using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Financeiro.ContasPagar.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.ContasPagar.Queries;

public record GetContasPagarQuery(
    string? Busca = null,
    int? FornecedorId = null,
    StatusContaFinanceira? Status = null,
    DateTime? DataVencimentoInicio = null,
    DateTime? DataVencimentoFim = null,
    int Pagina = 1,
    int TamanhoPagina = 20
) : IRequest<PaginatedList<ContaPagarListItemDto>>;

public record GetContaPagarByIdQuery(int Id) : IRequest<Result<ContaPagarDto>>;

public record GetResumoContasPagarQuery : IRequest<ResumoContasPagarDto>;

public class GetContasPagarQueriesHandler :
    IRequestHandler<GetContasPagarQuery, PaginatedList<ContaPagarListItemDto>>,
    IRequestHandler<GetContaPagarByIdQuery, Result<ContaPagarDto>>,
    IRequestHandler<GetResumoContasPagarQuery, ResumoContasPagarDto>
{
    private readonly IApplicationDbContext _context;

    public GetContasPagarQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ContaPagarListItemDto>> Handle(GetContasPagarQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ContasPagar.AsNoTracking()
            .Include(c => c.Fornecedor)
            .Include(c => c.FormaPagamento)
            .Include(c => c.ContaBancaria)
            .Include(c => c.PlanoConta)
            .Include(c => c.CentroCusto)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = request.Busca.Trim().ToLower();
            query = query.Where(c =>
                c.Descricao.ToLower().Contains(busca) ||
                (c.NumeroDocumento != null && c.NumeroDocumento.ToLower().Contains(busca)) ||
                (c.Fornecedor != null && c.Fornecedor.RazaoSocial.ToLower().Contains(busca))
            );
        }

        if (request.FornecedorId.HasValue)
            query = query.Where(c => c.FornecedorId == request.FornecedorId.Value);

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.DataVencimentoInicio.HasValue)
        {
            var inicioUtc = request.DataVencimentoInicio.Value.Date.ToUniversalTime();
            query = query.Where(c => c.DataVencimento >= inicioUtc);
        }

        if (request.DataVencimentoFim.HasValue)
        {
            var fimUtc = request.DataVencimentoFim.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(c => c.DataVencimento <= fimUtc);
        }

        var totalItens = await query.CountAsync(cancellationToken);
        var hoje = DateTime.UtcNow.Date;

        var itens = await query
            .OrderBy(c => c.DataVencimento)
            .ThenBy(c => c.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(c => new ContaPagarListItemDto(
                c.Id,
                c.FornecedorId,
                c.Fornecedor != null ? c.Fornecedor.RazaoSocial : null,
                c.Descricao,
                c.NumeroDocumento,
                c.ValorOriginal,
                c.ValorPago,
                c.SaldoRestante,
                c.DataEmissao,
                c.DataVencimento,
                c.Status,
                c.NumeroParcela,
                c.TotalParcelas,
                c.FormaPagamento != null ? c.FormaPagamento.Nome : null,
                c.ContaBancaria != null ? c.ContaBancaria.Descricao : null,
                c.PlanoConta != null ? c.PlanoConta.Descricao : null,
                c.CentroCusto != null ? c.CentroCusto.Descricao : null,
                c.DataVencimento.Date < hoje && c.Status != StatusContaFinanceira.Paga && c.Status != StatusContaFinanceira.Cancelada
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<ContaPagarListItemDto>(itens, totalItens, request.Pagina, request.TamanhoPagina);
    }

    public async Task<Result<ContaPagarDto>> Handle(GetContaPagarByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.ContasPagar.AsNoTracking()
            .Include(c => c.Fornecedor)
            .Include(c => c.FormaPagamento)
            .Include(c => c.ContaBancaria)
            .Include(c => c.PlanoConta)
            .Include(c => c.CentroCusto)
            .Include(c => c.Baixas)
                .ThenInclude(b => b.ContaBancaria)
            .Include(c => c.Baixas)
                .ThenInclude(b => b.FormaPagamento)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (c == null) return Result<ContaPagarDto>.Falha("Conta a pagar não encontrada.");

        var baixasDto = c.Baixas.Select(b => new BaixaContaPagarDto(
            b.Id,
            b.ContaPagarId,
            b.ContaBancariaId,
            b.ContaBancaria.Descricao,
            b.FormaPagamentoId,
            b.FormaPagamento.Nome,
            b.DataBaixa,
            b.ValorPrincipal,
            b.ValorJuros,
            b.ValorMulta,
            b.ValorDesconto,
            b.ValorTotalPago,
            b.Observacoes
        )).ToList();

        var dto = new ContaPagarDto(
            c.Id,
            c.FornecedorId,
            c.Fornecedor?.RazaoSocial,
            c.Descricao,
            c.NumeroDocumento,
            c.ValorOriginal,
            c.ValorPago,
            c.SaldoRestante,
            c.DataEmissao,
            c.DataVencimento,
            c.DataCompetencia,
            c.Status,
            c.FormaPagamentoId,
            c.FormaPagamento?.Nome,
            c.ContaBancariaId,
            c.ContaBancaria?.Descricao,
            c.PlanoContaId,
            c.PlanoConta?.Descricao,
            c.CentroCustoId,
            c.CentroCusto?.Descricao,
            c.NumeroParcela,
            c.TotalParcelas,
            c.IdParcelamento,
            c.Observacoes,
            baixasDto
        );

        return Result<ContaPagarDto>.Ok(dto);
    }

    public async Task<ResumoContasPagarDto> Handle(GetResumoContasPagarQuery request, CancellationToken cancellationToken)
    {
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = inicioMes.AddMonths(1).AddTicks(-1);

        var query = _context.ContasPagar.AsNoTracking()
            .Where(c => c.Status != StatusContaFinanceira.Cancelada);

        var vencidos = await query
            .Where(c => c.Status != StatusContaFinanceira.Paga && c.DataVencimento.Date < hoje)
            .Select(c => c.SaldoRestante)
            .ToListAsync(cancellationToken);

        var aVencer = await query
            .Where(c => c.Status != StatusContaFinanceira.Paga && c.DataVencimento.Date >= hoje)
            .Select(c => c.SaldoRestante)
            .ToListAsync(cancellationToken);

        var pagosNoMes = await _context.BaixasContasPagar.AsNoTracking()
            .Where(b => b.DataBaixa >= inicioMes && b.DataBaixa <= fimMes)
            .Select(b => b.ValorTotalPago)
            .ToListAsync(cancellationToken);

        var totalAberto = vencidos.Sum() + aVencer.Sum();

        return new ResumoContasPagarDto(
            TotalVencido: vencidos.Sum(),
            QuantidadeVencidos: vencidos.Count,
            TotalAVencer: aVencer.Sum(),
            QuantidadeAVencer: aVencer.Count,
            TotalPagoNoMes: pagosNoMes.Sum(),
            QuantidadePagosNoMes: pagosNoMes.Count,
            TotalAberto: totalAberto
        );
    }
}
