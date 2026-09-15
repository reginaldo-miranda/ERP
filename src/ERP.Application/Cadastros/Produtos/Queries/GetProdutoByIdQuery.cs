using ERP.Application.Cadastros.Produtos.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Cadastros.Produtos.Queries;

public record GetProdutoByIdQuery(int Id) : IRequest<Result<ProdutoDto>>;

public class GetProdutoByIdQueryHandler : IRequestHandler<GetProdutoByIdQuery, Result<ProdutoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProdutoByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProdutoDto>> Handle(GetProdutoByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.Produtos
            .Include(x => x.Categoria)
            .Include(x => x.UnidadeMedida)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (p == null)
            return Result<ProdutoDto>.Falha("Produto não encontrado.");

        return Result<ProdutoDto>.Ok(new ProdutoDto
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Nome = p.Nome,
            Descricao = p.Descricao,
            CategoriaId = p.CategoriaId,
            CategoriaNome = p.Categoria?.Nome,
            UnidadeMedidaId = p.UnidadeMedidaId,
            UnidadeMedidaSigla = p.UnidadeMedida?.Sigla,
            PrecoVenda = p.PrecoVenda,
            PrecoCusto = p.PrecoCusto,
            EstoqueMinimo = p.EstoqueMinimo,
            ControlaLote = p.ControlaLote,
            ControlaSerie = p.ControlaSerie,
            Ncm = p.Ncm,
            Cest = p.Cest,
            Cfop = p.Cfop,
            CodigoBarras = p.CodigoBarras,
            Ativo = p.Ativo
        });
    }
}
