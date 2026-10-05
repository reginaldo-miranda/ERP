using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Compras.Queries;

public record GetSolicitacoesCompraQuery(
    string? Busca,
    StatusSolicitacaoCompra? Status,
    PrioridadeSolicitacaoCompra? Prioridade,
    DateTime? DataInicio,
    DateTime? DataFim,
    int Pagina = 1,
    int TamanhoPagina = 50
) : IRequest<PaginatedList<SolicitacaoCompraListItemDto>>;

public class GetSolicitacoesCompraQueryHandler : IRequestHandler<GetSolicitacoesCompraQuery, PaginatedList<SolicitacaoCompraListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSolicitacoesCompraQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<SolicitacaoCompraListItemDto>> Handle(GetSolicitacoesCompraQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SolicitacoesCompra
            .AsNoTracking()
            .Include(s => s.Itens)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(s => s.Numero.ToLower().Contains(termo) ||
                                     s.SolicitanteNome.ToLower().Contains(termo) ||
                                     (s.Departamento != null && s.Departamento.ToLower().Contains(termo)));
        }

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.Prioridade.HasValue)
            query = query.Where(s => s.Prioridade == request.Prioridade.Value);

        if (request.DataInicio.HasValue)
            query = query.Where(s => s.DataSolicitacao >= request.DataInicio.Value);

        if (request.DataFim.HasValue)
            query = query.Where(s => s.DataSolicitacao <= request.DataFim.Value);

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(s => s.DataSolicitacao)
            .ThenByDescending(s => s.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(s => new SolicitacaoCompraListItemDto(
                s.Id,
                s.Numero,
                s.SolicitanteNome,
                s.Departamento,
                s.DataSolicitacao,
                s.DataNecessidade,
                s.Status,
                s.Prioridade,
                s.Itens.Count,
                s.Justificativa,
                s.AprovadorNome,
                s.DataAprovacao
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<SolicitacaoCompraListItemDto>(itens, total, request.Pagina, request.TamanhoPagina);
    }
}

public record GetSolicitacaoCompraByIdQuery(int Id) : IRequest<SolicitacaoCompraDetalheDto?>;

public class GetSolicitacaoCompraByIdQueryHandler : IRequestHandler<GetSolicitacaoCompraByIdQuery, SolicitacaoCompraDetalheDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSolicitacaoCompraByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SolicitacaoCompraDetalheDto?> Handle(GetSolicitacaoCompraByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await _context.SolicitacoesCompra
            .AsNoTracking()
            .Include(x => x.Itens)
                .ThenInclude(i => i.Produto)
                    .ThenInclude(p => p.UnidadeMedida)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (s == null) return null;

        return new SolicitacaoCompraDetalheDto(
            s.Id,
            s.Numero,
            s.SolicitanteId,
            s.SolicitanteNome,
            s.Departamento,
            s.DataSolicitacao,
            s.DataNecessidade,
            s.Status,
            s.Prioridade,
            s.Justificativa,
            s.Observacoes,
            s.AprovadorNome,
            s.DataAprovacao,
            s.MotivoRejeicao,
            s.Itens.Select(i => new SolicitacaoCompraItemDto(
                i.Id,
                i.ProdutoId,
                i.Produto.Nome,
                i.Produto.Codigo,
                i.Produto.UnidadeMedida?.Sigla ?? "UN",
                i.Quantidade,
                i.QuantidadeAtendida,
                i.Observacao
            )).ToList()
        );
    }
}
