using ERP.Application.Common.Interfaces;
using ERP.Application.Empresas.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Empresas.Queries.GetEmpresaById;

public record GetEmpresaByIdQuery(int Id) : IRequest<EmpresaDto?>;

public class GetEmpresaByIdQueryHandler : IRequestHandler<GetEmpresaByIdQuery, EmpresaDto?>
{
    private readonly IApplicationDbContext _context;

    public GetEmpresaByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmpresaDto?> Handle(GetEmpresaByIdQuery request, CancellationToken cancellationToken)
    {
        var e = await _context.Empresas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (e == null) return null;

        return new EmpresaDto
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
        };
    }
}
