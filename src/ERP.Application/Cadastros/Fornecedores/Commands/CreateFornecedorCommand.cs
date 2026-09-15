using ERP.Application.Cadastros.Clientes.Helpers;
using ERP.Application.Cadastros.Fornecedores.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities.Cadastros;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Fornecedores.Commands;

public record CreateFornecedorCommand(string RazaoSocial, string? NomeFantasia, string Cnpj, string? InscricaoEstadual, string? Email, string? Telefone, string? Celular, string? Contato, string? Site, string? Observacoes) : IRequest<Result<FornecedorDto>>;

public class CreateFornecedorCommandValidator : AbstractValidator<CreateFornecedorCommand>
{
    public CreateFornecedorCommandValidator()
    {
        RuleFor(x => x.RazaoSocial).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Cnpj).NotEmpty().Must(cnpj => CpfCnpjValidator.ValidarCnpj(cnpj)).WithMessage("CNPJ inválido.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CreateFornecedorCommandHandler : IRequestHandler<CreateFornecedorCommand, Result<FornecedorDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateFornecedorCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    { _context = context; _empresaService = empresaService; }

    public async Task<Result<FornecedorDto>> Handle(CreateFornecedorCommand request, CancellationToken cancellationToken)
    {
        var cnpjLimpo = new string(request.Cnpj.Where(char.IsDigit).ToArray());
        if (await _context.Fornecedores.AnyAsync(f => f.Cnpj == cnpjLimpo, cancellationToken))
            return Result<FornecedorDto>.Falha("Já existe um fornecedor com este CNPJ.");

        var fornecedor = new Fornecedor
        {
            RazaoSocial = request.RazaoSocial, NomeFantasia = request.NomeFantasia, Cnpj = cnpjLimpo,
            InscricaoEstadual = request.InscricaoEstadual, Email = request.Email, Telefone = request.Telefone,
            Celular = request.Celular, Contato = request.Contato, Site = request.Site,
            Observacoes = request.Observacoes, EmpresaId = _empresaService.EmpresaId!.Value
        };
        _context.Fornecedores.Add(fornecedor);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<FornecedorDto>.Ok(new FornecedorDto { Id = fornecedor.Id, RazaoSocial = fornecedor.RazaoSocial, Cnpj = fornecedor.Cnpj, Ativo = fornecedor.Ativo });
    }
}
