using ERP.Application.Cadastros.Servicos.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Servicos.Queries;

public record GetServicoByIdQuery(int Id) : IRequest<Result<ServicoDto>>;

public class GetServicoByIdQueryHandler : IRequestHandler<GetServicoByIdQuery, Result<ServicoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServicoByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ServicoDto>> Handle(GetServicoByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await _context.Servicos
            .Include(x => x.Categoria)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (s == null)
            return Result<ServicoDto>.Falha("Serviço não encontrado.");

        return Result<ServicoDto>.Ok(new ServicoDto
        {
            Id = s.Id,
            Codigo = s.Codigo,
            Nome = s.Nome,
            Descricao = s.Descricao,
            CategoriaId = s.CategoriaId,
            CategoriaNome = s.Categoria?.Nome,
            PrecoBase = s.PrecoBase,
            Unidade = s.Unidade,
            CodigoServicoCnae = s.CodigoServicoCnae,
            AliquotaIss = s.AliquotaIss,
            Ativo = s.Ativo
        });
    }
}
