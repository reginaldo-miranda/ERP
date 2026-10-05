using ERP.Application.Common.Interfaces;
using ERP.Application.Estoque.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Estoque.Queries;

public record GetDepositosQuery(bool SomenteAtivos = true) : IRequest<List<DepositoDto>>;

public record GetPosicaoEstoqueQuery(
    int? DepositoId = null,
    string? Busca = null,
    bool? SomenteAbaixoMinimo = null
) : IRequest<List<PosicaoEstoqueDto>>;

public record GetMovimentacoesEstoqueQuery(
    int? ProdutoId = null,
    int? DepositoId = null,
    int Top = 100
) : IRequest<List<MovimentacaoEstoqueDto>>;

public record GetLotesQuery(
    int? ProdutoId = null,
    int? DepositoId = null,
    bool? SomenteAtivos = true,
    bool? SomenteVencidos = null,
    bool? SomenteCriticos30Dias = null
) : IRequest<List<LoteDto>>;

public record GetNumerosSerieQuery(
    int? ProdutoId = null,
    int? DepositoId = null,
    StatusNumeroSerie? Status = null,
    string? Busca = null
) : IRequest<List<NumeroSerieDto>>;

public record GetInventariosQuery(
    int? DepositoId = null,
    StatusInventario? Status = null
) : IRequest<List<InventarioDto>>;

public record GetInventarioPorIdQuery(int Id) : IRequest<InventarioDto?>;

public record GetInventarioItensQuery(int InventarioId) : IRequest<List<InventarioItemDto>>;

public class GetEstoqueQueriesHandler :
    IRequestHandler<GetDepositosQuery, List<DepositoDto>>,
    IRequestHandler<GetPosicaoEstoqueQuery, List<PosicaoEstoqueDto>>,
    IRequestHandler<GetMovimentacoesEstoqueQuery, List<MovimentacaoEstoqueDto>>,
    IRequestHandler<GetLotesQuery, List<LoteDto>>,
    IRequestHandler<GetNumerosSerieQuery, List<NumeroSerieDto>>,
    IRequestHandler<GetInventariosQuery, List<InventarioDto>>,
    IRequestHandler<GetInventarioPorIdQuery, InventarioDto?>,
    IRequestHandler<GetInventarioItensQuery, List<InventarioItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEstoqueQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepositoDto>> Handle(GetDepositosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Depositos.AsNoTracking();
        if (request.SomenteAtivos)
            query = query.Where(d => d.Ativo);

        return await query
            .OrderByDescending(d => d.Padrao)
            .ThenBy(d => d.Nome)
            .Select(d => new DepositoDto(
                d.Id,
                d.Nome,
                d.Codigo,
                d.Endereco,
                d.Responsavel,
                d.Ativo,
                d.Padrao
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PosicaoEstoqueDto>> Handle(GetPosicaoEstoqueQuery request, CancellationToken cancellationToken)
    {
        var query = _context.EstoqueProdutos
            .AsNoTracking()
            .Include(ep => ep.Produto)
                .ThenInclude(p => p.UnidadeMedida)
            .Include(ep => ep.Deposito)
            .AsQueryable();

        if (request.DepositoId.HasValue && request.DepositoId.Value > 0)
        {
            query = query.Where(ep => ep.DepositoId == request.DepositoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(ep =>
                ep.Produto.Nome.ToLower().Contains(termo) ||
                ep.Produto.Codigo.ToLower().Contains(termo) ||
                (ep.Produto.CodigoBarras != null && ep.Produto.CodigoBarras.ToLower().Contains(termo))
            );
        }

        if (request.SomenteAbaixoMinimo == true)
        {
            query = query.Where(ep => ep.Quantidade <= ep.EstoqueMinimo);
        }

        var itens = await query
            .OrderBy(ep => ep.Produto.Nome)
            .ThenBy(ep => ep.Deposito.Nome)
            .Select(ep => new PosicaoEstoqueDto(
                ep.Id,
                ep.ProdutoId,
                ep.Produto.Codigo,
                ep.Produto.Nome,
                ep.Produto.UnidadeMedida != null ? ep.Produto.UnidadeMedida.Sigla : null,
                ep.DepositoId,
                ep.Deposito.Nome,
                ep.Quantidade,
                ep.CustoMedio,
                ep.CustoUltimaCompra,
                ep.EstoqueMinimo,
                ep.EstoqueMaximo,
                ep.Quantidade <= ep.EstoqueMinimo,
                ep.Quantidade * (ep.CustoMedio > 0 ? ep.CustoMedio : ep.Produto.PrecoCusto)
            ))
            .ToListAsync(cancellationToken);

        return itens;
    }

    public async Task<List<MovimentacaoEstoqueDto>> Handle(GetMovimentacoesEstoqueQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MovimentacoesEstoque
            .AsNoTracking()
            .Include(m => m.Produto)
            .Include(m => m.DepositoOrigem)
            .Include(m => m.DepositoDestino)
            .AsQueryable();

        if (request.ProdutoId.HasValue && request.ProdutoId.Value > 0)
        {
            query = query.Where(m => m.ProdutoId == request.ProdutoId.Value);
        }

        if (request.DepositoId.HasValue && request.DepositoId.Value > 0)
        {
            query = query.Where(m => m.DepositoOrigemId == request.DepositoId.Value || m.DepositoDestinoId == request.DepositoId.Value);
        }

        return await query
            .OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.Id)
            .Take(request.Top)
            .Select(m => new MovimentacaoEstoqueDto(
                m.Id,
                m.ProdutoId,
                m.Produto.Nome,
                m.Produto.Codigo,
                m.DepositoOrigemId,
                m.DepositoOrigem.Nome,
                m.DepositoDestinoId,
                m.DepositoDestino != null ? m.DepositoDestino.Nome : null,
                m.Tipo,
                m.Origem,
                m.Quantidade,
                m.CustoUnitario,
                m.CustoTotal,
                m.DataMovimentacao,
                m.DocumentoOrigem,
                m.Observacao
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LoteDto>> Handle(GetLotesQuery request, CancellationToken cancellationToken)
    {
        var hoje = DateTime.UtcNow.Date;
        var em30Dias = hoje.AddDays(30);

        var query = _context.Lotes
            .AsNoTracking()
            .Include(l => l.Produto)
            .Include(l => l.Deposito)
            .AsQueryable();

        if (request.SomenteAtivos == true)
            query = query.Where(l => l.Ativo);

        if (request.ProdutoId.HasValue && request.ProdutoId.Value > 0)
            query = query.Where(l => l.ProdutoId == request.ProdutoId.Value);

        if (request.DepositoId.HasValue && request.DepositoId.Value > 0)
            query = query.Where(l => l.DepositoId == request.DepositoId.Value);

        if (request.SomenteVencidos == true)
            query = query.Where(l => l.DataValidade < hoje);
        else if (request.SomenteCriticos30Dias == true)
            query = query.Where(l => l.DataValidade >= hoje && l.DataValidade <= em30Dias);

        var lotes = await query
            .OrderBy(l => l.DataValidade)
            .ThenBy(l => l.Produto.Nome)
            .Select(l => new LoteDto(
                l.Id,
                l.ProdutoId,
                l.Produto.Nome,
                l.Produto.Codigo,
                l.DepositoId,
                l.Deposito.Nome,
                l.CodigoLote,
                l.DataFabricacao,
                l.DataValidade,
                l.QuantidadeInicial,
                l.QuantidadeAtual,
                l.CustoUnitario,
                l.Ativo,
                l.DataValidade < hoje,
                (l.DataValidade.Date - hoje).Days,
                l.Observacoes
            ))
            .ToListAsync(cancellationToken);

        return lotes;
    }

    public async Task<List<NumeroSerieDto>> Handle(GetNumerosSerieQuery request, CancellationToken cancellationToken)
    {
        var query = _context.NumerosSerie
            .AsNoTracking()
            .Include(n => n.Produto)
            .Include(n => n.Deposito)
            .Include(n => n.Lote)
            .AsQueryable();

        if (request.ProdutoId.HasValue && request.ProdutoId.Value > 0)
            query = query.Where(n => n.ProdutoId == request.ProdutoId.Value);

        if (request.DepositoId.HasValue && request.DepositoId.Value > 0)
            query = query.Where(n => n.DepositoId == request.DepositoId.Value);

        if (request.Status.HasValue)
            query = query.Where(n => n.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(n =>
                n.Numero.ToLower().Contains(termo) ||
                n.Produto.Nome.ToLower().Contains(termo) ||
                n.Produto.Codigo.ToLower().Contains(termo)
            );
        }

        return await query
            .OrderBy(n => n.Produto.Nome)
            .ThenBy(n => n.Numero)
            .Select(n => new NumeroSerieDto(
                n.Id,
                n.ProdutoId,
                n.Produto.Nome,
                n.Produto.Codigo,
                n.DepositoId,
                n.Deposito.Nome,
                n.LoteId,
                n.Lote != null ? n.Lote.CodigoLote : null,
                n.Numero,
                n.Status,
                n.DataEntrada,
                n.DataSaida,
                n.Observacoes
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<InventarioDto>> Handle(GetInventariosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Inventarios
            .AsNoTracking()
            .Include(i => i.Deposito)
            .Include(i => i.Categoria)
            .Include(i => i.Itens)
            .AsQueryable();

        if (request.DepositoId.HasValue && request.DepositoId.Value > 0)
            query = query.Where(i => i.DepositoId == request.DepositoId.Value);

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        var inventarios = await query
            .OrderByDescending(i => i.DataAbertura)
            .ToListAsync(cancellationToken);

        return inventarios
            .Select(i => new InventarioDto(
                i.Id,
                i.DepositoId,
                i.Deposito.Nome,
                i.CategoriaId,
                i.Categoria != null ? i.Categoria.Nome : null,
                i.Protocolo,
                i.DataAbertura,
                i.DataFechamento,
                i.Status,
                i.Responsavel,
                i.Observacao,
                i.Itens.Count,
                i.Itens.Sum(it => (it.QuantidadeContada - it.QuantidadeSistema) * it.CustoUnitario)
            ))
            .ToList();
    }

    public async Task<InventarioDto?> Handle(GetInventarioPorIdQuery request, CancellationToken cancellationToken)
    {
        var inv = await _context.Inventarios
            .AsNoTracking()
            .Include(i => i.Deposito)
            .Include(i => i.Categoria)
            .Include(i => i.Itens)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (inv == null) return null;

        return new InventarioDto(
            inv.Id,
            inv.DepositoId,
            inv.Deposito.Nome,
            inv.CategoriaId,
            inv.Categoria != null ? inv.Categoria.Nome : null,
            inv.Protocolo,
            inv.DataAbertura,
            inv.DataFechamento,
            inv.Status,
            inv.Responsavel,
            inv.Observacao,
            inv.Itens.Count,
            inv.Itens.Sum(it => (it.QuantidadeContada - it.QuantidadeSistema) * it.CustoUnitario)
        );
    }

    public async Task<List<InventarioItemDto>> Handle(GetInventarioItensQuery request, CancellationToken cancellationToken)
    {
        return await _context.InventarioItens
            .AsNoTracking()
            .Include(it => it.Produto)
                .ThenInclude(p => p.UnidadeMedida)
            .Include(it => it.Lote)
            .Where(it => it.InventarioId == request.InventarioId)
            .OrderBy(it => it.Produto.Nome)
            .Select(it => new InventarioItemDto(
                it.Id,
                it.InventarioId,
                it.ProdutoId,
                it.Produto.Codigo,
                it.Produto.Nome,
                it.Produto.UnidadeMedida != null ? it.Produto.UnidadeMedida.Sigla : null,
                it.LoteId,
                it.Lote != null ? it.Lote.CodigoLote : null,
                it.QuantidadeSistema,
                it.QuantidadeContada,
                it.QuantidadeContada - it.QuantidadeSistema,
                it.CustoUnitario,
                Math.Round((it.QuantidadeContada - it.QuantidadeSistema) * it.CustoUnitario, 2),
                it.AjusteAplicado,
                it.Observacao
            ))
            .ToListAsync(cancellationToken);
    }
}
