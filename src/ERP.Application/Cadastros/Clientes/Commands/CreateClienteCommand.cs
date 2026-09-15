using ERP.Application.Cadastros.Clientes.DTOs;
using ERP.Application.Cadastros.Clientes.Helpers;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Clientes.Commands;

public record CreateClienteCommand(
    string Nome,
    TipoPessoa TipoPessoa,
    string CpfCnpj,
    string? Email,
    string? Telefone,
    string? Celular,
    string? NomeFantasia,
    string? InscricaoEstadual,
    decimal? LimiteCredito,
    string? Observacoes,
    List<CreateEnderecoRequest>? Enderecos
) : IRequest<Result<ClienteDto>>;

public class CreateClienteCommandValidator : AbstractValidator<CreateClienteCommand>
{
    public CreateClienteCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200).WithMessage("Nome é obrigatório.");
        RuleFor(x => x.CpfCnpj).NotEmpty().WithMessage("CPF/CNPJ é obrigatório.")
            .Must((cmd, cpfCnpj) =>
            {
                var digits = new string(cpfCnpj.Where(char.IsDigit).ToArray());
                return cmd.TipoPessoa == TipoPessoa.Fisica
                    ? CpfCnpjValidator.ValidarCpf(digits)
                    : CpfCnpjValidator.ValidarCnpj(digits);
            }).WithMessage("CPF/CNPJ inválido.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("E-mail inválido.");
    }
}

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, Result<ClienteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateClienteCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<ClienteDto>> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        var cpfCnpjLimpo = new string(request.CpfCnpj.Where(char.IsDigit).ToArray());
        var existe = await _context.Clientes.AnyAsync(c => c.CpfCnpj == cpfCnpjLimpo, cancellationToken);
        if (existe) return Result<ClienteDto>.Falha("Já existe um cliente com este CPF/CNPJ.");

        var cliente = new Cliente
        {
            Nome = request.Nome,
            TipoPessoa = request.TipoPessoa,
            CpfCnpj = cpfCnpjLimpo,
            Email = request.Email,
            Telefone = request.Telefone,
            Celular = request.Celular,
            NomeFantasia = request.NomeFantasia,
            InscricaoEstadual = request.InscricaoEstadual,
            LimiteCredito = request.LimiteCredito,
            Observacoes = request.Observacoes,
            EmpresaId = _empresaService.EmpresaId!.Value
        };

        if (request.Enderecos?.Any() == true)
        {
            foreach (var e in request.Enderecos)
                cliente.Enderecos.Add(new Endereco { Logradouro = e.Logradouro, Numero = e.Numero, Complemento = e.Complemento, Bairro = e.Bairro, Cidade = e.Cidade, Uf = e.Uf, Cep = e.Cep, Principal = e.Principal, Ativo = true });
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ClienteDto>.Ok(new ClienteDto { Id = cliente.Id, Nome = cliente.Nome, TipoPessoa = cliente.TipoPessoa, CpfCnpj = cliente.CpfCnpj, Email = cliente.Email, Ativo = cliente.Ativo });
    }
}
