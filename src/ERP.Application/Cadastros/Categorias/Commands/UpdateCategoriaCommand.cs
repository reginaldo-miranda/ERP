using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ERP.Application.Cadastros.Categorias.Commands;

public record UpdateCategoriaCommand(int Id, string Nome) : IRequest<Result>;

public class UpdateCategoriaCommandValidator : AbstractValidator<UpdateCategoriaCommand>
{
    public UpdateCategoriaCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100);
    }
}

public class UpdateCategoriaCommandHandler : IRequestHandler<UpdateCategoriaCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoriaCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _context.Categorias.FindAsync(new object[] { request.Id }, cancellationToken);
        if (categoria == null) return Result.Falha("Categoria não encontrada.");

        categoria.Nome = request.Nome;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
