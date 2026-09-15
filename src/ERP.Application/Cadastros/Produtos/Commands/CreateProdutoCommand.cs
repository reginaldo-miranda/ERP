using ERP.Application.Cadastros.Produtos.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities.Cadastros;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Produtos.Commands;

public record CreateProdutoCommand(string Codigo, string Nome, string? Descricao, int? CategoriaId, int? UnidadeMedidaId, decimal PrecoVenda, decimal PrecoCusto, decimal EstoqueMinimo, bool ControlaLote, bool ControlaSerie, string? Ncm, string? Cest, string? Cfop, string? CodigoBarras) : IRequest<Result<ProdutoDto>>;

public class CreateProdutoCommandValidator : AbstractValidator<CreateProdutoCommand>
{
    public CreateProdutoCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PrecoVenda).GreaterThanOrEqualTo(0);
    }
}

public class CreateProdutoCommandHandler : IRequestHandler<CreateProdutoCommand, Result<ProdutoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateProdutoCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    { _context = context; _empresaService = empresaService; }

    public async Task<Result<ProdutoDto>> Handle(CreateProdutoCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Produtos.AnyAsync(p => p.Codigo == request.Codigo, cancellationToken))
            return Result<ProdutoDto>.Falha($"Já existe um produto com o código '{request.Codigo}'.");

        var produto = new Produto
        {
            Codigo = request.Codigo, Nome = request.Nome, Descricao = request.Descricao,
            CategoriaId = request.CategoriaId, UnidadeMedidaId = request.UnidadeMedidaId,
            PrecoVenda = request.PrecoVenda, PrecoCusto = request.PrecoCusto, EstoqueMinimo = request.EstoqueMinimo,
            ControlaLote = request.ControlaLote, ControlaSerie = request.ControlaSerie,
            Ncm = request.Ncm, Cest = request.Cest, Cfop = request.Cfop, CodigoBarras = request.CodigoBarras,
            EmpresaId = _empresaService.EmpresaId!.Value
        };
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<ProdutoDto>.Ok(new ProdutoDto { Id = produto.Id, Codigo = produto.Codigo, Nome = produto.Nome, PrecoVenda = produto.PrecoVenda, Ativo = produto.Ativo });
    }
}
