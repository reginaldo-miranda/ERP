using ERP.Application.Common.Interfaces;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.Relatorios.Queries;

// DTOs
public record ContaVencidaDto(
    int Id,
    string Tipo, // "Pagar" or "Receber"
    string Descricao,
    string? Pessoa, // Fornecedor ou Cliente
    decimal ValorOriginal,
    decimal SaldoRestante,
    DateTime DataVencimento,
    int DiasAtraso,
    StatusContaFinanceira Status,
    string? PlanoConta
);

public record RelatorioVencimentosDto(
    List<ContaVencidaDto> ContasVencidas,
    List<ContaVencidaDto> ContasAVencer7Dias,
    List<ContaVencidaDto> ContasAVencer15Dias,
    List<ContaVencidaDto> ContasAVencer30Dias,
    ResumoVencimentosDto Resumo
);

public record ResumoVencimentosDto(
    int TotalVencidas,
    decimal ValorTotalVencido,
    int TotalAVencer7Dias,
    decimal ValorAVencer7Dias,
    int TotalAVencer15Dias,
    decimal ValorAVencer15Dias,
    int TotalAVencer30Dias,
    decimal ValorAVencer30Dias,
    int TotalContasPagarVencidas,
    decimal ValorContasPagarVencidas,
    int TotalContasReceberVencidas,
    decimal ValorContasReceberVencidas
);

// Query
public record GetRelatorioVencimentosQuery(
    string? TipoFiltro = null, // "Pagar", "Receber", or null for both
    int? DiasAVencer = null
) : IRequest<RelatorioVencimentosDto>;

// Handler
public class GetRelatorioVencimentosQueryHandler : IRequestHandler<GetRelatorioVencimentosQuery, RelatorioVencimentosDto>
{
    private readonly IApplicationDbContext _context;

    public GetRelatorioVencimentosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RelatorioVencimentosDto> Handle(GetRelatorioVencimentosQuery request, CancellationToken cancellationToken)
    {
        var hoje = DateTime.UtcNow.Date;
        var em7Dias = hoje.AddDays(7);
        var em15Dias = hoje.AddDays(15);
        var em30Dias = hoje.AddDays(30);

        var contasPagar = await _context.ContasPagar
            .AsNoTracking()
            .Include(c => c.Fornecedor)
            .Include(c => c.PlanoConta)
            .Where(c => c.Status != StatusContaFinanceira.Paga && c.Status != StatusContaFinanceira.Cancelada)
            .Where(c => c.DataVencimento <= em30Dias)
            .ToListAsync(cancellationToken);

        var contasReceber = await _context.ContasReceber
            .AsNoTracking()
            .Include(c => c.Cliente)
            .Include(c => c.PlanoConta)
            .Where(c => c.Status != StatusContaFinanceira.Paga && c.Status != StatusContaFinanceira.Cancelada)
            .Where(c => c.DataVencimento <= em30Dias)
            .ToListAsync(cancellationToken);

        var todas = new List<ContaVencidaDto>();

        if (request.TipoFiltro == null || request.TipoFiltro == "Pagar")
        {
            foreach (var cp in contasPagar)
            {
                todas.Add(new ContaVencidaDto(
                    cp.Id, "Pagar", cp.Descricao,
                    cp.Fornecedor?.RazaoSocial,
                    cp.ValorOriginal, cp.SaldoRestante,
                    cp.DataVencimento,
                    cp.DataVencimento < hoje ? (hoje - cp.DataVencimento).Days : 0,
                    cp.Status,
                    cp.PlanoConta?.Descricao
                ));
            }
        }

        if (request.TipoFiltro == null || request.TipoFiltro == "Receber")
        {
            foreach (var cr in contasReceber)
            {
                todas.Add(new ContaVencidaDto(
                    cr.Id, "Receber", cr.Descricao,
                    cr.Cliente?.Nome,
                    cr.ValorOriginal, cr.SaldoRestante,
                    cr.DataVencimento,
                    cr.DataVencimento < hoje ? (hoje - cr.DataVencimento).Days : 0,
                    cr.Status,
                    cr.PlanoConta?.Descricao
                ));
            }
        }

        var vencidas = todas.Where(c => c.DataVencimento < hoje).OrderBy(c => c.DataVencimento).ToList();
        var aVencer7 = todas.Where(c => c.DataVencimento >= hoje && c.DataVencimento <= em7Dias).OrderBy(c => c.DataVencimento).ToList();
        var aVencer15 = todas.Where(c => c.DataVencimento > em7Dias && c.DataVencimento <= em15Dias).OrderBy(c => c.DataVencimento).ToList();
        var aVencer30 = todas.Where(c => c.DataVencimento > em15Dias && c.DataVencimento <= em30Dias).OrderBy(c => c.DataVencimento).ToList();

        var cpVencidas = vencidas.Where(c => c.Tipo == "Pagar").ToList();
        var crVencidas = vencidas.Where(c => c.Tipo == "Receber").ToList();

        var resumo = new ResumoVencimentosDto(
            TotalVencidas: vencidas.Count,
            ValorTotalVencido: vencidas.Sum(c => c.SaldoRestante),
            TotalAVencer7Dias: aVencer7.Count,
            ValorAVencer7Dias: aVencer7.Sum(c => c.SaldoRestante),
            TotalAVencer15Dias: aVencer15.Count,
            ValorAVencer15Dias: aVencer15.Sum(c => c.SaldoRestante),
            TotalAVencer30Dias: aVencer30.Count,
            ValorAVencer30Dias: aVencer30.Sum(c => c.SaldoRestante),
            TotalContasPagarVencidas: cpVencidas.Count,
            ValorContasPagarVencidas: cpVencidas.Sum(c => c.SaldoRestante),
            TotalContasReceberVencidas: crVencidas.Count,
            ValorContasReceberVencidas: crVencidas.Sum(c => c.SaldoRestante)
        );

        return new RelatorioVencimentosDto(
            ContasVencidas: vencidas,
            ContasAVencer7Dias: aVencer7,
            ContasAVencer15Dias: aVencer15,
            ContasAVencer30Dias: aVencer30,
            Resumo: resumo
        );
    }
}
