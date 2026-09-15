using ERP.Application.Cadastros.Fornecedores.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Fornecedores.Queries;

public record GetFornecedorByIdQuery(int Id) : IRequest<Result<FornecedorDto>>;

public class GetFornecedorByIdQueryHandler : IRequestHandler<GetFornecedorByIdQuery, Result<FornecedorDto>>
{
    private readonly IApplicationDbContext _context;
    public GetFornecedorByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<FornecedorDto>> Handle(GetFornecedorByIdQuery request, CancellationToken cancellationToken)
    {
        var f = await _context.Fornecedores.Include(x => x.Enderecos).FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (f == null) return Result<FornecedorDto>.Falha("Fornecedor não encontrado.");

        return Result<FornecedorDto>.Ok(new FornecedorDto
        {
            Id = f.Id, RazaoSocial = f.RazaoSocial, NomeFantasia = f.NomeFantasia, Cnpj = f.Cnpj,
            InscricaoEstadual = f.InscricaoEstadual, Email = f.Email, Telefone = f.Telefone, Celular = f.Celular,
            Contato = f.Contato, Site = f.Site, Observacoes = f.Observacoes, Ativo = f.Ativo,
            Enderecos = f.Enderecos.Select(e => new EnderecoFornDto { Id = e.Id, Logradouro = e.Logradouro, Numero = e.Numero, Cidade = e.Cidade, Uf = e.Uf, Cep = e.Cep, Principal = e.Principal }).ToList()
        });
    }
}
