using ERP.Application.Cadastros.Clientes.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Clientes.Queries;

public record GetClienteByIdQuery(int Id) : IRequest<Result<ClienteDto>>;

public class GetClienteByIdQueryHandler : IRequestHandler<GetClienteByIdQuery, Result<ClienteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClienteByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<ClienteDto>> Handle(GetClienteByIdQuery request, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Enderecos)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (cliente == null) return Result<ClienteDto>.Falha("Cliente não encontrado.");

        return Result<ClienteDto>.Ok(new ClienteDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            TipoPessoa = cliente.TipoPessoa,
            CpfCnpj = cliente.CpfCnpj,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            Celular = cliente.Celular,
            NomeFantasia = cliente.NomeFantasia,
            InscricaoEstadual = cliente.InscricaoEstadual,
            LimiteCredito = cliente.LimiteCredito,
            Observacoes = cliente.Observacoes,
            Ativo = cliente.Ativo,
            Enderecos = cliente.Enderecos.Select(e => new EnderecoDto
            {
                Id = e.Id, Logradouro = e.Logradouro, Numero = e.Numero,
                Complemento = e.Complemento, Bairro = e.Bairro, Cidade = e.Cidade,
                Uf = e.Uf, Cep = e.Cep, Principal = e.Principal
            }).ToList()
        });
    }
}
