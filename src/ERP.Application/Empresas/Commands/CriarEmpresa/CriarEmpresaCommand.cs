using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Empresas.DTOs;
using ERP.Domain.Core.Entities;
using FluentValidation;
using MediatR;

namespace ERP.Application.Empresas.Commands.CriarEmpresa;

public record CriarEmpresaCommand(
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

public class CriarEmpresaCommandValidator : AbstractValidator<CriarEmpresaCommand>
{
    public CriarEmpresaCommandValidator()
    {
        RuleFor(x => x.RazaoSocial)
            .NotEmpty().WithMessage("A razão social é obrigatória.")
            .MaximumLength(300).WithMessage("A razão social deve ter no máximo 300 caracteres.");

        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage("O CNPJ é obrigatório.")
            .Length(14).WithMessage("O CNPJ deve ter 14 dígitos (sem formatação).");
    }
}

public class CriarEmpresaCommandHandler : IRequestHandler<CriarEmpresaCommand, Result<EmpresaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CriarEmpresaCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<EmpresaDto>> Handle(CriarEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = new Empresa
        {
            RazaoSocial = request.RazaoSocial,
            Cnpj = request.Cnpj,
            NomeFantasia = request.NomeFantasia,
            InscricaoEstadual = request.InscricaoEstadual,
            InscricaoMunicipal = request.InscricaoMunicipal,
            Email = request.Email,
            Telefone = request.Telefone,
            Logradouro = request.Logradouro,
            Numero = request.Numero,
            Complemento = request.Complemento,
            Bairro = request.Bairro,
            Cidade = request.Cidade,
            Uf = request.Uf,
            Cep = request.Cep,
            RegimeTributario = request.RegimeTributario,
            MetodoCusteio = request.MetodoCusteio
        };

        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync(cancellationToken);

        // Vincular o usuário criador à empresa
        if (_currentUser.UsuarioId != null)
        {
            _context.UsuarioEmpresas.Add(new UsuarioEmpresa
            {
                UsuarioId = _currentUser.UsuarioId,
                EmpresaId = empresa.Id
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result<EmpresaDto>.Ok(new EmpresaDto
        {
            Id = empresa.Id,
            RazaoSocial = empresa.RazaoSocial,
            NomeFantasia = empresa.NomeFantasia,
            Cnpj = empresa.Cnpj,
            Email = empresa.Email,
            Telefone = empresa.Telefone,
            Ativo = empresa.Ativo,
            CriadoEm = empresa.CriadoEm
        });
    }
}
