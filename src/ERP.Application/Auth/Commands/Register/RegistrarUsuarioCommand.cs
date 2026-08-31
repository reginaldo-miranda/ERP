using ERP.Application.Common.Models;
using ERP.Domain.Core.Interfaces;
using FluentValidation;
using MediatR;

namespace ERP.Application.Auth.Commands.Register;

/// <summary>
/// Command para registrar um novo usuário no sistema.
/// </summary>
public record RegistrarUsuarioCommand(
    string Email,
    string Senha,
    string NomeCompleto) : IRequest<Result<string>>;

public class RegistrarUsuarioCommandValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");

        RuleFor(x => x.NomeCompleto)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");
    }
}

public class RegistrarUsuarioCommandHandler : IRequestHandler<RegistrarUsuarioCommand, Result<string>>
{
    private readonly IIdentityService _identityService;

    public RegistrarUsuarioCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<string>> Handle(RegistrarUsuarioCommand request, CancellationToken cancellationToken)
    {
        // Verificar se o email já existe
        if (await _identityService.UsuarioExisteAsync(request.Email))
        {
            return Result<string>.Falha("Já existe um usuário com este email.");
        }

        // Criar o usuário
        var (sucesso, usuarioId, erros) = await _identityService.CriarUsuarioAsync(
            request.Email, request.Senha, request.NomeCompleto);

        if (!sucesso)
        {
            return Result<string>.Falha(erros);
        }

        return Result<string>.Ok(usuarioId!);
    }
}
