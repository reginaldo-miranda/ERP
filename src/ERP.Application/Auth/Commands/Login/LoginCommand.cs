using ERP.Application.Auth.DTOs;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Auth.Commands.Login;

/// <summary>
/// Command para autenticar um usuário via email e senha.
/// Retorna JWT + RefreshToken + dados do usuário.
/// </summary>
public record LoginCommand(string Email, string Senha) : IRequest<Result<LoginResultDto>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context)
    {
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    public async Task<Result<LoginResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Validar credenciais
        var (sucesso, usuarioId) = await _identityService.ValidarCredenciaisAsync(request.Email, request.Senha);

        if (!sucesso || usuarioId == null)
        {
            return Result<LoginResultDto>.Falha("Email ou senha inválidos.");
        }

        // Obter dados do usuário
        var (nomeCompleto, email) = await _identityService.ObterDadosUsuarioAsync(usuarioId);

        // Obter empresas do usuário
        var empresas = await _context.UsuarioEmpresas
            .Where(ue => ue.UsuarioId == usuarioId && ue.Ativo)
            .Include(ue => ue.Empresa)
            .Select(ue => ue.Empresa)
            .Where(e => e.Ativo)
            .ToListAsync(cancellationToken);

        if (!empresas.Any())
        {
            return Result<LoginResultDto>.Falha("Usuário não está vinculado a nenhuma empresa ativa.");
        }

        var empresaAtual = empresas.First();

        // Obter permissões do usuário
        var papeis = await _identityService.ObterPapeisAsync(usuarioId);
        var permissoes = await _context.PapelPermissoes
            .Where(pp => papeis.Contains(pp.PapelId))
            .Include(pp => pp.Permissao)
            .Where(pp => pp.Permissao.Ativo)
            .Select(pp => pp.Permissao.Codigo)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Gerar tokens
        var token = _jwtTokenService.GerarToken(
            usuarioId,
            email!,
            nomeCompleto!,
            empresaAtual.Id,
            empresas.Select(e => e.Id),
            permissoes);

        var refreshToken = _jwtTokenService.GerarRefreshToken();

        var resultado = new LoginResultDto
        {
            Token = token,
            RefreshToken = refreshToken,
            TokenExpiracao = DateTime.UtcNow.AddHours(1),
            Usuario = new UsuarioDto
            {
                Id = usuarioId,
                NomeCompleto = nomeCompleto ?? "",
                Email = email ?? "",
                EmpresaAtualId = empresaAtual.Id,
                Empresas = empresas.Select(e => new EmpresaResumoDto
                {
                    Id = e.Id,
                    RazaoSocial = e.RazaoSocial,
                    NomeFantasia = e.NomeFantasia,
                    Cnpj = e.Cnpj
                }).ToList(),
                Permissoes = permissoes,
                Papeis = papeis.ToList()
            }
        };

        return Result<LoginResultDto>.Ok(resultado);
    }
}
