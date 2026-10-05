using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Vendas.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Vendas.Queries;

public record GetOrcamentosQuery(
    string? Busca = null,
    int? ClienteId = null,
    StatusOrcamento? Status = null,
    bool? ApenasExpirados = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    int Pagina = 1,
    int TamanhoPagina = 20
) : IRequest<PaginatedList<OrcamentoListItemDto>>;

public record GetOrcamentoByIdQuery(int Id) : IRequest<Result<OrcamentoDetalheDto>>;

public class OrcamentoQueriesHandler :
    IRequestHandler<GetOrcamentosQuery, PaginatedList<OrcamentoListItemDto>>,
    IRequestHandler<GetOrcamentoByIdQuery, Result<OrcamentoDetalheDto>>
{
    private readonly IApplicationDbContext _context;

    public OrcamentoQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<OrcamentoListItemDto>> Handle(GetOrcamentosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orcamentos.AsNoTracking()
            .Include(o => o.Cliente)
            .Include(o => o.PedidoVenda)
            .Include(o => o.Itens)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = request.Busca.Trim().ToLower();
            query = query.Where(o =>
                o.Numero.ToLower().Contains(busca) ||
                o.Cliente.Nome.ToLower().Contains(busca) ||
                (o.Cliente.CpfCnpj != null && o.Cliente.CpfCnpj.Contains(busca)) ||
                (o.VendedorNome != null && o.VendedorNome.ToLower().Contains(busca))
            );
        }

        if (request.ClienteId.HasValue)
            query = query.Where(o => o.ClienteId == request.ClienteId.Value);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        var hoje = DateTime.UtcNow.Date;
        if (request.ApenasExpirados == true)
        {
            query = query.Where(o => o.Status == StatusOrcamento.Aberto && o.DataValidade.Date < hoje);
        }

        if (request.DataInicio.HasValue)
        {
            var inicioUtc = request.DataInicio.Value.Date.ToUniversalTime();
            query = query.Where(o => o.DataOrcamento >= inicioUtc);
        }

        if (request.DataFim.HasValue)
        {
            var fimUtc = request.DataFim.Value.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
            query = query.Where(o => o.DataOrcamento <= fimUtc);
        }

        int totalItens = await query.CountAsync(cancellationToken);

        var itensDb = await query
            .OrderByDescending(o => o.DataOrcamento)
            .ThenByDescending(o => o.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .ToListAsync(cancellationToken);

        var dtos = itensDb.Select(o =>
        {
            string statusCalculado = o.Status == StatusOrcamento.Aberto && o.DataValidade.Date < hoje
                ? "Expirado"
                : o.Status.ToString();

            return new OrcamentoListItemDto(
                o.Id,
                o.Numero,
                o.ClienteId,
                o.Cliente.Nome,
                o.Cliente.CpfCnpj,
                o.VendedorNome,
                o.DataOrcamento,
                o.DataValidade,
                o.Status,
                statusCalculado,
                o.ValorBruto,
                o.ValorDesconto,
                o.ValorTotal,
                o.PedidoVendaId,
                o.PedidoVenda?.Numero,
                o.Itens.Count
            );
        }).ToList();

        return new PaginatedList<OrcamentoListItemDto>(dtos, totalItens, request.Pagina, request.TamanhoPagina);
    }

    public async Task<Result<OrcamentoDetalheDto>> Handle(GetOrcamentoByIdQuery request, CancellationToken cancellationToken)
    {
        var o = await _context.Orcamentos.AsNoTracking()
            .Include(o => o.Cliente)
            .Include(o => o.PedidoVenda)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Produto)
            .Include(o => o.Itens)
                .ThenInclude(i => i.Servico)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (o == null)
            return Result<OrcamentoDetalheDto>.Falha($"Orçamento com ID {request.Id} não encontrado.");

        var hoje = DateTime.UtcNow.Date;
        string statusCalculado = o.Status == StatusOrcamento.Aberto && o.DataValidade.Date < hoje
            ? "Expirado"
            : o.Status.ToString();

        var itensDto = o.Itens.Select(i => new ItemVendaDto(
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
            0m
        )).ToList();

        var detalhe = new OrcamentoDetalheDto(
            o.Id,
            o.Numero,
            o.ClienteId,
            o.Cliente.Nome,
            o.Cliente.CpfCnpj,
            o.Cliente.Email,
            o.Cliente.Telefone,
            o.VendedorId,
            o.VendedorNome,
            o.DataOrcamento,
            o.DataValidade,
            o.Status,
            statusCalculado,
            o.ValorBruto,
            o.DescontoTipo,
            o.DescontoGeral,
            o.ValorDesconto,
            o.ValorTotal,
            o.Observacao,
            o.PedidoVendaId,
            o.PedidoVenda?.Numero,
            o.CriadoEm,
            itensDto
        );

        return Result<OrcamentoDetalheDto>.Ok(detalhe);
    }
}
