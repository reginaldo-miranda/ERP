using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;

namespace ERP.Application.Cadastros.Produtos.Commands;

public record DeleteProdutoCommand(int Id) : IRequest<Result>;

public class DeleteProdutoCommandHandler : IRequestHandler<DeleteProdutoCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteProdutoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteProdutoCommand request, CancellationToken cancellationToken)
    {
        var p = await _context.Produtos.FindAsync(new object[] { request.Id }, cancellationToken);
        if (p == null) return Result.Falha("Produto não encontrado.");
        p.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
