using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ERP.Application.Cadastros.Produtos.Commands;

public record UpdateProdutoCommand(int Id, string Codigo, string Nome, string? Descricao, int? CategoriaId, int? UnidadeMedidaId, decimal PrecoVenda, decimal PrecoCusto, decimal EstoqueMinimo, bool ControlaLote, bool ControlaSerie, string? Ncm, string? Cest, string? Cfop, string? CodigoBarras, bool Ativo) : IRequest<Result>;

public class UpdateProdutoCommandValidator : AbstractValidator<UpdateProdutoCommand>
{
    public UpdateProdutoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
    }
}

public class UpdateProdutoCommandHandler : IRequestHandler<UpdateProdutoCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateProdutoCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateProdutoCommand request, CancellationToken cancellationToken)
    {
        var p = await _context.Produtos.FindAsync(new object[] { request.Id }, cancellationToken);
        if (p == null) return Result.Falha("Produto não encontrado.");
        p.Codigo = request.Codigo; p.Nome = request.Nome; p.Descricao = request.Descricao;
        p.CategoriaId = request.CategoriaId; p.UnidadeMedidaId = request.UnidadeMedidaId;
        p.PrecoVenda = request.PrecoVenda; p.PrecoCusto = request.PrecoCusto; p.EstoqueMinimo = request.EstoqueMinimo;
        p.ControlaLote = request.ControlaLote; p.ControlaSerie = request.ControlaSerie;
        p.Ncm = request.Ncm; p.Cest = request.Cest; p.Cfop = request.Cfop; p.CodigoBarras = request.CodigoBarras;
        p.Ativo = request.Ativo;
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
