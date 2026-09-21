using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Empresas.Commands.DesativarEmpresa;

public record DesativarEmpresaCommand(int Id) : IRequest<Result>;

public class DesativarEmpresaCommandHandler : IRequestHandler<DesativarEmpresaCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DesativarEmpresaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DesativarEmpresaCommand request, CancellationToken cancellationToken)
    {
        var empresa = await _context.Empresas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (empresa == null)
            return Result.Falha("Empresa não encontrada.");

        // Alterna status ou desativa
        empresa.Ativo = !empresa.Ativo;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
