using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;

namespace ERP.Application.Cadastros.Clientes.Commands;

public record DeleteClienteCommand(int Id) : IRequest<Result>;

public class DeleteClienteCommandHandler : IRequestHandler<DeleteClienteCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteClienteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (cliente == null) return Result.Falha("Cliente não encontrado.");

        cliente.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
