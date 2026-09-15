using ERP.Application.Cadastros.Produtos.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Produtos.Queries;

public record GetProdutosQuery(string? Busca = null, int? CategoriaId = null, bool? Ativo = null, int Pagina = 1, int TamanhoPagina = 20) : IRequest<PaginatedList<ProdutoListItemDto>>;

public class GetProdutosQueryHandler : IRequestHandler<GetProdutosQuery, PaginatedList<ProdutoListItemDto>>
{
    private readonly IApplicationDbContext _context;
    public GetProdutosQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<ProdutoListItemDto>> Handle(GetProdutosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Produtos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Busca))
            query = query.Where(p => p.Nome.Contains(request.Busca) || p.Codigo.Contains(request.Busca) || (p.CodigoBarras != null && p.CodigoBarras.Contains(request.Busca)));
        if (request.CategoriaId.HasValue) query = query.Where(p => p.CategoriaId == request.CategoriaId.Value);
        if (request.Ativo.HasValue) query = query.Where(p => p.Ativo == request.Ativo.Value);

        var projection = query.OrderBy(p => p.Nome).Select(p => new ProdutoListItemDto
        { Id = p.Id, Codigo = p.Codigo, Nome = p.Nome, CategoriaNome = p.Categoria != null ? p.Categoria.Nome : null, UnidadeSigla = p.UnidadeMedida != null ? p.UnidadeMedida.Sigla : null, PrecoVenda = p.PrecoVenda, Ativo = p.Ativo });

        return await PaginatedList<ProdutoListItemDto>.CreateAsync(projection, request.Pagina, request.TamanhoPagina, cancellationToken);
    }
}
