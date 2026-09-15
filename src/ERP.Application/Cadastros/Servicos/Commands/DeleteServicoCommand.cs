using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;

namespace ERP.Application.Cadastros.Servicos.Commands;

public record DeleteServicoCommand(int Id) : IRequest<Result>;

public class DeleteServicoCommandHandler : IRequestHandler<DeleteServicoCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteServicoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteServicoCommand request, CancellationToken cancellationToken)
    {
        var s = await _context.Servicos.FindAsync(new object[] { request.Id }, cancellationToken);
        if (s == null) return Result.Falha("Serviço não encontrado.");
        s.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
