using ERP.Application.Cadastros.Categorias.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;
using FluentValidation;
using MediatR;

namespace ERP.Application.Cadastros.Categorias.Commands;

public record CreateCategoriaCommand(
    string Nome,
    TipoCategoria Tipo,
    int? CategoriaPaiId = null
) : IRequest<Result<CategoriaDto>>;

public class CreateCategoriaCommandValidator : AbstractValidator<CreateCategoriaCommand>
{
    public CreateCategoriaCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100).WithMessage("Nome é obrigatório (máx. 100 caracteres).");
    }
}

public class CreateCategoriaCommandHandler : IRequestHandler<CreateCategoriaCommand, Result<CategoriaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateCategoriaCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<CategoriaDto>> Handle(CreateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var nivel = 1;
        if (request.CategoriaPaiId.HasValue)
        {
            var pai = await _context.Categorias.FindAsync(new object[] { request.CategoriaPaiId.Value }, cancellationToken);
            if (pai == null) return Result<CategoriaDto>.Falha("Categoria pai não encontrada.");
            nivel = pai.Nivel + 1;
        }

        var categoria = new Categoria
        {
            Nome = request.Nome,
            Tipo = request.Tipo,
            CategoriaPaiId = request.CategoriaPaiId,
            Nivel = nivel,
            EmpresaId = _empresaService.EmpresaId!.Value
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CategoriaDto>.Ok(new CategoriaDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Tipo = categoria.Tipo,
            Nivel = categoria.Nivel,
            CategoriaPaiId = categoria.CategoriaPaiId,
            Ativo = categoria.Ativo
        });
    }
}
