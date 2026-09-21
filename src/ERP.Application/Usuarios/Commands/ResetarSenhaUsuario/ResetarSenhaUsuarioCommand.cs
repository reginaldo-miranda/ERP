using ERP.Application.Common.Models;
using ERP.Domain.Core.Interfaces;
using FluentValidation;
using MediatR;

namespace ERP.Application.Usuarios.Commands.ResetarSenhaUsuario;

public record ResetarSenhaUsuarioCommand(string Id, string NovaSenha) : IRequest<Result>;

public class ResetarSenhaUsuarioCommandValidator : AbstractValidator<ResetarSenhaUsuarioCommand>
{
    public ResetarSenhaUsuarioCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NovaSenha).NotEmpty().MinimumLength(6);
    }
}

public class ResetarSenhaUsuarioCommandHandler : IRequestHandler<ResetarSenhaUsuarioCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResetarSenhaUsuarioCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(ResetarSenhaUsuarioCommand request, CancellationToken cancellationToken)
    {
        var sucesso = await _identityService.ResetarSenhaAsync(request.Id, request.NovaSenha);
        return sucesso ? Result.Ok() : Result.Falha("Não foi possível redefinir a senha do usuário.");
    }
}
