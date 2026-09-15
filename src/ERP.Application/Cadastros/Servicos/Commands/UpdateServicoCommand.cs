using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ERP.Application.Cadastros.Servicos.Commands;

public record UpdateServicoCommand(int Id, string Codigo, string Nome, string? Descricao, int? CategoriaId, decimal PrecoBase, string? Unidade, string? CodigoServicoCnae, string? CodigoServiceLc116, decimal? AliquotaIss, string? Observacoes, bool Ativo) : IRequest<Result>;

public class UpdateServicoCommandValidator : AbstractValidator<UpdateServicoCommand>
{
    public UpdateServicoCommandValidator() { RuleFor(x => x.Nome).NotEmpty().MaximumLength(200); }
}

public class UpdateServicoCommandHandler : IRequestHandler<UpdateServicoCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateServicoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateServicoCommand request, CancellationToken cancellationToken)
    {
        var s = await _context.Servicos.FindAsync(new object[] { request.Id }, cancellationToken);
        if (s == null) return Result.Falha("Serviço não encontrado.");
        s.Codigo = request.Codigo; s.Nome = request.Nome; s.Descricao = request.Descricao;
        s.CategoriaId = request.CategoriaId; s.PrecoBase = request.PrecoBase; s.Unidade = request.Unidade;
        s.CodigoServicoCnae = request.CodigoServicoCnae; s.CodigoServiceLc116 = request.CodigoServiceLc116;
        s.AliquotaIss = request.AliquotaIss; s.Observacoes = request.Observacoes; s.Ativo = request.Ativo;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
