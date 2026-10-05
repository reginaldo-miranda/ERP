using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Vendas.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Vendas.Queries;

public record GetPedidosVendaQuery(
    string? Busca = null,
    int? ClienteId = null,
    int? DepositoId = null,
    StatusPedidoVenda? Status = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    int Pagina = 1,
    int TamanhoPagina = 20
) : IRequest<PaginatedList<PedidoVendaListItemDto>>;

public record GetPedidoVendaByIdQuery(int Id) : IRequest<Result<PedidoVendaDetalheDto>>;

public class PedidoVendaQueriesHandler :
    IRequestHandler<GetPedidosVendaQuery, PaginatedList<PedidoVendaListItemDto>>,
    IRequestHandler<GetPedidoVendaByIdQuery, Result<PedidoVendaDetalheDto>>
{
    private readonly IApplicationDbContext _context;

    public PedidoVendaQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<PedidoVendaListItemDto>> Handle(GetPedidosVendaQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PedidosVenda.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Deposito)
            .Include(p => p.FormaPagamento)
            .Include(p => p.Itens)
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

        if (request.DepositoId.HasValue)
            query = query.Where(p => p.DepositoId == request.DepositoId.Value);

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

        var itensDb = await query
            .OrderByDescending(p => p.DataPedido)
            .ThenByDescending(p => p.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .ToListAsync(cancellationToken);

        var dtos = itensDb.Select(p =>
        {
            var faturamentoAtivo = p.Faturamentos.FirstOrDefault(f => f.Status == StatusFaturamento.Emitido);

            return new PedidoVendaListItemDto(
                p.Id,
                p.Numero,
                p.ClienteId,
                p.Cliente.Nome,
                p.Cliente.CpfCnpj,
                p.VendedorNome,
                p.DataPedido,
                p.Status,
                p.Deposito?.Nome,
                p.FormaPagamento?.Nome,
                p.NumeroParcelas,
                p.ValorBruto,
                p.ValorDesconto,
                p.ValorTotal,
                p.DataAprovacao,
                p.Itens.Count,
                faturamentoAtivo?.Numero
            );
        }).ToList();

        return new PaginatedList<PedidoVendaListItemDto>(dtos, totalItens, request.Pagina, request.TamanhoPagina);
    }

    public async Task<Result<PedidoVendaDetalheDto>> Handle(GetPedidoVendaByIdQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.PedidosVenda.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Orcamento)
            .Include(p => p.Deposito)
            .Include(p => p.FormaPagamento)
            .Include(p => p.Faturamentos)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Servico)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (p == null)
            return Result<PedidoVendaDetalheDto>.Falha($"Pedido de venda com ID {request.Id} não encontrado.");

        var itensDto = p.Itens.Select(i => new ItemVendaDto(
            i.Id,
            i.TipoItem,
            i.ProdutoId,
            i.Produto?.Nome,
            i.Produto?.Codigo,
            i.ServicoId,
            i.Servico?.Nome,
            i.Servico?.Codigo,
            i.Descricao,
            i.Quantidade,
            i.PrecoUnitario,
            i.DescontoTipo,
            i.Desconto,
            i.ValorDesconto,
            i.SubTotal,
            i.CustoUnitario
        )).ToList();

        var faturamentosDto = p.Faturamentos.Select(f => new FaturamentoResumoDto(
            f.Id,
            f.Numero,
            f.DataFaturamento,
            f.Status,
            f.ValorTotal,
            f.CustoTotal,
            f.IdParcelamento,
            f.NotaFiscalId,
            f.DataCancelamento
        )).ToList();

        var detalhe = new PedidoVendaDetalheDto(
            p.Id,
            p.Numero,
            p.ClienteId,
            p.Cliente.Nome,
            p.Cliente.CpfCnpj,
            p.Cliente.Email,
            p.Cliente.Telefone,
            p.OrcamentoId,
            p.Orcamento?.Numero,
            p.VendedorId,
            p.VendedorNome,
            p.DataPedido,
            p.Status,
            p.DepositoId,
            p.Deposito?.Nome,
            p.FormaPagamentoId,
            p.FormaPagamento?.Nome,
            p.NumeroParcelas,
            p.IntervaloDias,
            p.DiasPrimeiroVencimento,
            p.ValorBruto,
            p.DescontoTipo,
            p.DescontoGeral,
            p.ValorDesconto,
            p.ValorTotal,
            p.Observacao,
            p.DataAprovacao,
            p.DataCancelamento,
            p.MotivoCancelamento,
            p.CriadoEm,
            itensDto,
            faturamentosDto
        );

        return Result<PedidoVendaDetalheDto>.Ok(detalhe);
    }
}
