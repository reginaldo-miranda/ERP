using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;

namespace ERP.Application.Cadastros.Categorias.Commands;

public record DeleteCategoriaCommand(int Id) : IRequest<Result>;

public class DeleteCategoriaCommandHandler : IRequestHandler<DeleteCategoriaCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoriaCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteCategoriaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _context.Categorias.FindAsync(new object[] { request.Id }, cancellationToken);
        if (categoria == null) return Result.Falha("Categoria não encontrada.");

        categoria.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
