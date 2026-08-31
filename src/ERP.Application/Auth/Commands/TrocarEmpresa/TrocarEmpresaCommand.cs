using ERP.Application.Auth.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Auth.Commands.TrocarEmpresa;

/// <summary>
/// Command para trocar a empresa ativa do usuário.
/// Gera um novo JWT com a empresa selecionada.
/// </summary>
public record TrocarEmpresaCommand(int EmpresaId) : IRequest<Result<LoginResultDto>>;

public class TrocarEmpresaCommandHandler : IRequestHandler<TrocarEmpresaCommand, Result<LoginResultDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;

    public TrocarEmpresaCommandHandler(
        ICurrentUserService currentUser,
        IIdentityService identityService,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    public async Task<Result<LoginResultDto>> Handle(TrocarEmpresaCommand request, CancellationToken cancellationToken)
    {
        var usuarioId = _currentUser.UsuarioId!;

        // Verificar se o usuário tem acesso à empresa
        var temAcesso = await _context.UsuarioEmpresas
            .AnyAsync(ue => ue.UsuarioId == usuarioId && ue.EmpresaId == request.EmpresaId && ue.Ativo, cancellationToken);

        if (!temAcesso)
        {
            return Result<LoginResultDto>.Falha("Você não tem acesso a esta empresa.");
        }

        // Obter dados atualizados
        var (nomeCompleto, email) = await _identityService.ObterDadosUsuarioAsync(usuarioId);
        var empresas = await _context.UsuarioEmpresas
            .Where(ue => ue.UsuarioId == usuarioId && ue.Ativo)
            .Include(ue => ue.Empresa)
            .Select(ue => ue.Empresa)
            .Where(e => e.Ativo)
            .ToListAsync(cancellationToken);

        var papeis = await _identityService.ObterPapeisAsync(usuarioId);
        var permissoes = await _context.PapelPermissoes
            .Where(pp => papeis.Contains(pp.PapelId))
            .Include(pp => pp.Permissao)
            .Where(pp => pp.Permissao.Ativo)
            .Select(pp => pp.Permissao.Codigo)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Gerar novo token com a nova empresa
        var token = _jwtTokenService.GerarToken(
            usuarioId, email!, nomeCompleto!,
            request.EmpresaId,
            empresas.Select(e => e.Id),
            permissoes);

        var refreshToken = _jwtTokenService.GerarRefreshToken();

        return Result<LoginResultDto>.Ok(new LoginResultDto
        {
            Token = token,
            RefreshToken = refreshToken,
            TokenExpiracao = DateTime.UtcNow.AddHours(1),
            Usuario = new UsuarioDto
            {
                Id = usuarioId,
                NomeCompleto = nomeCompleto ?? "",
                Email = email ?? "",
                EmpresaAtualId = request.EmpresaId,
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
        });
    }
}
