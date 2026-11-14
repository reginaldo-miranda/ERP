using ERP.Application.Common.Interfaces;
using ERP.Application.Contratos.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Contratos.Queries;

public record ListarContratosQuery(
    string? Busca = null,
    StatusContrato? Status = null,
    PeriodicidadeContrato? Periodicidade = null
) : IRequest<List<ContratoRecorrenteDto>>;

public record ObterContratoPorIdQuery(int Id) : IRequest<ContratoDetalhesDto?>;
public record ObterResumoKpiContratosQuery() : IRequest<ContratoResumoKpiDto>;

public class ContratoQueriesHandler :
    IRequestHandler<ListarContratosQuery, List<ContratoRecorrenteDto>>,
    IRequestHandler<ObterContratoPorIdQuery, ContratoDetalhesDto?>,
    IRequestHandler<ObterResumoKpiContratosQuery, ContratoResumoKpiDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public ContratoQueriesHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<List<ContratoRecorrenteDto>> Handle(ListarContratosQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var query = _context.ContratosRecorrentes
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId);

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.Periodicidade.HasValue)
            query = query.Where(c => c.Periodicidade == request.Periodicidade.Value);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(c =>
                c.Numero.ToLower().Contains(termo) ||
                c.Titulo.ToLower().Contains(termo) ||
                c.Cliente.Nome.ToLower().Contains(termo));
        }

        return await query
            .OrderBy(c => c.Status)
            .ThenBy(c => c.ProximaCobranca)
            .Select(c => new ContratoRecorrenteDto
            {
                Id = c.Id,
                Numero = c.Numero,
                Titulo = c.Titulo,
                Descricao = c.Descricao,
                ClienteId = c.ClienteId,
                ClienteNome = c.Cliente.Nome,
                ServicoId = c.ServicoId,
                ServicoNome = c.Servico != null ? c.Servico.Nome : null,
                FormaPagamentoId = c.FormaPagamentoId,
                FormaPagamentoNome = c.FormaPagamento != null ? c.FormaPagamento.Nome : null,
                Periodicidade = c.Periodicidade,
                Status = c.Status,
                DataInicio = c.DataInicio,
                DataFim = c.DataFim,
                DiaVencimento = c.DiaVencimento,
                ValorMensal = c.ValorMensal,
                PercentualReajustePadrao = c.PercentualReajustePadrao,
                IndiceReajuste = c.IndiceReajuste,
                RenovacaoAutomatica = c.RenovacaoAutomatica,
                ProximaCobranca = c.ProximaCobranca,
                DataUltimoFaturamento = c.DataUltimoFaturamento,
                DataUltimoReajuste = c.DataUltimoReajuste,
                Observacoes = c.Observacoes,
                TotalFaturamentos = c.Faturamentos.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ContratoDetalhesDto?> Handle(ObterContratoPorIdQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var c = await _context.ContratosRecorrentes
            .AsNoTracking()
            .Include(x => x.Cliente)
            .Include(x => x.Servico)
            .Include(x => x.FormaPagamento)
            .Include(x => x.Faturamentos)
            .Include(x => x.Reajustes)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);

        if (c == null) return null;

        return new ContratoDetalhesDto
        {
            Id = c.Id,
            Numero = c.Numero,
            Titulo = c.Titulo,
            Descricao = c.Descricao,
            ClienteId = c.ClienteId,
            ClienteNome = c.Cliente.Nome,
            ServicoId = c.ServicoId,
            ServicoNome = c.Servico?.Nome,
            FormaPagamentoId = c.FormaPagamentoId,
            FormaPagamentoNome = c.FormaPagamento?.Nome,
            Periodicidade = c.Periodicidade,
            Status = c.Status,
            DataInicio = c.DataInicio,
            DataFim = c.DataFim,
            DiaVencimento = c.DiaVencimento,
            ValorMensal = c.ValorMensal,
            PercentualReajustePadrao = c.PercentualReajustePadrao,
            IndiceReajuste = c.IndiceReajuste,
            RenovacaoAutomatica = c.RenovacaoAutomatica,
            ProximaCobranca = c.ProximaCobranca,
            DataUltimoFaturamento = c.DataUltimoFaturamento,
            DataUltimoReajuste = c.DataUltimoReajuste,
            Observacoes = c.Observacoes,
            TotalFaturamentos = c.Faturamentos.Count,
            Faturamentos = c.Faturamentos
                .OrderByDescending(f => f.DataReferencia)
                .Select(f => new ContratoFaturamentoDto
                {
                    Id = f.Id,
                    Competencia = f.Competencia,
                    DataReferencia = f.DataReferencia,
                    DataVencimento = f.DataVencimento,
                    ValorFaturado = f.ValorFaturado,
                    ContaReceberId = f.ContaReceberId
                }).ToList(),
            Reajustes = c.Reajustes
                .OrderByDescending(r => r.DataReajuste)
                .Select(r => new ContratoReajusteDto
                {
                    Id = r.Id,
                    DataReajuste = r.DataReajuste,
                    ValorAnterior = r.ValorAnterior,
                    ValorNovo = r.ValorNovo,
                    PercentualAplicado = r.PercentualAplicado,
                    Indice = r.Indice,
                    Observacao = r.Observacao
                }).ToList()
        };
    }

    public async Task<ContratoResumoKpiDto> Handle(ObterResumoKpiContratosQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var ativos = await _context.ContratosRecorrentes
            .AsNoTracking()
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusContrato.Ativo)
            .Select(c => new { c.ValorMensal, c.Periodicidade, c.ProximaCobranca })
            .ToListAsync(cancellationToken);

        decimal faturadoMes = await _context.ContratoFaturamentos
            .AsNoTracking()
            .Where(f => f.EmpresaId == empresaId && f.CriadoEm >= inicioMes)
            .SumAsync(f => f.ValorFaturado, cancellationToken);

        // MRR normaliza contratos trimestrais/anuais para equivalente mensal
        decimal mrr = ativos.Sum(c =>
        {
            int meses = Math.Max(1, (int)c.Periodicidade);
            return Math.Round(c.ValorMensal / meses, 2);
        });

        return new ContratoResumoKpiDto
        {
            TotalAtivos = ativos.Count,
            MrrEstimado = mrr,
            PendentesFaturamento = ativos.Count(c => c.ProximaCobranca.Date <= hoje),
            FaturadoNoMes = faturadoMes
        };
    }
}
