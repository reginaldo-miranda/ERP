using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Empresas.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Empresas.Commands.AtualizarEmpresa;

public record AtualizarEmpresaCommand(
    int Id,
    string RazaoSocial,
    string Cnpj,
    string? NomeFantasia,
    string? InscricaoEstadual,
    string? InscricaoMunicipal,
    string? Email,
    string? Telefone,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Cep,
    string? RegimeTributario,
    string? MetodoCusteio) : IRequest<Result<EmpresaDto>>;

public class AtualizarEmpresaCommandValidator : AbstractValidator<AtualizarEmpresaCommand>
{
    public AtualizarEmpresaCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.RazaoSocial).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Cnpj).NotEmpty().Length(14);
    }
}

public class AtualizarEmpresaCommandHandler : IRequestHandler<AtualizarEmpresaCommand, Result<EmpresaDto>>
{
    private readonly IApplicationDbContext _context;

    public AtualizarEmpresaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EmpresaDto>> Handle(AtualizarEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _context.Empresas.FindAsync(new object[] { request.Id }, cancellationToken);

        if (empresa == null)
            throw new NotFoundException(nameof(Domain.Core.Entities.Empresa), request.Id);

        empresa.RazaoSocial = request.RazaoSocial;
        empresa.Cnpj = request.Cnpj;
        empresa.NomeFantasia = request.NomeFantasia;
        empresa.InscricaoEstadual = request.InscricaoEstadual;
        empresa.InscricaoMunicipal = request.InscricaoMunicipal;
        empresa.Email = request.Email;
        empresa.Telefone = request.Telefone;
        empresa.Logradouro = request.Logradouro;
        empresa.Numero = request.Numero;
        empresa.Complemento = request.Complemento;
        empresa.Bairro = request.Bairro;
        empresa.Cidade = request.Cidade;
        empresa.Uf = request.Uf;
        empresa.Cep = request.Cep;
        empresa.RegimeTributario = request.RegimeTributario;
        empresa.MetodoCusteio = request.MetodoCusteio;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<EmpresaDto>.Ok(new EmpresaDto
        {
            Id = empresa.Id,
            RazaoSocial = empresa.RazaoSocial,
            NomeFantasia = empresa.NomeFantasia,
            Cnpj = empresa.Cnpj,
            Email = empresa.Email,
            Ativo = empresa.Ativo,
            CriadoEm = empresa.CriadoEm
        });
    }
}
