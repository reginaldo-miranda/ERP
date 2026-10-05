using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Compras.Queries;

public record GetHistoricoComprasQuery(
    int? FornecedorId,
    int? ProdutoId,
    DateTime? DataInicio,
    DateTime? DataFim,
    int Pagina = 1,
    int TamanhoPagina = 50
) : IRequest<PaginatedList<HistoricoComprasItemDto>>;

public class GetHistoricoComprasQueryHandler : IRequestHandler<GetHistoricoComprasQuery, PaginatedList<HistoricoComprasItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHistoricoComprasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<HistoricoComprasItemDto>> Handle(GetHistoricoComprasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.EntradaMercadoriaItens
            .AsNoTracking()
            .Include(i => i.EntradaMercadoria)
                .ThenInclude(e => e.Fornecedor)
            .Include(i => i.EntradaMercadoria)
                .ThenInclude(e => e.Deposito)
            .Include(i => i.Produto)
                .ThenInclude(p => p.UnidadeMedida)
            .Where(i => i.EntradaMercadoria.Status == StatusEntradaMercadoria.Conferida)
            .AsQueryable();

        if (request.FornecedorId.HasValue)
            query = query.Where(i => i.EntradaMercadoria.FornecedorId == request.FornecedorId.Value);

        if (request.ProdutoId.HasValue)
            query = query.Where(i => i.ProdutoId == request.ProdutoId.Value);

        if (request.DataInicio.HasValue)
            query = query.Where(i => i.EntradaMercadoria.DataEntrada >= request.DataInicio.Value);

        if (request.DataFim.HasValue)
            query = query.Where(i => i.EntradaMercadoria.DataEntrada <= request.DataFim.Value);

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(i => i.EntradaMercadoria.DataEntrada)
            .ThenByDescending(i => i.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(i => new HistoricoComprasItemDto(
                i.EntradaMercadoria.Id,
                i.EntradaMercadoria.Numero,
                i.EntradaMercadoria.NumeroNotaFiscal,
                i.EntradaMercadoria.DataEntrada,
                i.EntradaMercadoria.Fornecedor.NomeFantasia ?? i.EntradaMercadoria.Fornecedor.RazaoSocial,
                i.Produto.Codigo,
                i.Produto.Nome,
                i.Produto.UnidadeMedida != null ? i.Produto.UnidadeMedida.Sigla : "UN",
                i.Quantidade,
                i.CustoUnitario,
                i.CustoTotal,
                i.EntradaMercadoria.Deposito.Nome
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedList<HistoricoComprasItemDto>(itens, total, request.Pagina, request.TamanhoPagina);
    }
}

public record GetComparativoPrecosFornecedoresQuery(int ProdutoId) : IRequest<List<ComparativoPrecoFornecedorDto>>;

public class GetComparativoPrecosFornecedoresQueryHandler : IRequestHandler<GetComparativoPrecosFornecedoresQuery, List<ComparativoPrecoFornecedorDto>>
{
    private readonly IApplicationDbContext _context;

    public GetComparativoPrecosFornecedoresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ComparativoPrecoFornecedorDto>> Handle(GetComparativoPrecosFornecedoresQuery request, CancellationToken cancellationToken)
    {
        var entradasItens = await _context.EntradaMercadoriaItens
            .AsNoTracking()
            .Include(i => i.EntradaMercadoria)
                .ThenInclude(e => e.Fornecedor)
            .Where(i => i.ProdutoId == request.ProdutoId && i.EntradaMercadoria.Status == StatusEntradaMercadoria.Conferida)
            .ToListAsync(cancellationToken);

        var agrupado = entradasItens
            .GroupBy(i => new { i.EntradaMercadoria.FornecedorId, Nome = i.EntradaMercadoria.Fornecedor.NomeFantasia ?? i.EntradaMercadoria.Fornecedor.RazaoSocial })
            .Select(g =>
            {
                var ultima = g.OrderByDescending(x => x.EntradaMercadoria.DataEntrada).First();
                return new ComparativoPrecoFornecedorDto(
                    g.Key.FornecedorId,
                    g.Key.Nome,
                    ultima.EntradaMercadoria.DataEntrada,
                    ultima.EntradaMercadoria.NumeroNotaFiscal,
                    g.Min(x => x.CustoUnitario),
                    g.Max(x => x.CustoUnitario),
                    ultima.CustoUnitario,
                    g.Sum(x => x.Quantidade)
                );
            })
            .OrderBy(c => c.UltimoPreco)
            .ToList();

        return agrupado;
    }
}

public record GetResumoComprasQuery(DateTime? DataInicio, DateTime? DataFim) : IRequest<ResumoComprasDto>;

public class GetResumoComprasQueryHandler : IRequestHandler<GetResumoComprasQuery, ResumoComprasDto>
{
    private readonly IApplicationDbContext _context;

    public GetResumoComprasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResumoComprasDto> Handle(GetResumoComprasQuery request, CancellationToken cancellationToken)
    {
        var pedidosQuery = _context.PedidosCompra.AsNoTracking();
        var entradasQuery = _context.EntradasMercadoria.AsNoTracking().Where(e => e.Status == StatusEntradaMercadoria.Conferida);

        if (request.DataInicio.HasValue)
        {
            pedidosQuery = pedidosQuery.Where(p => p.DataPedido >= request.DataInicio.Value);
            entradasQuery = entradasQuery.Where(e => e.DataEntrada >= request.DataInicio.Value);
        }

        if (request.DataFim.HasValue)
        {
            pedidosQuery = pedidosQuery.Where(p => p.DataPedido <= request.DataFim.Value);
            entradasQuery = entradasQuery.Where(e => e.DataEntrada <= request.DataFim.Value);
        }

        var pedidos = await pedidosQuery.ToListAsync(cancellationToken);
        var totalEntradas = await entradasQuery.CountAsync(cancellationToken);
        var valorTotalComprado = await entradasQuery.SumAsync(e => e.ValorTotal, cancellationToken);

        var totalPedidos = pedidos.Count;
        var pendentes = pedidos.Count(p => p.Status == StatusPedidoCompra.Pendente || p.Status == StatusPedidoCompra.Aprovado);
        var recebidos = pedidos.Count(p => p.Status == StatusPedidoCompra.Recebido || p.Status == StatusPedidoCompra.ParcialmenteRecebido);
        var ticketMedio = totalPedidos > 0 ? (pedidos.Sum(p => p.ValorTotal) / totalPedidos) : 0m;

        return new ResumoComprasDto(
            totalPedidos,
            pendentes,
            recebidos,
            valorTotalComprado,
            totalEntradas,
            ticketMedio
        );
    }
}
