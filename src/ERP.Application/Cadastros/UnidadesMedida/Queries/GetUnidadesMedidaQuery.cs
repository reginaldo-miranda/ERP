using ERP.Application.Cadastros.UnidadesMedida.DTOs;
using ERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.UnidadesMedida.Queries;

public record GetUnidadesMedidaQuery(bool ApenasAtivas = true) : IRequest<List<UnidadeMedidaDto>>;

public class GetUnidadesMedidaQueryHandler : IRequestHandler<GetUnidadesMedidaQuery, List<UnidadeMedidaDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUnidadesMedidaQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<UnidadeMedidaDto>> Handle(GetUnidadesMedidaQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UnidadesMedida.AsQueryable();
        if (request.ApenasAtivas) query = query.Where(u => u.Ativo);

        return await query
            .OrderBy(u => u.Sigla)
            .Select(u => new UnidadeMedidaDto { Id = u.Id, Sigla = u.Sigla, Descricao = u.Descricao, Ativo = u.Ativo })
            .ToListAsync(cancellationToken);
    }
}
