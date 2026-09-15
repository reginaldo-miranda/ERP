using ERP.Application.Cadastros.UnidadesMedida.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities.Cadastros;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.UnidadesMedida.Commands;

public record CreateUnidadeMedidaCommand(string Sigla, string Descricao) : IRequest<Result<UnidadeMedidaDto>>;

public class CreateUnidadeMedidaCommandValidator : AbstractValidator<CreateUnidadeMedidaCommand>
{
    public CreateUnidadeMedidaCommandValidator()
    {
        RuleFor(x => x.Sigla).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(100);
    }
}

public class CreateUnidadeMedidaCommandHandler : IRequestHandler<CreateUnidadeMedidaCommand, Result<UnidadeMedidaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateUnidadeMedidaCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<UnidadeMedidaDto>> Handle(CreateUnidadeMedidaCommand request, CancellationToken cancellationToken)
    {
        var existe = await _context.UnidadesMedida.AnyAsync(u => u.Sigla == request.Sigla.ToUpper(), cancellationToken);
        if (existe) return Result<UnidadeMedidaDto>.Falha($"Unidade de medida '{request.Sigla}' já cadastrada.");

        var unidade = new UnidadeMedida
        {
            Sigla = request.Sigla.ToUpper(),
            Descricao = request.Descricao,
            EmpresaId = _empresaService.EmpresaId!.Value
        };

        _context.UnidadesMedida.Add(unidade);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<UnidadeMedidaDto>.Ok(new UnidadeMedidaDto { Id = unidade.Id, Sigla = unidade.Sigla, Descricao = unidade.Descricao, Ativo = unidade.Ativo });
    }
}
