using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ERP.Application.Cadastros.UnidadesMedida.Commands;

public record UpdateUnidadeMedidaCommand(int Id, string Sigla, string Descricao, bool Ativo) : IRequest<Result>;

public class UpdateUnidadeMedidaCommandValidator : AbstractValidator<UpdateUnidadeMedidaCommand>
{
    public UpdateUnidadeMedidaCommandValidator()
    {
        RuleFor(x => x.Sigla).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(100);
    }
}

public class UpdateUnidadeMedidaCommandHandler : IRequestHandler<UpdateUnidadeMedidaCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateUnidadeMedidaCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateUnidadeMedidaCommand request, CancellationToken cancellationToken)
    {
        var unidade = await _context.UnidadesMedida.FindAsync(new object[] { request.Id }, cancellationToken);
        if (unidade == null) return Result.Falha("Unidade de medida não encontrada.");

        unidade.Sigla = request.Sigla.ToUpper();
        unidade.Descricao = request.Descricao;
        unidade.Ativo = request.Ativo;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
