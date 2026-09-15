using ERP.Application.Cadastros.Fornecedores.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Fornecedores.Queries;

public record GetFornecedoresQuery(string? Busca = null, bool? Ativo = null, int Pagina = 1, int TamanhoPagina = 20) : IRequest<PaginatedList<FornecedorListItemDto>>;

public class GetFornecedoresQueryHandler : IRequestHandler<GetFornecedoresQuery, PaginatedList<FornecedorListItemDto>>
{
    private readonly IApplicationDbContext _context;
    public GetFornecedoresQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<FornecedorListItemDto>> Handle(GetFornecedoresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Fornecedores.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Busca))
            query = query.Where(f => f.RazaoSocial.Contains(request.Busca) || f.Cnpj.Contains(request.Busca) || (f.NomeFantasia != null && f.NomeFantasia.Contains(request.Busca)));
        if (request.Ativo.HasValue) query = query.Where(f => f.Ativo == request.Ativo.Value);

        var projection = query.OrderBy(f => f.RazaoSocial).Select(f => new FornecedorListItemDto
        { Id = f.Id, RazaoSocial = f.RazaoSocial, NomeFantasia = f.NomeFantasia, Cnpj = f.Cnpj, Email = f.Email, Telefone = f.Telefone, Ativo = f.Ativo });

        return await PaginatedList<FornecedorListItemDto>.CreateAsync(projection, request.Pagina, request.TamanhoPagina, cancellationToken);
    }
}
