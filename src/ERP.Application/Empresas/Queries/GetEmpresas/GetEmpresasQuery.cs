using ERP.Application.Common.Interfaces;
using ERP.Application.Empresas.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Empresas.Queries.GetEmpresas;

public record GetEmpresasQuery(bool ApenasAtivas = true) : IRequest<List<EmpresaDto>>;

public class GetEmpresasQueryHandler : IRequestHandler<GetEmpresasQuery, List<EmpresaDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEmpresasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmpresaDto>> Handle(GetEmpresasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Empresas.AsNoTracking();

        if (request.ApenasAtivas)
            query = query.Where(e => e.Ativo);

        return await query
            .OrderBy(e => e.RazaoSocial)
            .Select(e => new EmpresaDto
            {
                Id = e.Id,
                RazaoSocial = e.RazaoSocial,
                NomeFantasia = e.NomeFantasia,
                Cnpj = e.Cnpj,
                InscricaoEstadual = e.InscricaoEstadual,
                InscricaoMunicipal = e.InscricaoMunicipal,
                Email = e.Email,
                Telefone = e.Telefone,
                Logradouro = e.Logradouro,
                Numero = e.Numero,
                Complemento = e.Complemento,
                Bairro = e.Bairro,
                Cidade = e.Cidade,
                Uf = e.Uf,
                Cep = e.Cep,
                RegimeTributario = e.RegimeTributario,
                MetodoCusteio = e.MetodoCusteio,
                LogoUrl = e.LogoUrl,
                Ativo = e.Ativo,
                CriadoEm = e.CriadoEm
            })
            .ToListAsync(cancellationToken);
    }
}
