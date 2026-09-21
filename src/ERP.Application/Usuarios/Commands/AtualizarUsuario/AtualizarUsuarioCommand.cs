using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities;
using ERP.Domain.Core.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Usuarios.Commands.AtualizarUsuario;

public record AtualizarUsuarioCommand(
    string Id,
    string NomeCompleto,
    string Email,
    bool Ativo,
    List<string> Papeis,
    List<int> EmpresasIds) : IRequest<Result>;

public class AtualizarUsuarioCommandValidator : AbstractValidator<AtualizarUsuarioCommand>
{
    public AtualizarUsuarioCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NomeCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class AtualizarUsuarioCommandHandler : IRequestHandler<AtualizarUsuarioCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public AtualizarUsuarioCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result> Handle(AtualizarUsuarioCommand request, CancellationToken cancellationToken)
    {
        var sucesso = await _identityService.AtualizarUsuarioAsync(
            request.Id,
            request.NomeCompleto,
            request.Email,
            request.Ativo,
            request.Papeis);

        if (!sucesso)
            return Result.Falha("Não foi possível atualizar os dados do usuário.");

        // Atualizar empresas vinculadas
        var vinculosAtuais = await _context.UsuarioEmpresas
            .Where(ue => ue.UsuarioId == request.Id)
            .ToListAsync(cancellationToken);

        _context.UsuarioEmpresas.RemoveRange(vinculosAtuais);

        if (request.EmpresasIds != null)
        {
            foreach (var empId in request.EmpresasIds)
            {
                _context.UsuarioEmpresas.Add(new UsuarioEmpresa
                {
                    UsuarioId = request.Id,
                    EmpresaId = empId,
                    Ativo = true
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
