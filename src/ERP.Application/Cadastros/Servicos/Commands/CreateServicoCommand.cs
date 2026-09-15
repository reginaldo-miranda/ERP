using ERP.Application.Cadastros.Servicos.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities.Cadastros;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Servicos.Commands;

public record CreateServicoCommand(string Codigo, string Nome, string? Descricao, int? CategoriaId, decimal PrecoBase, string? Unidade, string? CodigoServicoCnae, string? CodigoServiceLc116, decimal? AliquotaIss, string? Observacoes) : IRequest<Result<ServicoDto>>;

public class CreateServicoCommandValidator : AbstractValidator<CreateServicoCommand>
{
    public CreateServicoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PrecoBase).GreaterThanOrEqualTo(0);
    }
}

public class CreateServicoCommandHandler : IRequestHandler<CreateServicoCommand, Result<ServicoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateServicoCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    { _context = context; _empresaService = empresaService; }

    public async Task<Result<ServicoDto>> Handle(CreateServicoCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Servicos.AnyAsync(s => s.Codigo == request.Codigo, cancellationToken))
            return Result<ServicoDto>.Falha($"Já existe um serviço com o código '{request.Codigo}'.");

        var servico = new Servico
        {
            Codigo = request.Codigo, Nome = request.Nome, Descricao = request.Descricao,
            CategoriaId = request.CategoriaId, PrecoBase = request.PrecoBase, Unidade = request.Unidade,
            CodigoServicoCnae = request.CodigoServicoCnae, CodigoServiceLc116 = request.CodigoServiceLc116,
            AliquotaIss = request.AliquotaIss, Observacoes = request.Observacoes,
            EmpresaId = _empresaService.EmpresaId!.Value
        };
        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<ServicoDto>.Ok(new ServicoDto { Id = servico.Id, Codigo = servico.Codigo, Nome = servico.Nome, PrecoBase = servico.PrecoBase, Ativo = servico.Ativo });
    }
}
