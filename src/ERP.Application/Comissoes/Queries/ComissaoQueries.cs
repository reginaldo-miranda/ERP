using ERP.Application.Comissoes.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Comissoes.Queries;

public record ListarVendedoresQuery(bool? ApenasAtivos = null) : IRequest<List<VendedorDto>>;
public record ListarRegrasComissaoQuery(int? VendedorId = null) : IRequest<List<RegraComissaoDto>>;
public record ListarComissoesVendaQuery(
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    int? VendedorId = null,
    StatusComissao? Status = null
) : IRequest<List<ComissaoVendaDto>>;

public record ObterResumoComissoesQuery(
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    int? VendedorId = null
) : IRequest<ComissaoResumoPeriodoDto>;

public class ComissaoQueriesHandler :
    IRequestHandler<ListarVendedoresQuery, List<VendedorDto>>,
    IRequestHandler<ListarRegrasComissaoQuery, List<RegraComissaoDto>>,
    IRequestHandler<ListarComissoesVendaQuery, List<ComissaoVendaDto>>,
    IRequestHandler<ObterResumoComissoesQuery, ComissaoResumoPeriodoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public ComissaoQueriesHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<List<VendedorDto>> Handle(ListarVendedoresQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var query = _context.Vendedores.AsNoTracking().Where(v => v.EmpresaId == empresaId);

        if (request.ApenasAtivos == true)
            query = query.Where(v => v.Ativo);

        return await query
            .OrderBy(v => v.Nome)
            .Select(v => new VendedorDto
            {
                Id = v.Id,
                UsuarioId = v.UsuarioId,
                Nome = v.Nome,
                Email = v.Email,
                Telefone = v.Telefone,
                Cpf = v.Cpf,
                TipoComissaoPadrao = v.TipoComissaoPadrao,
                PercentualPadrao = v.PercentualPadrao,
                ValorFixoPadrao = v.ValorFixoPadrao,
                Observacoes = v.Observacoes,
                Ativo = v.Ativo,
                TotalRegras = v.Regras.Count(r => r.Ativo)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RegraComissaoDto>> Handle(ListarRegrasComissaoQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var query = _context.RegrasComissao.AsNoTracking().Where(r => r.EmpresaId == empresaId);

        if (request.VendedorId.HasValue)
            query = query.Where(r => r.VendedorId == request.VendedorId.Value);

        return await query
            .OrderByDescending(r => r.Prioridade)
            .ThenBy(r => r.Nome)
            .Select(r => new RegraComissaoDto
            {
                Id = r.Id,
                VendedorId = r.VendedorId,
                VendedorNome = r.Vendedor != null ? r.Vendedor.Nome : "Geral (Todos)",
                Nome = r.Nome,
                Tipo = r.Tipo,
                Percentual = r.Percentual,
                ValorFixo = r.ValorFixo,
                ValorMinimoVenda = r.ValorMinimoVenda,
                Prioridade = r.Prioridade,
                Ativo = r.Ativo
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ComissaoVendaDto>> Handle(ListarComissoesVendaQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var query = _context.ComissoesVenda.AsNoTracking().Where(c => c.EmpresaId == empresaId);

        if (request.DataInicio.HasValue)
            query = query.Where(c => c.DataVenda >= request.DataInicio.Value.Date.ToUniversalTime());

        if (request.DataFim.HasValue)
        {
            var fim = request.DataFim.Value.Date.AddDays(1).ToUniversalTime();
            query = query.Where(c => c.DataVenda < fim);
        }

        if (request.VendedorId.HasValue)
            query = query.Where(c => c.VendedorId == request.VendedorId.Value);

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        return await query
            .OrderByDescending(c => c.DataVenda)
            .Select(c => new ComissaoVendaDto
            {
                Id = c.Id,
                VendedorId = c.VendedorId,
                VendedorNome = c.Vendedor.Nome,
                PedidoVendaId = c.PedidoVendaId,
                PedidoNumero = c.PedidoVenda.Numero,
                ClienteNome = c.PedidoVenda.Cliente.Nome,
                FaturamentoId = c.FaturamentoId,
                FaturamentoNumero = c.Faturamento != null ? c.Faturamento.Numero : null,
                RegraNome = c.RegraComissao != null ? c.RegraComissao.Nome : "Padrão do Vendedor",
                DataVenda = c.DataVenda,
                ValorBaseVenda = c.ValorBaseVenda,
                PercentualAplicado = c.PercentualAplicado,
                ValorComissao = c.ValorComissao,
                Status = c.Status,
                DataAprovacao = c.DataAprovacao,
                DataPagamento = c.DataPagamento,
                Observacoes = c.Observacoes
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ComissaoResumoPeriodoDto> Handle(ObterResumoComissoesQuery request, CancellationToken cancellationToken)
    {
        var itens = await Handle(new ListarComissoesVendaQuery(request.DataInicio, request.DataFim, request.VendedorId, null), cancellationToken);
        var ativas = itens.Where(i => i.Status != StatusComissao.Cancelada).ToList();

        var porVendedor = ativas
            .GroupBy(i => new { i.VendedorId, i.VendedorNome })
            .Select(g => new ComissaoPorVendedorDto
            {
                VendedorId = g.Key.VendedorId,
                VendedorNome = g.Key.VendedorNome,
                QuantidadeVendas = g.Count(),
                TotalVendido = g.Sum(x => x.ValorBaseVenda),
                TotalComissao = g.Sum(x => x.ValorComissao),
                Pendente = g.Where(x => x.Status == StatusComissao.Pendente).Sum(x => x.ValorComissao),
                Aprovada = g.Where(x => x.Status == StatusComissao.Aprovada).Sum(x => x.ValorComissao),
                Paga = g.Where(x => x.Status == StatusComissao.Paga).Sum(x => x.ValorComissao)
            })
            .OrderByDescending(v => v.TotalComissao)
            .ToList();

        return new ComissaoResumoPeriodoDto
        {
            TotalPendente = ativas.Where(i => i.Status == StatusComissao.Pendente).Sum(i => i.ValorComissao),
            TotalAprovada = ativas.Where(i => i.Status == StatusComissao.Aprovada).Sum(i => i.ValorComissao),
            TotalPaga = ativas.Where(i => i.Status == StatusComissao.Paga).Sum(i => i.ValorComissao),
            TotalVendasBase = ativas.Sum(i => i.ValorBaseVenda),
            QuantidadeComissoes = ativas.Count,
            PorVendedor = porVendedor
        };
    }
}
