using ERP.Application.Cadastros.Clientes.Helpers;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Enums;
using FluentValidation;
using MediatR;

namespace ERP.Application.Cadastros.Clientes.Commands;

public record UpdateClienteCommand(
    int Id,
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
    bool Ativo
) : IRequest<Result>;

public class UpdateClienteCommandValidator : AbstractValidator<UpdateClienteCommand>
{
    public UpdateClienteCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CpfCnpj).NotEmpty()
            .Must((cmd, cpfCnpj) =>
            {
                var digits = new string(cpfCnpj.Where(char.IsDigit).ToArray());
                return cmd.TipoPessoa == TipoPessoa.Fisica
                    ? CpfCnpjValidator.ValidarCpf(digits)
                    : CpfCnpjValidator.ValidarCnpj(digits);
            }).WithMessage("CPF/CNPJ inválido.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateClienteCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clientes.FindAsync(new object[] { request.Id }, cancellationToken);
        if (cliente == null) return Result.Falha("Cliente não encontrado.");

        cliente.Nome = request.Nome;
        cliente.TipoPessoa = request.TipoPessoa;
        cliente.CpfCnpj = new string(request.CpfCnpj.Where(char.IsDigit).ToArray());
        cliente.Email = request.Email;
        cliente.Telefone = request.Telefone;
        cliente.Celular = request.Celular;
        cliente.NomeFantasia = request.NomeFantasia;
        cliente.InscricaoEstadual = request.InscricaoEstadual;
        cliente.LimiteCredito = request.LimiteCredito;
        cliente.Observacoes = request.Observacoes;
        cliente.Ativo = request.Ativo;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
