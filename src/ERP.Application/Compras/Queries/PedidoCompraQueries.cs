using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Compras.Queries;

public record GetPedidosCompraQuery(
    string? Busca,
    int? FornecedorId,
    StatusPedidoCompra? Status,
    DateTime? DataInicio,
    DateTime? DataFim,
    int Pagina = 1,
    int TamanhoPagina = 50
) : IRequest<PaginatedList<PedidoCompraListItemDto>>;

public class GetPedidosCompraQueryHandler : IRequestHandler<GetPedidosCompraQuery, PaginatedList<PedidoCompraListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPedidosCompraQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<PedidoCompraListItemDto>> Handle(GetPedidosCompraQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PedidosCompra
            .AsNoTracking()
            .Include(p => p.Fornecedor)
            .Include(p => p.SolicitacaoCompra)
            .Include(p => p.DepositoDestino)
            .Include(p => p.Itens)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(p => p.Numero.ToLower().Contains(termo) ||
                                     p.Fornecedor.RazaoSocial.ToLower().Contains(termo) ||
                                     (p.Fornecedor.NomeFantasia != null && p.Fornecedor.NomeFantasia.ToLower().Contains(termo)));
        }

        if (request.FornecedorId.HasValue)
            query = query.Where(p => p.FornecedorId == request.FornecedorId.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.DataInicio.HasValue)
            query = query.Where(p => p.DataPedido >= request.DataInicio.Value);

        if (request.DataFim.HasValue)
            query = query.Where(p => p.DataPedido <= request.DataFim.Value);

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(p => p.DataPedido)
            .ThenByDescending(p => p.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(p => new PedidoCompraListItemDto(
                p.Id,
                p.Numero,
                p.FornecedorId,
                p.Fornecedor.NomeFantasia ?? p.Fornecedor.RazaoSocial,
                p.Fornecedor.Cnpj,
                p.SolicitacaoCompraId,
                p.SolicitacaoCompra != null ? p.SolicitacaoCompra.Numero : null,
                p.DataPedido,
                p.PrevisaoEntrega,
                p.Status,
                p.DepositoDestinoId,
                p.DepositoDestino.Nome,
                p.Itens.Count,
                p.ValorTotal,
                p.NumeroParcelas,
                p.CompradorNome
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<PedidoCompraListItemDto>(itens, total, request.Pagina, request.TamanhoPagina);
    }
}

public record GetPedidoCompraByIdQuery(int Id) : IRequest<PedidoCompraDetalheDto?>;

public class GetPedidoCompraByIdQueryHandler : IRequestHandler<GetPedidoCompraByIdQuery, PedidoCompraDetalheDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPedidoCompraByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PedidoCompraDetalheDto?> Handle(GetPedidoCompraByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.PedidosCompra
            .AsNoTracking()
            .Include(x => x.Fornecedor)
            .Include(x => x.SolicitacaoCompra)
            .Include(x => x.DepositoDestino)
            .Include(x => x.FormaPagamento)
            .Include(x => x.Itens)
                .ThenInclude(i => i.Produto)
                    .ThenInclude(prod => prod.UnidadeMedida)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (p == null) return null;

        return new PedidoCompraDetalheDto(
            p.Id,
            p.Numero,
            p.FornecedorId,
            p.Fornecedor.NomeFantasia ?? p.Fornecedor.RazaoSocial,
            p.Fornecedor.Cnpj,
            p.SolicitacaoCompraId,
            p.SolicitacaoCompra?.Numero,
            p.DataPedido,
            p.PrevisaoEntrega,
            p.Status,
            p.CompradorNome,
            p.DepositoDestinoId,
            p.DepositoDestino.Nome,
            p.FormaPagamentoId,
            p.FormaPagamento?.Nome,
            p.NumeroParcelas,
            p.IntervaloDias,
            p.DiasPrimeiroVencimento,
            p.ValorBruto,
            p.ValorDesconto,
            p.ValorFrete,
            p.ValorSeguro,
            p.ValorOutrasDespesas,
            p.ValorTotal,
            p.Observacoes,
            p.DataAprovacao,
            p.DataCancelamento,
            p.MotivoCancelamento,
            p.Itens.Select(i => new PedidoCompraItemDto(
                i.Id,
                i.ProdutoId,
                i.Produto.Nome,
                i.Produto.Codigo,
                i.Produto.UnidadeMedida?.Sigla ?? "UN",
                i.Quantidade,
                i.QuantidadeEntregue,
                i.PrecoUnitario,
                i.ValorDesconto,
                i.SubTotal,
                i.Observacao
            )).ToList()
        );
    }
}
