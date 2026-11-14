using ERP.Application.Common.Interfaces;
using ERP.Application.Pdv.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Pdv.Queries;

public record ListarTerminaisPdvQuery(bool? ApenasAtivos = null) : IRequest<List<TerminalPdvDto>>;
public record ObterSessaoAbertaTerminalQuery(int TerminalId) : IRequest<SessaoCaixaPdvDto?>;
public record BuscarProdutosPdvQuery(string? Busca = null) : IRequest<List<ProdutoPdvDto>>;
public record ListarPreVendasAbertasQuery(string? Busca = null) : IRequest<List<VendaPdvDto>>;
public record ObterVendaPdvPorIdQuery(int Id) : IRequest<VendaPdvDto?>;
public record ListarVendasSessaoQuery(int SessaoId) : IRequest<List<VendaPdvDto>>;

public class PdvQueriesHandler :
    IRequestHandler<ListarTerminaisPdvQuery, List<TerminalPdvDto>>,
    IRequestHandler<ObterSessaoAbertaTerminalQuery, SessaoCaixaPdvDto?>,
    IRequestHandler<BuscarProdutosPdvQuery, List<ProdutoPdvDto>>,
    IRequestHandler<ListarPreVendasAbertasQuery, List<VendaPdvDto>>,
    IRequestHandler<ObterVendaPdvPorIdQuery, VendaPdvDto?>,
    IRequestHandler<ListarVendasSessaoQuery, List<VendaPdvDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public PdvQueriesHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<List<TerminalPdvDto>> Handle(ListarTerminaisPdvQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var query = _context.TerminaisPdv.AsNoTracking().Where(t => t.EmpresaId == empresaId);

        if (request.ApenasAtivos == true)
            query = query.Where(t => t.Ativo);

        return await query
            .OrderBy(t => t.Codigo)
            .Select(t => new TerminalPdvDto
            {
                Id = t.Id,
                Codigo = t.Codigo,
                Nome = t.Nome,
                Tipo = t.Tipo,
                DepositoId = t.DepositoId,
                DepositoNome = t.Deposito != null ? t.Deposito.Nome : null,
                ContaBancariaId = t.ContaBancariaId,
                ContaBancariaNome = t.ContaBancaria != null ? t.ContaBancaria.Descricao : null,
                ImpressoraCupom = t.ImpressoraCupom,
                Ativo = t.Ativo,
                PossuiSessaoAberta = t.Sessoes.Any(s => s.Status == StatusSessaoCaixa.Aberta),
                OperadorAtual = t.Sessoes
                    .Where(s => s.Status == StatusSessaoCaixa.Aberta)
                    .Select(s => s.OperadorNome)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SessaoCaixaPdvDto?> Handle(ObterSessaoAbertaTerminalQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var s = await _context.SessoesCaixaPdv
            .AsNoTracking()
            .Include(x => x.TerminalPdv)
            .Include(x => x.Movimentos)
            .Include(x => x.Vendas)
            .FirstOrDefaultAsync(x =>
                x.EmpresaId == empresaId &&
                x.TerminalPdvId == request.TerminalId &&
                x.Status == StatusSessaoCaixa.Aberta, cancellationToken);

        if (s == null) return null;

        var vendasValidas = s.Vendas.Where(v => v.Status == StatusVendaPdv.Finalizada).ToList();

        return new SessaoCaixaPdvDto
        {
            Id = s.Id,
            TerminalPdvId = s.TerminalPdvId,
            TerminalNome = s.TerminalPdv.Nome,
            OperadorNome = s.OperadorNome,
            DataAbertura = s.DataAbertura,
            ValorAbertura = s.ValorAbertura,
            Status = s.Status,
            TotalVendas = vendasValidas.Sum(v => v.ValorTotal),
            QuantidadeVendas = vendasValidas.Count,
            TotalSuprimentos = s.Movimentos.Where(m => m.Tipo == TipoMovimentoCaixa.Suprimento).Sum(m => m.Valor),
            TotalSangrias = s.Movimentos.Where(m => m.Tipo == TipoMovimentoCaixa.Sangria).Sum(m => m.Valor)
        };
    }

    public async Task<List<ProdutoPdvDto>> Handle(BuscarProdutosPdvQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var query = _context.Produtos
            .AsNoTracking()
            .Where(p => p.EmpresaId == empresaId && p.Ativo);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                p.Codigo.ToLower().Contains(termo) ||
                (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(termo)));
        }

        return await query
            .OrderBy(p => p.Nome)
            .Take(60)
            .Select(p => new ProdutoPdvDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                CodigoBarras = p.CodigoBarras,
                Nome = p.Nome,
                PrecoVenda = p.PrecoVenda,
                ImagemUrl = p.ImagemUrl,
                UnidadeSigla = p.UnidadeMedida != null ? p.UnidadeMedida.Sigla : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VendaPdvDto>> Handle(ListarPreVendasAbertasQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var query = _context.VendasPdv
            .AsNoTracking()
            .Include(v => v.TerminalPdv)
            .Include(v => v.Cliente)
            .Include(v => v.Itens)
            .Where(v => v.EmpresaId == empresaId && v.Status == StatusVendaPdv.PreVenda);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(v =>
                v.Numero.ToLower().Contains(termo) ||
                (v.ConsumidorNome != null && v.ConsumidorNome.ToLower().Contains(termo)) ||
                (v.Cliente != null && v.Cliente.Nome.ToLower().Contains(termo)) ||
                (v.VendedorNome != null && v.VendedorNome.ToLower().Contains(termo)));
        }

        var lista = await query.OrderByDescending(v => v.DataVenda).Take(50).ToListAsync(cancellationToken);
        return lista.Select(MapDto).ToList();
    }

    public async Task<VendaPdvDto?> Handle(ObterVendaPdvPorIdQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var v = await _context.VendasPdv
            .AsNoTracking()
            .Include(x => x.TerminalPdv)
            .Include(x => x.Cliente)
            .Include(x => x.Itens)
            .Include(x => x.Pagamentos)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);

        return v == null ? null : MapDto(v);
    }

    public async Task<List<VendaPdvDto>> Handle(ListarVendasSessaoQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var lista = await _context.VendasPdv
            .AsNoTracking()
            .Include(x => x.TerminalPdv)
            .Include(x => x.Cliente)
            .Include(x => x.Itens)
            .Include(x => x.Pagamentos)
            .Where(x => x.EmpresaId == empresaId && x.SessaoCaixaPdvId == request.SessaoId)
            .OrderByDescending(x => x.DataVenda)
            .ToListAsync(cancellationToken);

        return lista.Select(MapDto).ToList();
    }

    private static VendaPdvDto MapDto(ERP.Domain.Core.Entities.Pdv.VendaPdv v) => new()
    {
        Id = v.Id,
        Numero = v.Numero,
        TerminalPdvId = v.TerminalPdvId,
        TerminalNome = v.TerminalPdv.Nome,
        ClienteId = v.ClienteId,
        ClienteNome = v.Cliente?.Nome,
        ConsumidorNome = v.ConsumidorNome,
        ConsumidorCpf = v.ConsumidorCpf,
        VendedorId = v.VendedorId,
        VendedorNome = v.VendedorNome,
        Status = v.Status,
        DataVenda = v.DataVenda,
        ValorBruto = v.ValorBruto,
        ValorDesconto = v.ValorDesconto,
        ValorTotal = v.ValorTotal,
        Troco = v.Troco,
        Observacoes = v.Observacoes,
        Itens = v.Itens.Select(i => new VendaPdvItemInputDto
        {
            ProdutoId = i.ProdutoId,
            CodigoProduto = i.CodigoProduto,
            NomeProduto = i.NomeProduto,
            Quantidade = i.Quantidade,
            ValorUnitario = i.ValorUnitario,
            ValorDesconto = i.ValorDesconto
        }).ToList(),
        Pagamentos = v.Pagamentos.Select(p => new VendaPdvPagamentoInputDto
        {
            FormaPagamentoId = p.FormaPagamentoId,
            DescricaoForma = p.DescricaoForma,
            Valor = p.Valor
        }).ToList()
    };
}
