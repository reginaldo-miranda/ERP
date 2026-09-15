using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;

namespace ERP.Application.Cadastros.Fornecedores.Commands;

public record DeleteFornecedorCommand(int Id) : IRequest<Result>;

public class DeleteFornecedorCommandHandler : IRequestHandler<DeleteFornecedorCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteFornecedorCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteFornecedorCommand request, CancellationToken cancellationToken)
    {
        var f = await _context.Fornecedores.FindAsync(new object[] { request.Id }, cancellationToken);
        if (f == null) return Result.Falha("Fornecedor não encontrado.");
        f.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
