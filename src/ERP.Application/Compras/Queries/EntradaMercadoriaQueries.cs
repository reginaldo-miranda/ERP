using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Compras.Queries;

public record GetEntradasMercadoriaQuery(
    string? Busca,
    int? FornecedorId,
    int? DepositoId,
    StatusEntradaMercadoria? Status,
    DateTime? DataInicio,
    DateTime? DataFim,
    int Pagina = 1,
    int TamanhoPagina = 50
) : IRequest<PaginatedList<EntradaMercadoriaListItemDto>>;

public class GetEntradasMercadoriaQueryHandler : IRequestHandler<GetEntradasMercadoriaQuery, PaginatedList<EntradaMercadoriaListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEntradasMercadoriaQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<EntradaMercadoriaListItemDto>> Handle(GetEntradasMercadoriaQuery request, CancellationToken cancellationToken)
    {
        var query = _context.EntradasMercadoria
            .AsNoTracking()
            .Include(e => e.Fornecedor)
            .Include(e => e.Deposito)
            .Include(e => e.PedidoCompra)
            .Include(e => e.Itens)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(e => e.Numero.ToLower().Contains(termo) ||
                                     e.NumeroNotaFiscal.ToLower().Contains(termo) ||
                                     e.Fornecedor.RazaoSocial.ToLower().Contains(termo) ||
                                     (e.Fornecedor.NomeFantasia != null && e.Fornecedor.NomeFantasia.ToLower().Contains(termo)));
        }

        if (request.FornecedorId.HasValue)
            query = query.Where(e => e.FornecedorId == request.FornecedorId.Value);

        if (request.DepositoId.HasValue)
            query = query.Where(e => e.DepositoId == request.DepositoId.Value);

        if (request.Status.HasValue)
            query = query.Where(e => e.Status == request.Status.Value);

        if (request.DataInicio.HasValue)
            query = query.Where(e => e.DataEntrada >= request.DataInicio.Value);

        if (request.DataFim.HasValue)
            query = query.Where(e => e.DataEntrada <= request.DataFim.Value);

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(e => e.DataEntrada)
            .ThenByDescending(e => e.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(e => new EntradaMercadoriaListItemDto(
                e.Id,
                e.Numero,
                e.PedidoCompraId,
                e.PedidoCompra != null ? e.PedidoCompra.Numero : null,
                e.FornecedorId,
                e.Fornecedor.NomeFantasia ?? e.Fornecedor.RazaoSocial,
                e.DepositoId,
                e.Deposito.Nome,
                e.NumeroNotaFiscal,
                e.SerieNotaFiscal,
                e.DataEmissao,
                e.DataEntrada,
                e.Status,
                e.Itens.Count,
                e.ValorTotal,
                e.ConferenteNome
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<EntradaMercadoriaListItemDto>(itens, total, request.Pagina, request.TamanhoPagina);
    }
}

public record GetEntradaMercadoriaByIdQuery(int Id) : IRequest<EntradaMercadoriaDetalheDto?>;

public class GetEntradaMercadoriaByIdQueryHandler : IRequestHandler<GetEntradaMercadoriaByIdQuery, EntradaMercadoriaDetalheDto?>
{
    private readonly IApplicationDbContext _context;

    public GetEntradaMercadoriaByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EntradaMercadoriaDetalheDto?> Handle(GetEntradaMercadoriaByIdQuery request, CancellationToken cancellationToken)
    {
        var e = await _context.EntradasMercadoria
            .AsNoTracking()
            .Include(x => x.Fornecedor)
            .Include(x => x.Deposito)
            .Include(x => x.PedidoCompra)
            .Include(x => x.Itens)
                .ThenInclude(i => i.Produto)
                    .ThenInclude(p => p.UnidadeMedida)
            .Include(x => x.Itens)
                .ThenInclude(i => i.PedidoCompraItem)
            .Include(x => x.Itens)
                .ThenInclude(i => i.Lote)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (e == null) return null;

        return new EntradaMercadoriaDetalheDto(
            e.Id,
            e.Numero,
            e.PedidoCompraId,
            e.PedidoCompra?.Numero,
            e.FornecedorId,
            e.Fornecedor.NomeFantasia ?? e.Fornecedor.RazaoSocial,
            e.Fornecedor.Cnpj,
            e.DepositoId,
            e.Deposito.Nome,
            e.NumeroNotaFiscal,
            e.SerieNotaFiscal,
            e.ChaveAcessoNfe,
            e.DataEmissao,
            e.DataEntrada,
            e.Status,
            e.ConferenteNome,
            e.ValorProdutos,
            e.ValorFrete,
            e.ValorSeguro,
            e.ValorOutrasDespesas,
            e.ValorDesconto,
            e.ValorTotal,
            e.IdParcelamento,
            e.Observacoes,
            e.Itens.Select(i => new EntradaMercadoriaItemDto(
                i.Id,
                i.PedidoCompraItemId,
                i.ProdutoId,
                i.Produto.Nome,
                i.Produto.Codigo,
                i.Produto.UnidadeMedida?.Sigla ?? "UN",
                i.Produto.ControlaLote,
                i.Produto.ControlaSerie,
                i.PedidoCompraItem != null ? i.PedidoCompraItem.Quantidade : 0,
                i.Quantidade,
                i.CustoUnitario,
                i.CustoTotal,
                i.LoteId,
                i.CodigoLote,
                i.DataValidadeLote,
                i.DataFabricacaoLote,
                i.NumerosSerieTexto,
                i.Observacao
            )).ToList()
        );
    }
}
