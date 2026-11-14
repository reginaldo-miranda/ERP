using ERP.Application.Common.Interfaces;
using ERP.Application.Contratos.DTOs;
using ERP.Domain.Core.Entities.Contratos;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Contratos.Commands;

public record CriarContratoCommand(SalvarContratoDto Dto) : IRequest<int>;
public record AtualizarContratoCommand(int Id, SalvarContratoDto Dto) : IRequest<bool>;
public record AlterarStatusContratoCommand(int Id, StatusContrato NovoStatus) : IRequest<bool>;
public record FaturarContratoCommand(int ContratoId) : IRequest<int>;
public record FaturarContratosVencidosCommand() : IRequest<int>;
public record AplicarReajusteContratoCommand(int ContratoId, decimal Percentual, string? Indice, string? Observacao) : IRequest<bool>;
public record RenovarContratoCommand(int ContratoId, int MesesAdicionais, decimal? PercentualReajuste = null) : IRequest<bool>;

public class ContratoCommandsHandler :
    IRequestHandler<CriarContratoCommand, int>,
    IRequestHandler<AtualizarContratoCommand, bool>,
    IRequestHandler<AlterarStatusContratoCommand, bool>,
    IRequestHandler<FaturarContratoCommand, int>,
    IRequestHandler<FaturarContratosVencidosCommand, int>,
    IRequestHandler<AplicarReajusteContratoCommand, bool>,
    IRequestHandler<RenovarContratoCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public ContratoCommandsHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<int> Handle(CriarContratoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        int ano = DateTime.UtcNow.Year;
        int totalAno = await _context.ContratosRecorrentes
            .CountAsync(c => c.EmpresaId == empresaId && c.CriadoEm.Year == ano, cancellationToken);
        string numero = $"CTR-{ano}-{(totalAno + 1):D4}";

        var contrato = new ContratoRecorrente
        {
            EmpresaId = empresaId,
            Numero = numero,
            Titulo = dto.Titulo.Trim(),
            Descricao = dto.Descricao?.Trim(),
            ClienteId = dto.ClienteId,
            ServicoId = dto.ServicoId,
            FormaPagamentoId = dto.FormaPagamentoId,
            Periodicidade = dto.Periodicidade,
            Status = dto.Status,
            DataInicio = dto.DataInicio.ToUniversalTime(),
            DataFim = dto.DataFim?.ToUniversalTime(),
            DiaVencimento = Math.Clamp(dto.DiaVencimento, 1, 28),
            ValorMensal = dto.ValorMensal,
            PercentualReajustePadrao = dto.PercentualReajustePadrao,
            IndiceReajuste = dto.IndiceReajuste?.Trim(),
            RenovacaoAutomatica = dto.RenovacaoAutomatica,
            ProximaCobranca = dto.ProximaCobranca.ToUniversalTime(),
            Observacoes = dto.Observacoes?.Trim()
        };

        _context.ContratosRecorrentes.Add(contrato);
        await _context.SaveChangesAsync(cancellationToken);
        return contrato.Id;
    }

    public async Task<bool> Handle(AtualizarContratoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var c = await _context.ContratosRecorrentes
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);
        if (c == null) return false;

        var dto = request.Dto;
        c.Titulo = dto.Titulo.Trim();
        c.Descricao = dto.Descricao?.Trim();
        c.ClienteId = dto.ClienteId;
        c.ServicoId = dto.ServicoId;
        c.FormaPagamentoId = dto.FormaPagamentoId;
        c.Periodicidade = dto.Periodicidade;
        c.Status = dto.Status;
        c.DataInicio = dto.DataInicio.ToUniversalTime();
        c.DataFim = dto.DataFim?.ToUniversalTime();
        c.DiaVencimento = Math.Clamp(dto.DiaVencimento, 1, 28);
        c.ValorMensal = dto.ValorMensal;
        c.PercentualReajustePadrao = dto.PercentualReajustePadrao;
        c.IndiceReajuste = dto.IndiceReajuste?.Trim();
        c.RenovacaoAutomatica = dto.RenovacaoAutomatica;
        c.ProximaCobranca = dto.ProximaCobranca.ToUniversalTime();
        c.Observacoes = dto.Observacoes?.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(AlterarStatusContratoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var c = await _context.ContratosRecorrentes
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);
        if (c == null) return false;

        c.Status = request.NovoStatus;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(FaturarContratoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var contrato = await _context.ContratosRecorrentes
            .FirstOrDefaultAsync(x => x.Id == request.ContratoId && x.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException("Contrato não encontrado.");

        if (contrato.Status != StatusContrato.Ativo)
            throw new InvalidOperationException("Somente contratos ativos podem ser faturados.");

        int contaReceberId = await FaturarCicloContratoAsync(contrato, empresaId, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return contaReceberId;
    }

    public async Task<int> Handle(FaturarContratosVencidosCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var hoje = DateTime.UtcNow.Date;

        var contratos = await _context.ContratosRecorrentes
            .Where(c => c.EmpresaId == empresaId && c.Status == StatusContrato.Ativo && c.ProximaCobranca.Date <= hoje)
            .ToListAsync(cancellationToken);

        int faturados = 0;
        foreach (var contrato in contratos)
        {
            if (contrato.DataFim.HasValue && contrato.ProximaCobranca.Date > contrato.DataFim.Value.Date)
            {
                if (contrato.RenovacaoAutomatica)
                {
                    contrato.DataFim = contrato.DataFim.Value.AddMonths(12);
                }
                else
                {
                    contrato.Status = StatusContrato.Encerrado;
                    continue;
                }
            }

            await FaturarCicloContratoAsync(contrato, empresaId, cancellationToken);
            faturados++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return faturados;
    }

    public async Task<bool> Handle(AplicarReajusteContratoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var contrato = await _context.ContratosRecorrentes
            .FirstOrDefaultAsync(x => x.Id == request.ContratoId && x.EmpresaId == empresaId, cancellationToken);
        if (contrato == null) return false;

        decimal valorAnterior = contrato.ValorMensal;
        decimal valorNovo = Math.Round(valorAnterior * (1m + request.Percentual / 100m), 2);

        contrato.ValorMensal = valorNovo;
        contrato.DataUltimoReajuste = DateTime.UtcNow;

        _context.ContratoReajustes.Add(new ContratoReajuste
        {
            EmpresaId = empresaId,
            ContratoRecorrenteId = contrato.Id,
            DataReajuste = DateTime.UtcNow,
            ValorAnterior = valorAnterior,
            ValorNovo = valorNovo,
            PercentualAplicado = request.Percentual,
            Indice = string.IsNullOrWhiteSpace(request.Indice) ? contrato.IndiceReajuste : request.Indice.Trim(),
            Observacao = request.Observacao?.Trim()
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(RenovarContratoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var contrato = await _context.ContratosRecorrentes
            .FirstOrDefaultAsync(x => x.Id == request.ContratoId && x.EmpresaId == empresaId, cancellationToken);
        if (contrato == null) return false;

        var baseData = contrato.DataFim ?? DateTime.UtcNow.Date;
        contrato.DataFim = baseData.AddMonths(Math.Max(1, request.MesesAdicionais));
        if (contrato.Status == StatusContrato.Encerrado)
            contrato.Status = StatusContrato.Ativo;

        if (request.PercentualReajuste.HasValue && request.PercentualReajuste.Value != 0)
        {
            decimal valorAnterior = contrato.ValorMensal;
            decimal valorNovo = Math.Round(valorAnterior * (1m + request.PercentualReajuste.Value / 100m), 2);
            contrato.ValorMensal = valorNovo;
            contrato.DataUltimoReajuste = DateTime.UtcNow;

            _context.ContratoReajustes.Add(new ContratoReajuste
            {
                EmpresaId = empresaId,
                ContratoRecorrenteId = contrato.Id,
                DataReajuste = DateTime.UtcNow,
                ValorAnterior = valorAnterior,
                ValorNovo = valorNovo,
                PercentualAplicado = request.PercentualReajuste.Value,
                Indice = contrato.IndiceReajuste,
                Observacao = $"Reajuste na renovação por +{request.MesesAdicionais} meses"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int> FaturarCicloContratoAsync(ContratoRecorrente contrato, int empresaId, CancellationToken cancellationToken)
    {
        var refData = contrato.ProximaCobranca.Date;
        int dia = Math.Clamp(contrato.DiaVencimento, 1, DateTime.DaysInMonth(refData.Year, refData.Month));
        var vencimento = new DateTime(refData.Year, refData.Month, dia, 0, 0, 0, DateTimeKind.Utc);
        string competencia = $"{refData.Month:D2}/{refData.Year}";

        var contaReceber = new ContaReceber
        {
            EmpresaId = empresaId,
            ClienteId = contrato.ClienteId,
            Descricao = $"Contrato {contrato.Numero} - {contrato.Titulo} ({competencia})",
            NumeroDocumento = $"{contrato.Numero}-{refData:yyyyMM}",
            ValorOriginal = contrato.ValorMensal,
            ValorRecebido = 0m,
            SaldoRestante = contrato.ValorMensal,
            DataEmissao = DateTime.UtcNow,
            DataVencimento = vencimento,
            DataCompetencia = refData.ToUniversalTime(),
            Status = StatusContaFinanceira.Pendente,
            FormaPagamentoId = contrato.FormaPagamentoId,
            NumeroParcela = 1,
            TotalParcelas = 1,
            Observacoes = $"Gerado automaticamente pelo Contrato Recorrente {contrato.Numero} (Competência {competencia})."
        };

        _context.ContasReceber.Add(contaReceber);
        await _context.SaveChangesAsync(cancellationToken);

        _context.ContratoFaturamentos.Add(new ContratoFaturamento
        {
            EmpresaId = empresaId,
            ContratoRecorrenteId = contrato.Id,
            Competencia = competencia,
            DataReferencia = refData.ToUniversalTime(),
            DataVencimento = vencimento,
            ValorFaturado = contrato.ValorMensal,
            ContaReceberId = contaReceber.Id
        });

        int mesesCiclo = Math.Max(1, (int)contrato.Periodicidade);
        contrato.DataUltimoFaturamento = DateTime.UtcNow;
        contrato.ProximaCobranca = refData.AddMonths(mesesCiclo).ToUniversalTime();

        return contaReceber.Id;
    }
}
