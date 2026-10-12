using ERP.Application.Common.Interfaces;
using ERP.Application.Relatorios.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Relatorios.Queries;

#region 1. Relatório de Vendas
public record GetRelatorioVendasQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? ClienteId = null,
    StatusPedidoVenda? Status = null
) : IRequest<RelatorioVendasResultDto>;

public class GetRelatorioVendasQueryHandler : IRequestHandler<GetRelatorioVendasQuery, RelatorioVendasResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetRelatorioVendasQueryHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<RelatorioVendasResultDto> Handle(GetRelatorioVendasQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dataInicio = request.DataInicio.Date;
        var dataFim = request.DataFim.Date.AddDays(1).AddTicks(-1);

        var query = _context.PedidosVenda.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.FormaPagamento)
            .Include(p => p.Itens)
            .Where(p => p.EmpresaId == empresaId && p.DataPedido >= dataInicio && p.DataPedido <= dataFim);

        if (request.ClienteId.HasValue)
            query = query.Where(p => p.ClienteId == request.ClienteId.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);
        else
            query = query.Where(p => p.Status != StatusPedidoVenda.Cancelado);

        var pedidos = await query.OrderByDescending(p => p.DataPedido).ToListAsync(cancellationToken);

        List<RelatorioVendasItemDto> itens = pedidos.Select(p => new RelatorioVendasItemDto
        {
            PedidoId = p.Id,
            Numero = p.Numero,
            DataPedido = p.DataPedido,
            ClienteNome = p.Cliente.Nome,
            VendedorNome = p.VendedorNome ?? "Não informado",
            FormaPagamento = p.FormaPagamento?.Nome ?? "Padrão",
            QuantidadeItens = p.Itens.Count(),
            ValorBruto = p.ValorBruto,
            ValorDesconto = p.ValorDesconto,
            ValorTotal = p.ValorTotal,
            Status = p.Status.ToString()
        }).ToList();

        var totalValor = itens.Sum(i => i.ValorTotal);
        var totalDesconto = itens.Sum(i => i.ValorDesconto);
        var ticketMedio = itens.Count > 0 ? totalValor / itens.Count : 0;

        return new RelatorioVendasResultDto
        {
            Itens = itens,
            TotalPedidos = itens.Count,
            ValorTotalFaturado = totalValor,
            ValorTotalDescontos = totalDesconto,
            TicketMedio = Math.Round(ticketMedio, 2)
        };
    }
}
#endregion

#region 2. Relatório Financeiro
public record GetRelatorioFinanceiroQuery(
    DateTime DataInicio,
    DateTime DataFim,
    string Tipo = "Todos", // "Receber", "Pagar" ou "Todos"
    StatusContaFinanceira? Status = null
) : IRequest<RelatorioFinanceiroResultDto>;

public class GetRelatorioFinanceiroQueryHandler : IRequestHandler<GetRelatorioFinanceiroQuery, RelatorioFinanceiroResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetRelatorioFinanceiroQueryHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<RelatorioFinanceiroResultDto> Handle(GetRelatorioFinanceiroQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dataInicio = request.DataInicio.Date;
        var dataFim = request.DataFim.Date.AddDays(1).AddTicks(-1);
        var hoje = DateTime.UtcNow.Date;

        var lista = new List<RelatorioFinanceiroItemDto>();

        // Contas a Receber
        if (request.Tipo == "Todos" || request.Tipo == "Receber")
        {
            var queryReceber = _context.ContasReceber.AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.PlanoConta)
                .Where(c => c.EmpresaId == empresaId && c.DataVencimento >= dataInicio && c.DataVencimento <= dataFim);

            if (request.Status.HasValue)
                queryReceber = queryReceber.Where(c => c.Status == request.Status.Value);
            else
                queryReceber = queryReceber.Where(c => c.Status != StatusContaFinanceira.Cancelada);

            var titulosReceber = await queryReceber.ToListAsync(cancellationToken);
            lista.AddRange(titulosReceber.Select(c => new RelatorioFinanceiroItemDto
            {
                TituloId = c.Id,
                Tipo = "Receber",
                Documento = c.NumeroDocumento ?? $"CR-{c.Id}",
                DataEmissao = c.DataEmissao,
                DataVencimento = c.DataVencimento,
                PessoaNome = c.Cliente?.Nome ?? "Cliente Avulso",
                Descricao = c.Descricao,
                CategoriaPlano = c.PlanoConta?.Descricao ?? "Receita Operacional",
                ValorOriginal = c.ValorOriginal,
                ValorBaixado = c.ValorRecebido,
                SaldoRestante = c.SaldoRestante,
                Status = c.Status.ToString(),
                Vencido = c.DataVencimento < hoje && c.Status != StatusContaFinanceira.Paga
            }));
        }

        // Contas a Pagar
        if (request.Tipo == "Todos" || request.Tipo == "Pagar")
        {
            var queryPagar = _context.ContasPagar.AsNoTracking()
                .Include(c => c.Fornecedor)
                .Include(c => c.PlanoConta)
                .Where(c => c.EmpresaId == empresaId && c.DataVencimento >= dataInicio && c.DataVencimento <= dataFim);

            if (request.Status.HasValue)
                queryPagar = queryPagar.Where(c => c.Status == request.Status.Value);
            else
                queryPagar = queryPagar.Where(c => c.Status != StatusContaFinanceira.Cancelada);

            var titulosPagar = await queryPagar.ToListAsync(cancellationToken);
            lista.AddRange(titulosPagar.Select(c => new RelatorioFinanceiroItemDto
            {
                TituloId = c.Id,
                Tipo = "Pagar",
                Documento = c.NumeroDocumento ?? $"CP-{c.Id}",
                DataEmissao = c.DataEmissao,
                DataVencimento = c.DataVencimento,
                PessoaNome = c.Fornecedor?.RazaoSocial ?? "Fornecedor",
                Descricao = c.Descricao,
                CategoriaPlano = c.PlanoConta?.Descricao ?? "Despesa Operacional",
                ValorOriginal = c.ValorOriginal,
                ValorBaixado = c.ValorPago,
                SaldoRestante = c.SaldoRestante,
                Status = c.Status.ToString(),
                Vencido = c.DataVencimento < hoje && c.Status != StatusContaFinanceira.Paga
            }));
        }

        var itensOrdenados = lista.OrderBy(i => i.DataVencimento).ToList();

        return new RelatorioFinanceiroResultDto
        {
            Itens = itensOrdenados,
            QuantidadeTitulos = itensOrdenados.Count,
            TotalOriginal = itensOrdenados.Sum(i => i.ValorOriginal),
            TotalBaixado = itensOrdenados.Sum(i => i.ValorBaixado),
            TotalEmAberto = itensOrdenados.Where(i => i.Status != "Paga").Sum(i => i.SaldoRestante),
            TotalVencido = itensOrdenados.Where(i => i.Vencido).Sum(i => i.SaldoRestante)
        };
    }
}
#endregion

#region 3. Relatório de Estoque (Posição & Curva ABC)
public record GetRelatorioEstoqueQuery(
    int? CategoriaId = null,
    bool SomenteCriticos = false
) : IRequest<RelatorioEstoqueResultDto>;

public class GetRelatorioEstoqueQueryHandler : IRequestHandler<GetRelatorioEstoqueQuery, RelatorioEstoqueResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetRelatorioEstoqueQueryHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<RelatorioEstoqueResultDto> Handle(GetRelatorioEstoqueQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var queryProdutos = _context.Produtos.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.UnidadeMedida)
            .Where(p => p.EmpresaId == empresaId && p.Ativo);

        if (request.CategoriaId.HasValue)
            queryProdutos = queryProdutos.Where(p => p.CategoriaId == request.CategoriaId.Value);

        var produtos = await queryProdutos.ToListAsync(cancellationToken);

        // Agrupamento de saldos por produto
        var saldosEstoque = await _context.EstoqueProdutos.AsNoTracking()
            .Where(e => e.EmpresaId == empresaId)
            .GroupBy(e => e.ProdutoId)
            .Select(g => new
            {
                ProdutoId = g.Key,
                Saldo = g.Sum(e => e.Quantidade),
                CustoMedio = g.Average(e => e.CustoMedio)
            })
            .ToListAsync(cancellationToken);

        var saldosMap = saldosEstoque.ToDictionary(s => s.ProdutoId, s => (s.Saldo, s.CustoMedio));

        var lista = new List<RelatorioEstoqueItemDto>();

        foreach (var prod in produtos)
        {
            decimal saldo = 0;
            decimal custoMedio = prod.PrecoCusto;

            if (saldosMap.TryGetValue(prod.Id, out var dados))
            {
                saldo = dados.Saldo;
                if (dados.CustoMedio > 0)
                    custoMedio = dados.CustoMedio;
            }

            var valorCusto = saldo * custoMedio;
            var valorVenda = saldo * prod.PrecoVenda;
            bool critico = prod.EstoqueMinimo > 0 && saldo <= prod.EstoqueMinimo;

            if (request.SomenteCriticos && !critico)
                continue;

            lista.Add(new RelatorioEstoqueItemDto
            {
                ProdutoId = prod.Id,
                Codigo = prod.Codigo,
                Nome = prod.Nome,
                Categoria = prod.Categoria?.Nome ?? "Sem Categoria",
                UnidadeMedida = prod.UnidadeMedida?.Sigla ?? "UN",
                SaldoFisico = saldo,
                CustoMedio = custoMedio,
                PrecoVenda = prod.PrecoVenda,
                ValorTotalCusto = valorCusto,
                ValorTotalVenda = valorVenda,
                EstoqueMinimo = prod.EstoqueMinimo,
                StatusEstoque = critico ? "Crítico" : "Normal"
            });
        }

        // Cálculo da Curva ABC baseada no Valor Total em Estoque
        var totalCustoGeral = lista.Sum(i => i.ValorTotalCusto);
        if (totalCustoGeral > 0)
        {
            var ordenados = lista.OrderByDescending(i => i.ValorTotalCusto).ToList();
            decimal acumulado = 0;

            foreach (var item in ordenados)
            {
                var percentual = (item.ValorTotalCusto / totalCustoGeral) * 100;
                item.PercentualRepresentatividade = Math.Round(percentual, 2);
                acumulado += percentual;

                if (acumulado <= 80)
                    item.ClassificacaoAbc = "A";
                else if (acumulado <= 95)
                    item.ClassificacaoAbc = "B";
                else
                    item.ClassificacaoAbc = "C";
            }

            lista = ordenados;
        }

        return new RelatorioEstoqueResultDto
        {
            Itens = lista,
            TotalItens = lista.Count,
            QuantidadePecasTotal = lista.Sum(i => i.SaldoFisico),
            ValorTotalCustoEstoque = lista.Sum(i => i.ValorTotalCusto),
            ValorTotalPotencialVenda = lista.Sum(i => i.ValorTotalVenda),
            TotalProdutosCriticos = lista.Count(i => i.StatusEstoque == "Crítico")
        };
    }
}
#endregion

#region 4. Relatório de Compras
public record GetRelatorioComprasQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? FornecedorId = null
) : IRequest<RelatorioComprasResultDto>;

public class GetRelatorioComprasQueryHandler : IRequestHandler<GetRelatorioComprasQuery, RelatorioComprasResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetRelatorioComprasQueryHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<RelatorioComprasResultDto> Handle(GetRelatorioComprasQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dataInicio = request.DataInicio.Date;
        var dataFim = request.DataFim.Date.AddDays(1).AddTicks(-1);

        var query = _context.EntradasMercadoria.AsNoTracking()
            .Include(e => e.Fornecedor)
            .Where(e => e.EmpresaId == empresaId && e.DataEntrada >= dataInicio && e.DataEntrada <= dataFim && e.Status != StatusEntradaMercadoria.Cancelada);

        if (request.FornecedorId.HasValue)
            query = query.Where(e => e.FornecedorId == request.FornecedorId.Value);

        var entradas = await query.OrderByDescending(e => e.DataEntrada).ToListAsync(cancellationToken);

        var itens = entradas.Select(e => new RelatorioComprasItemDto
        {
            EntradaId = e.Id,
            Numero = e.Numero,
            DataEntrada = e.DataEntrada,
            FornecedorNome = e.Fornecedor.RazaoSocial,
            NumeroNotaFiscal = e.NumeroNotaFiscal,
            ValorProdutos = e.ValorProdutos,
            ValorFrete = e.ValorFrete,
            ValorOutros = e.ValorOutrasDespesas + e.ValorSeguro,
            ValorDesconto = e.ValorDesconto,
            ValorTotal = e.ValorTotal,
            Status = e.Status.ToString()
        }).ToList();

        var totalCompras = itens.Sum(i => i.ValorTotal);
        var totalFrete = itens.Sum(i => i.ValorFrete);
        var media = itens.Count > 0 ? totalCompras / itens.Count : 0;

        return new RelatorioComprasResultDto
        {
            Itens = itens,
            TotalEntradas = itens.Count,
            ValorTotalCompras = totalCompras,
            ValorTotalFrete = totalFrete,
            MediaPorCompra = Math.Round(media, 2)
        };
    }
}
#endregion

#region 5. Relatório Fiscal
public record GetRelatorioFiscalQuery(
    DateTime DataInicio,
    DateTime DataFim,
    StatusNotaFiscal? Status = null,
    ModeloDocumentoFiscal? Modelo = null
) : IRequest<RelatorioFiscalResultDto>;

public class GetRelatorioFiscalQueryHandler : IRequestHandler<GetRelatorioFiscalQuery, RelatorioFiscalResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetRelatorioFiscalQueryHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<RelatorioFiscalResultDto> Handle(GetRelatorioFiscalQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dataInicio = request.DataInicio.Date;
        var dataFim = request.DataFim.Date.AddDays(1).AddTicks(-1);

        var query = _context.NotasFiscais.AsNoTracking()
            .Include(n => n.Cliente)
            .Include(n => n.Itens)
            .Where(n => n.EmpresaId == empresaId && n.DataEmissao >= dataInicio && n.DataEmissao <= dataFim);

        if (request.Status.HasValue)
            query = query.Where(n => n.Status == request.Status.Value);

        if (request.Modelo.HasValue)
            query = query.Where(n => n.Modelo == request.Modelo.Value);

        var notas = await query.OrderByDescending(n => n.DataEmissao).ToListAsync(cancellationToken);

        List<RelatorioFiscalItemDto> itens = notas.Select(n => new RelatorioFiscalItemDto
        {
            NotaFiscalId = n.Id,
            Modelo = ((int)n.Modelo).ToString(),
            Serie = n.Serie,
            Numero = n.Numero,
            DataEmissao = n.DataEmissao,
            DestinatarioNome = n.Cliente?.Nome ?? "Consumidor Final",
            ChaveAcesso = n.ChaveAcesso ?? string.Empty,
            ValorTotal = n.ValorTotalNota,
            BaseCalculoIcms = n.Itens.Sum(i => i.BaseCalculoIcms),
            ValorIcms = n.Itens.Sum(i => i.ValorIcms),
            ValorPis = n.Itens.Sum(i => i.ValorPis),
            ValorCofins = n.Itens.Sum(i => i.ValorCofins),
            ValorIpi = n.Itens.Sum(i => i.ValorIpi),
            Status = n.Status.ToString()
        }).ToList();

        var totalValor = itens.Sum(i => i.ValorTotal);
        var totalIcms = itens.Sum(i => i.ValorIcms);
        var totalPis = itens.Sum(i => i.ValorPis);
        var totalCofins = itens.Sum(i => i.ValorCofins);
        var totalIpi = itens.Sum(i => i.ValorIpi);

        return new RelatorioFiscalResultDto
        {
            Itens = itens,
            TotalNotas = itens.Count,
            ValorTotalDocumentos = totalValor,
            TotalIcms = totalIcms,
            TotalPis = totalPis,
            TotalCofins = totalCofins,
            TotalIpi = totalIpi,
            TotalTributosConsolidados = totalIcms + totalPis + totalCofins + totalIpi
        };
    }
}
#endregion
