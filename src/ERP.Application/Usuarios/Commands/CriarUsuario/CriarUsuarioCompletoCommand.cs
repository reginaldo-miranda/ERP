using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities;
using ERP.Domain.Core.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Usuarios.Commands.CriarUsuario;

public record CriarUsuarioCompletoCommand(
    string NomeCompleto,
    string Email,
    string Senha,
    List<string> Papeis,
    List<int> EmpresasIds) : IRequest<Result<string>>;

public class CriarUsuarioCompletoCommandValidator : AbstractValidator<CriarUsuarioCompletoCommand>
{
    public CriarUsuarioCompletoCommandValidator()
    {
        RuleFor(x => x.NomeCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(6);
    }
}

public class CriarUsuarioCompletoCommandHandler : IRequestHandler<CriarUsuarioCompletoCommand, Result<string>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public CriarUsuarioCompletoCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result<string>> Handle(CriarUsuarioCompletoCommand request, CancellationToken cancellationToken)
    {
        if (await _identityService.UsuarioExisteAsync(request.Email))
            return Result<string>.Falha("Já existe um usuário cadastrado com este e-mail.");

        var (sucesso, usuarioId, erros) = await _identityService.CriarUsuarioAsync(request.Email, request.Senha, request.NomeCompleto);
        if (!sucesso || usuarioId == null)
            return Result<string>.Falha(erros);

        // Atribuir papéis
        foreach (var papel in request.Papeis)
        {
            await _identityService.AdicionarAoPapelAsync(usuarioId, papel);
        }

        // Atribuir empresas
        if (request.EmpresasIds != null && request.EmpresasIds.Any())
        {
            foreach (var empId in request.EmpresasIds)
            {
                _context.UsuarioEmpresas.Add(new UsuarioEmpresa
                {
                    UsuarioId = usuarioId,
                    EmpresaId = empId,
                    Ativo = true
                });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<string>.Ok(usuarioId);
    }
}
