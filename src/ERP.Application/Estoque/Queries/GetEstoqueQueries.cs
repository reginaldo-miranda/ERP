using ERP.Application.Common.Interfaces;
using ERP.Application.Estoque.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Estoque.Queries;

public record GetDepositosQuery(bool SomenteAtivos = true) : IRequest<List<DepositoDto>>;

public record GetPosicaoEstoqueQuery(
    int? DepositoId = null,
    string? Busca = null,
    bool? SomenteAbaixoMinimo = null
) : IRequest<List<PosicaoEstoqueDto>>;

public record GetMovimentacoesEstoqueQuery(
    int? ProdutoId = null,
    int? DepositoId = null,
    int Top = 100
) : IRequest<List<MovimentacaoEstoqueDto>>;

public class GetEstoqueQueriesHandler :
    IRequestHandler<GetDepositosQuery, List<DepositoDto>>,
    IRequestHandler<GetPosicaoEstoqueQuery, List<PosicaoEstoqueDto>>,
    IRequestHandler<GetMovimentacoesEstoqueQuery, List<MovimentacaoEstoqueDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEstoqueQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepositoDto>> Handle(GetDepositosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Depositos.AsNoTracking();
        if (request.SomenteAtivos)
            query = query.Where(d => d.Ativo);

        return await query
            .OrderByDescending(d => d.Padrao)
            .ThenBy(d => d.Nome)
            .Select(d => new DepositoDto(
                d.Id,
                d.Nome,
                d.Codigo,
                d.Endereco,
                d.Responsavel,
                d.Ativo,
                d.Padrao
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PosicaoEstoqueDto>> Handle(GetPosicaoEstoqueQuery request, CancellationToken cancellationToken)
    {
        var query = _context.EstoqueProdutos
            .AsNoTracking()
            .Include(ep => ep.Produto)
                .ThenInclude(p => p.UnidadeMedida)
            .Include(ep => ep.Deposito)
            .AsQueryable();

        if (request.DepositoId.HasValue && request.DepositoId.Value > 0)
        {
            query = query.Where(ep => ep.DepositoId == request.DepositoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(ep =>
                ep.Produto.Nome.ToLower().Contains(termo) ||
                ep.Produto.Codigo.ToLower().Contains(termo) ||
                (ep.Produto.CodigoBarras != null && ep.Produto.CodigoBarras.ToLower().Contains(termo))
            );
        }

        if (request.SomenteAbaixoMinimo == true)
        {
            query = query.Where(ep => ep.Quantidade <= ep.EstoqueMinimo);
        }

        var itens = await query
            .OrderBy(ep => ep.Produto.Nome)
            .ThenBy(ep => ep.Deposito.Nome)
            .Select(ep => new PosicaoEstoqueDto(
                ep.Id,
                ep.ProdutoId,
                ep.Produto.Codigo,
                ep.Produto.Nome,
                ep.Produto.UnidadeMedida != null ? ep.Produto.UnidadeMedida.Sigla : null,
                ep.DepositoId,
                ep.Deposito.Nome,
                ep.Quantidade,
                ep.CustoMedio,
                ep.CustoUltimaCompra,
                ep.EstoqueMinimo,
                ep.EstoqueMaximo,
                ep.Quantidade <= ep.EstoqueMinimo,
                ep.Quantidade * (ep.CustoMedio > 0 ? ep.CustoMedio : ep.Produto.PrecoCusto)
            ))
            .ToListAsync(cancellationToken);

        return itens;
    }

    public async Task<List<MovimentacaoEstoqueDto>> Handle(GetMovimentacoesEstoqueQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MovimentacoesEstoque
            .AsNoTracking()
            .Include(m => m.Produto)
            .Include(m => m.DepositoOrigem)
            .Include(m => m.DepositoDestino)
            .AsQueryable();

        if (request.ProdutoId.HasValue && request.ProdutoId.Value > 0)
        {
            query = query.Where(m => m.ProdutoId == request.ProdutoId.Value);
        }

        if (request.DepositoId.HasValue && request.DepositoId.Value > 0)
        {
            query = query.Where(m => m.DepositoOrigemId == request.DepositoId.Value || m.DepositoDestinoId == request.DepositoId.Value);
        }

        return await query
            .OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.Id)
            .Take(request.Top)
            .Select(m => new MovimentacaoEstoqueDto(
                m.Id,
                m.ProdutoId,
                m.Produto.Nome,
                m.Produto.Codigo,
                m.DepositoOrigemId,
                m.DepositoOrigem.Nome,
                m.DepositoDestinoId,
                m.DepositoDestino != null ? m.DepositoDestino.Nome : null,
                m.Tipo,
                m.Origem,
                m.Quantidade,
                m.CustoUnitario,
                m.CustoTotal,
                m.DataMovimentacao,
                m.DocumentoOrigem,
                m.Observacao
            ))
            .ToListAsync(cancellationToken);
    }
}
