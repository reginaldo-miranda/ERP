using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Vendas.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Vendas.Queries;

public record GetHistoricoVendasQuery(
    string? Busca = null,
    int? ClienteId = null,
    string? VendedorId = null,
    StatusPedidoVenda? Status = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    int Pagina = 1,
    int TamanhoPagina = 20
) : IRequest<PaginatedList<HistoricoVendasItemDto>>;

public record GetResumoVendasQuery(
    DateTime? DataInicio = null,
    DateTime? DataFim = null
) : IRequest<ResumoVendasDto>;

public class HistoricoVendasQueriesHandler :
    IRequestHandler<GetHistoricoVendasQuery, PaginatedList<HistoricoVendasItemDto>>,
    IRequestHandler<GetResumoVendasQuery, ResumoVendasDto>
{
    private readonly IApplicationDbContext _context;

    public HistoricoVendasQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<HistoricoVendasItemDto>> Handle(GetHistoricoVendasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PedidosVenda.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.FormaPagamento)
            .Include(p => p.Faturamentos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = request.Busca.Trim().ToLower();
            query = query.Where(p =>
                p.Numero.ToLower().Contains(busca) ||
                p.Cliente.Nome.ToLower().Contains(busca) ||
                (p.Cliente.CpfCnpj != null && p.Cliente.CpfCnpj.Contains(busca)) ||
                (p.VendedorNome != null && p.VendedorNome.ToLower().Contains(busca))
            );
        }

        if (request.ClienteId.HasValue)
            query = query.Where(p => p.ClienteId == request.ClienteId.Value);

        if (!string.IsNullOrWhiteSpace(request.VendedorId))
            query = query.Where(p => p.VendedorId == request.VendedorId);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.DataInicio.HasValue)
        {
            var inicioUtc = request.DataInicio.Value.Date.ToUniversalTime();
            query = query.Where(p => p.DataPedido >= inicioUtc);
        }

        if (request.DataFim.HasValue)
        {
            var fimUtc = request.DataFim.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(p => p.DataPedido <= fimUtc);
        }

        int totalItens = await query.CountAsync(cancellationToken);

        var pedidosDb = await query
            .OrderByDescending(p => p.DataPedido)
            .ThenByDescending(p => p.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .ToListAsync(cancellationToken);

        var dtos = pedidosDb.Select(p =>
        {
            var faturamento = p.Faturamentos.FirstOrDefault(f => f.Status == StatusFaturamento.Emitido);
            decimal custoTotal = faturamento?.CustoTotal ?? 0m;
            decimal valorTotal = p.ValorTotal;
            decimal lucroBruto = valorTotal - custoTotal;
            decimal margem = valorTotal > 0 ? Math.Round((lucroBruto / valorTotal) * 100m, 2) : 0m;

            return new HistoricoVendasItemDto(
                p.Id,
                p.Numero,
                p.DataPedido,
                p.ClienteId,
                p.Cliente.Nome,
                p.VendedorNome,
                p.Status,
                faturamento?.Numero,
                faturamento?.DataFaturamento,
                valorTotal,
                custoTotal,
                lucroBruto,
                margem,
                p.FormaPagamento?.Nome ?? "A Combinar"
            );
        }).ToList();

        return new PaginatedList<HistoricoVendasItemDto>(dtos, totalItens, request.Pagina, request.TamanhoPagina);
    }

    public async Task<ResumoVendasDto> Handle(GetResumoVendasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PedidosVenda.AsNoTracking()
            .Include(p => p.Faturamentos)
            .AsQueryable();

        if (request.DataInicio.HasValue)
        {
            var inicioUtc = request.DataInicio.Value.Date.ToUniversalTime();
            query = query.Where(p => p.DataPedido >= inicioUtc);
        }

        if (request.DataFim.HasValue)
        {
            var fimUtc = request.DataFim.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(p => p.DataPedido <= fimUtc);
        }

        var pedidos = await query.ToListAsync(cancellationToken);

        int totalPedidos = pedidos.Count;
        int pendentes = pedidos.Count(p => p.Status == StatusPedidoVenda.Pendente || p.Status == StatusPedidoVenda.Aprovado);
        int faturados = pedidos.Count(p => p.Status == StatusPedidoVenda.Faturado);
        int cancelados = pedidos.Count(p => p.Status == StatusPedidoVenda.Cancelado);

        var pedidosFaturados = pedidos.Where(p => p.Status == StatusPedidoVenda.Faturado).ToList();
        decimal totalVendido = pedidosFaturados.Sum(p => p.ValorTotal);
        decimal custoTotalVendido = pedidosFaturados.Sum(p =>
        {
            var fat = p.Faturamentos.FirstOrDefault(f => f.Status == StatusFaturamento.Emitido);
            return fat?.CustoTotal ?? 0m;
        });

        decimal lucroBruto = totalVendido - custoTotalVendido;
        decimal margemGeral = totalVendido > 0 ? Math.Round((lucroBruto / totalVendido) * 100m, 2) : 0m;
        decimal ticketMedio = faturados > 0 ? Math.Round(totalVendido / faturados, 2) : 0m;

        return new ResumoVendasDto(
            totalVendido,
            custoTotalVendido,
            lucroBruto,
            margemGeral,
            totalPedidos,
            faturados,
            pendentes,
            cancelados,
            ticketMedio
        );
    }
}
