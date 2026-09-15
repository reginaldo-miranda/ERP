using ERP.Application.Cadastros.Servicos.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;

namespace ERP.Application.Cadastros.Servicos.Queries;

public record GetServicosQuery(string? Busca = null, bool? Ativo = null, int Pagina = 1, int TamanhoPagina = 20) : IRequest<PaginatedList<ServicoListItemDto>>;

public class GetServicosQueryHandler : IRequestHandler<GetServicosQuery, PaginatedList<ServicoListItemDto>>
{
    private readonly IApplicationDbContext _context;
    public GetServicosQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<ServicoListItemDto>> Handle(GetServicosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Servicos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Busca)) query = query.Where(s => s.Nome.Contains(request.Busca) || s.Codigo.Contains(request.Busca));
        if (request.Ativo.HasValue) query = query.Where(s => s.Ativo == request.Ativo.Value);

        var projection = query.OrderBy(s => s.Nome).Select(s => new ServicoListItemDto
        { Id = s.Id, Codigo = s.Codigo, Nome = s.Nome, CategoriaNome = s.Categoria != null ? s.Categoria.Nome : null, PrecoBase = s.PrecoBase, Unidade = s.Unidade, Ativo = s.Ativo });

        return await PaginatedList<ServicoListItemDto>.CreateAsync(projection, request.Pagina, request.TamanhoPagina, cancellationToken);
    }
}
