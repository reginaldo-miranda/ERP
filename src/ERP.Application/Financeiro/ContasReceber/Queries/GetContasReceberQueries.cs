using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Financeiro.ContasReceber.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.ContasReceber.Queries;

public record GetContasReceberQuery(
    string? Busca = null,
    int? ClienteId = null,
    StatusContaFinanceira? Status = null,
    DateTime? DataVencimentoInicio = null,
    DateTime? DataVencimentoFim = null,
    int Pagina = 1,
    int TamanhoPagina = 20
) : IRequest<PaginatedList<ContaReceberListItemDto>>;

public record GetContaReceberByIdQuery(int Id) : IRequest<Result<ContaReceberDto>>;

public record GetResumoContasReceberQuery : IRequest<ResumoContasReceberDto>;

public class GetContasReceberQueriesHandler :
    IRequestHandler<GetContasReceberQuery, PaginatedList<ContaReceberListItemDto>>,
    IRequestHandler<GetContaReceberByIdQuery, Result<ContaReceberDto>>,
    IRequestHandler<GetResumoContasReceberQuery, ResumoContasReceberDto>
{
    private readonly IApplicationDbContext _context;

    public GetContasReceberQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ContaReceberListItemDto>> Handle(GetContasReceberQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ContasReceber.AsNoTracking()
            .Include(c => c.Cliente)
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
                (c.Cliente != null && c.Cliente.Nome.ToLower().Contains(busca))
            );
        }

        if (request.ClienteId.HasValue)
            query = query.Where(c => c.ClienteId == request.ClienteId.Value);

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
            .Select(c => new ContaReceberListItemDto(
                c.Id,
                c.ClienteId,
                c.Cliente != null ? c.Cliente.Nome : null,
                c.Descricao,
                c.NumeroDocumento,
                c.ValorOriginal,
                c.ValorRecebido,
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

        return new PaginatedList<ContaReceberListItemDto>(itens, totalItens, request.Pagina, request.TamanhoPagina);
    }

    public async Task<Result<ContaReceberDto>> Handle(GetContaReceberByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.ContasReceber.AsNoTracking()
            .Include(c => c.Cliente)
            .Include(c => c.FormaPagamento)
            .Include(c => c.ContaBancaria)
            .Include(c => c.PlanoConta)
            .Include(c => c.CentroCusto)
            .Include(c => c.Baixas)
                .ThenInclude(b => b.ContaBancaria)
            .Include(c => c.Baixas)
                .ThenInclude(b => b.FormaPagamento)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (c == null) return Result<ContaReceberDto>.Falha("Conta a receber não encontrada.");

        var baixasDto = c.Baixas.Select(b => new BaixaContaReceberDto(
            b.Id,
            b.ContaReceberId,
            b.ContaBancariaId,
            b.ContaBancaria.Descricao,
            b.FormaPagamentoId,
            b.FormaPagamento.Nome,
            b.DataBaixa,
            b.ValorPrincipal,
            b.ValorJuros,
            b.ValorMulta,
            b.ValorDesconto,
            b.ValorTotalRecebido,
            b.Observacoes
        )).ToList();

        var dto = new ContaReceberDto(
            c.Id,
            c.ClienteId,
            c.Cliente?.Nome,
            c.Descricao,
            c.NumeroDocumento,
            c.ValorOriginal,
            c.ValorRecebido,
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

        return Result<ContaReceberDto>.Ok(dto);
    }

    public async Task<ResumoContasReceberDto> Handle(GetResumoContasReceberQuery request, CancellationToken cancellationToken)
    {
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = inicioMes.AddMonths(1).AddTicks(-1);

        var query = _context.ContasReceber.AsNoTracking()
            .Where(c => c.Status != StatusContaFinanceira.Cancelada);

        var vencidos = await query
            .Where(c => c.Status != StatusContaFinanceira.Paga && c.DataVencimento.Date < hoje)
            .Select(c => c.SaldoRestante)
            .ToListAsync(cancellationToken);

        var aVencer = await query
            .Where(c => c.Status != StatusContaFinanceira.Paga && c.DataVencimento.Date >= hoje)
            .Select(c => c.SaldoRestante)
            .ToListAsync(cancellationToken);

        var recebidosNoMes = await _context.BaixasContasReceber.AsNoTracking()
            .Where(b => b.DataBaixa >= inicioMes && b.DataBaixa <= fimMes)
            .Select(b => b.ValorTotalRecebido)
            .ToListAsync(cancellationToken);

        var totalAberto = vencidos.Sum() + aVencer.Sum();

        return new ResumoContasReceberDto(
            TotalVencido: vencidos.Sum(),
            QuantidadeVencidos: vencidos.Count,
            TotalAVencer: aVencer.Sum(),
            QuantidadeAVencer: aVencer.Count,
            TotalRecebidoNoMes: recebidosNoMes.Sum(),
            QuantidadeRecebidosNoMes: recebidosNoMes.Count,
            TotalAberto: totalAberto
        );
    }
}
