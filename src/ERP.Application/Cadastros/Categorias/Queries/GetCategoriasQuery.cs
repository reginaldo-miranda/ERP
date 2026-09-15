using ERP.Application.Cadastros.Categorias.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Categorias.Queries;

public record GetCategoriasQuery(TipoCategoria? Tipo = null, bool ApenasAtivas = true) : IRequest<List<CategoriaDto>>;

public class GetCategoriasQueryHandler : IRequestHandler<GetCategoriasQuery, List<CategoriaDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoriaDto>> Handle(GetCategoriasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categorias
            .Where(c => c.CategoriaPaiId == null)
            .AsQueryable();

        if (request.Tipo.HasValue)
            query = query.Where(c => c.Tipo == request.Tipo.Value);

        if (request.ApenasAtivas)
            query = query.Where(c => c.Ativo);

        var categorias = await query
            .OrderBy(c => c.Nome)
            .ToListAsync(cancellationToken);

        return categorias.Select(MapToDto).ToList();
    }

    private static CategoriaDto MapToDto(Domain.Core.Entities.Cadastros.Categoria c)
    {
        return new CategoriaDto
        {
            Id = c.Id,
            Nome = c.Nome,
            Tipo = c.Tipo,
            Nivel = c.Nivel,
            CategoriaPaiId = c.CategoriaPaiId,
            Ativo = c.Ativo,
            Filhos = c.Filhos.Select(MapToDto).ToList()
        };
    }
}
