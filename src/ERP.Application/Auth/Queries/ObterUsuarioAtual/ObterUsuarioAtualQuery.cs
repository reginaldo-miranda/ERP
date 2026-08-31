using ERP.Application.Auth.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Auth.Queries.ObterUsuarioAtual;

/// <summary>
/// Query para retornar os dados do usuário logado.
/// </summary>
public record ObterUsuarioAtualQuery : IRequest<UsuarioDto?>;

public class ObterUsuarioAtualQueryHandler : IRequestHandler<ObterUsuarioAtualQuery, UsuarioDto?>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public ObterUsuarioAtualQueryHandler(
        ICurrentUserService currentUser,
        IIdentityService identityService,
        IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _context = context;
    }

    public async Task<UsuarioDto?> Handle(ObterUsuarioAtualQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UsuarioId == null)
            return null;

        var usuarioId = _currentUser.UsuarioId;
        var (nomeCompleto, email) = await _identityService.ObterDadosUsuarioAsync(usuarioId);

        var empresas = await _context.UsuarioEmpresas
            .Where(ue => ue.UsuarioId == usuarioId && ue.Ativo)
            .Include(ue => ue.Empresa)
            .Select(ue => ue.Empresa)
            .Where(e => e.Ativo)
            .ToListAsync(cancellationToken);

        var papeis = await _identityService.ObterPapeisAsync(usuarioId);

        return new UsuarioDto
        {
            Id = usuarioId,
            NomeCompleto = nomeCompleto ?? "",
            Email = email ?? "",
            EmpresaAtualId = _currentUser.EmpresaAtualId ?? 0,
            Empresas = empresas.Select(e => new EmpresaResumoDto
            {
                Id = e.Id,
                RazaoSocial = e.RazaoSocial,
                NomeFantasia = e.NomeFantasia,
                Cnpj = e.Cnpj
            }).ToList(),
            Permissoes = _currentUser.Permissoes.ToList(),
            Papeis = papeis.ToList()
        };
    }
}
