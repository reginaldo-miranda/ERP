using ERP.Application.Common.Interfaces;
using ERP.Application.Financeiro.Cadastros.DTOs;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.Cadastros.Queries;

public record GetBancosQuery(bool SomenteAtivos = true) : IRequest<List<BancoDto>>;
public record GetContasBancariasQuery(bool SomenteAtivas = true) : IRequest<List<ContaBancariaDto>>;
public record GetFormasPagamentoQuery(bool SomenteAtivas = true) : IRequest<List<FormaPagamentoDto>>;
public record GetPlanosContasQuery(TipoPlanoConta? Tipo = null, bool? SomenteAnaliticas = null, bool SomenteAtivos = true) : IRequest<List<PlanoContaDto>>;
public record GetCentrosCustoQuery(bool SomenteAtivos = true) : IRequest<List<CentroCustoDto>>;

public class GetCadastrosFinanceirosQueryHandler :
    IRequestHandler<GetBancosQuery, List<BancoDto>>,
    IRequestHandler<GetContasBancariasQuery, List<ContaBancariaDto>>,
    IRequestHandler<GetFormasPagamentoQuery, List<FormaPagamentoDto>>,
    IRequestHandler<GetPlanosContasQuery, List<PlanoContaDto>>,
    IRequestHandler<GetCentrosCustoQuery, List<CentroCustoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCadastrosFinanceirosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BancoDto>> Handle(GetBancosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Bancos.AsNoTracking();
        if (request.SomenteAtivos) query = query.Where(b => b.Ativo);

        return await query.OrderBy(b => b.Codigo)
            .Select(b => new BancoDto(b.Id, b.Codigo, b.Nome, b.NomeReduzido, b.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ContaBancariaDto>> Handle(GetContasBancariasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ContasBancarias.AsNoTracking().Include(c => c.Banco).AsQueryable();
        if (request.SomenteAtivas) query = query.Where(c => c.Ativa);

        return await query.OrderBy(c => c.Descricao)
            .Select(c => new ContaBancariaDto(
                c.Id,
                c.Descricao,
                c.Tipo,
                c.BancoId,
                c.Banco != null ? c.Banco.NomeReduzido ?? c.Banco.Nome : null,
                c.Agencia,
                c.AgenciaDigito,
                c.Conta,
                c.ContaDigito,
                c.SaldoInicial,
                c.SaldoAtual,
                c.Ativa,
                c.Observacoes
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FormaPagamentoDto>> Handle(GetFormasPagamentoQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FormasPagamento.AsNoTracking().AsQueryable();
        if (request.SomenteAtivas) query = query.Where(f => f.Ativa);

        return await query.OrderBy(f => f.Nome)
            .Select(f => new FormaPagamentoDto(f.Id, f.Nome, f.Tipo, f.DiasCompensacao, f.TaxaPercentual, f.Ativa))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PlanoContaDto>> Handle(GetPlanosContasQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PlanosContas.AsNoTracking().Include(p => p.PlanoContaPai).AsQueryable();
        if (request.SomenteAtivos) query = query.Where(p => p.Ativo);
        if (request.Tipo.HasValue) query = query.Where(p => p.Tipo == request.Tipo.Value);
        if (request.SomenteAnaliticas.HasValue && request.SomenteAnaliticas.Value) query = query.Where(p => !p.Sintetica);

        return await query.OrderBy(p => p.Codigo)
            .Select(p => new PlanoContaDto(
                p.Id,
                p.Codigo,
                p.Descricao,
                p.Tipo,
                p.Sintetica,
                p.PlanoContaPaiId,
                p.PlanoContaPai != null ? p.PlanoContaPai.Descricao : null,
                p.Ativo
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CentroCustoDto>> Handle(GetCentrosCustoQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CentrosCusto.AsNoTracking().AsQueryable();
        if (request.SomenteAtivos) query = query.Where(c => c.Ativo);

        return await query.OrderBy(c => c.Codigo)
            .Select(c => new CentroCustoDto(c.Id, c.Codigo, c.Descricao, c.Ativo))
            .ToListAsync(cancellationToken);
    }
}
