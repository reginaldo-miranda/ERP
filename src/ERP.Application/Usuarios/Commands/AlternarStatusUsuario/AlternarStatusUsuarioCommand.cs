using ERP.Application.Common.Models;
using ERP.Domain.Core.Interfaces;
using MediatR;

namespace ERP.Application.Usuarios.Commands.AlternarStatusUsuario;

public record AlternarStatusUsuarioCommand(string Id) : IRequest<Result>;

public class AlternarStatusUsuarioCommandHandler : IRequestHandler<AlternarStatusUsuarioCommand, Result>
{
    private readonly IIdentityService _identityService;

    public AlternarStatusUsuarioCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(AlternarStatusUsuarioCommand request, CancellationToken cancellationToken)
    {
        var sucesso = await _identityService.AlternarStatusUsuarioAsync(request.Id);
        return sucesso ? Result.Ok() : Result.Falha("Não foi possível alterar o status do usuário.");
    }
}
