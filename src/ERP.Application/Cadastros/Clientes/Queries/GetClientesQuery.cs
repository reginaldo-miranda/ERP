using ERP.Application.Cadastros.Clientes.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Clientes.Queries;

public record GetClientesQuery(
    string? Busca = null,
    TipoPessoa? TipoPessoa = null,
    bool? Ativo = null,
    int Pagina = 1,
    int TamanhoPagina = 20
) : IRequest<PaginatedList<ClienteListItemDto>>;

public class GetClientesQueryHandler : IRequestHandler<GetClientesQuery, PaginatedList<ClienteListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<ClienteListItemDto>> Handle(GetClientesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Busca))
            query = query.Where(c => c.Nome.Contains(request.Busca) || c.CpfCnpj.Contains(request.Busca) || (c.Email != null && c.Email.Contains(request.Busca)));

        if (request.TipoPessoa.HasValue)
            query = query.Where(c => c.TipoPessoa == request.TipoPessoa.Value);

        if (request.Ativo.HasValue)
            query = query.Where(c => c.Ativo == request.Ativo.Value);

        var projection = query.OrderBy(c => c.Nome).Select(c => new ClienteListItemDto
        {
            Id = c.Id,
            Nome = c.Nome,
            TipoPessoa = c.TipoPessoa,
            CpfCnpj = c.CpfCnpj,
            Email = c.Email,
            Telefone = c.Telefone,
            Ativo = c.Ativo
        });

        return await PaginatedList<ClienteListItemDto>.CreateAsync(projection, request.Pagina, request.TamanhoPagina, cancellationToken);
    }
}
