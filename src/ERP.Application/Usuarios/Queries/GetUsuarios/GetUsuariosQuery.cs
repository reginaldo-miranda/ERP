using ERP.Application.Auth.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Usuarios.DTOs;
using ERP.Domain.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Usuarios.Queries.GetUsuarios;

public record GetUsuariosQuery : IRequest<List<UsuarioDetalhesDto>>;

public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, List<UsuarioDetalhesDto>>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public GetUsuariosQueryHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<List<UsuarioDetalhesDto>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
    {
        var users = await _identityService.ObterTodosUsuariosAsync();

        var vinculos = await _context.UsuarioEmpresas
            .AsNoTracking()
            .Where(ue => ue.Ativo)
            .Include(ue => ue.Empresa)
            .ToListAsync(cancellationToken);

        var list = new List<UsuarioDetalhesDto>();

        foreach (var u in users)
        {
            var empresasDoUsuario = vinculos
                .Where(v => v.UsuarioId == u.Id && v.Empresa != null)
                .Select(v => new EmpresaResumoDto
                {
                    Id = v.Empresa.Id,
                    RazaoSocial = v.Empresa.RazaoSocial,
                    NomeFantasia = v.Empresa.NomeFantasia,
                    Cnpj = v.Empresa.Cnpj
                })
                .ToList();

            list.Add(new UsuarioDetalhesDto
            {
                Id = u.Id,
                NomeCompleto = u.NomeCompleto,
                Email = u.Email,
                Ativo = u.Ativo,
                CriadoEm = u.CriadoEm,
                Papeis = u.Papeis.ToList(),
                Empresas = empresasDoUsuario
            });
        }

        return list;
    }
}
