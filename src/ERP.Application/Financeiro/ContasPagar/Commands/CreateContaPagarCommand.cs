using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Financeiro.ContasPagar.DTOs;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using FluentValidation;
using MediatR;

namespace ERP.Application.Financeiro.ContasPagar.Commands;

public record CreateContaPagarCommand(
    int? FornecedorId,
    string Descricao,
    decimal ValorTotal,
    DateTime DataEmissao,
    DateTime PrimeiroVencimento,
    DateTime? DataCompetencia,
    int? FormaPagamentoId,
    int? ContaBancariaId,
    int? PlanoContaId,
    int? CentroCustoId,
    string? NumeroDocumento,
    int NumeroParcelas,
    int IntervaloDias,
    string? Observacoes
) : IRequest<Result<List<ContaPagarListItemDto>>>;

public class CreateContaPagarCommandValidator : AbstractValidator<CreateContaPagarCommand>
{
    public CreateContaPagarCommandValidator()
    {
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(250).WithMessage("Descrição é obrigatória.");
        RuleFor(x => x.ValorTotal).GreaterThan(0).WithMessage("O valor total deve ser maior que zero.");
        RuleFor(x => x.NumeroParcelas).GreaterThanOrEqualTo(1).WithMessage("O número de parcelas deve ser no mínimo 1.");
        RuleFor(x => x.IntervaloDias).GreaterThanOrEqualTo(1).When(x => x.NumeroParcelas > 1).WithMessage("Intervalo de dias deve ser maior que zero.");
    }
}

public class CreateContaPagarCommandHandler : IRequestHandler<CreateContaPagarCommand, Result<List<ContaPagarListItemDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateContaPagarCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<List<ContaPagarListItemDto>>> Handle(CreateContaPagarCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        var parcelas = new List<ContaPagar>();
        var idParcelamento = request.NumeroParcelas > 1 ? Guid.NewGuid() : (Guid?)null;

        var valorParcelaBase = Math.Round(request.ValorTotal / request.NumeroParcelas, 2);
        var diferencaCentavos = request.ValorTotal - (valorParcelaBase * request.NumeroParcelas);

        for (int i = 1; i <= request.NumeroParcelas; i++)
        {
            var valorParcela = valorParcelaBase;
            if (i == 1) valorParcela += diferencaCentavos; // Ajusta centavos na primeira parcela

            var dataVencimento = request.NumeroParcelas == 1
                ? request.PrimeiroVencimento
                : request.PrimeiroVencimento.AddDays((i - 1) * request.IntervaloDias);

            var descricaoParcela = request.NumeroParcelas > 1
                ? $"{request.Descricao.Trim()} ({i}/{request.NumeroParcelas})"
                : request.Descricao.Trim();

            var conta = new ContaPagar
            {
                EmpresaId = empresaId,
                FornecedorId = request.FornecedorId,
                Descricao = descricaoParcela,
                NumeroDocumento = request.NumeroDocumento?.Trim(),
                ValorOriginal = valorParcela,
                ValorPago = 0,
                SaldoRestante = valorParcela,
                DataEmissao = request.DataEmissao.ToUniversalTime(),
                DataVencimento = dataVencimento.ToUniversalTime(),
                DataCompetencia = request.DataCompetencia?.ToUniversalTime(),
                Status = StatusContaFinanceira.Pendente,
                FormaPagamentoId = request.FormaPagamentoId,
                ContaBancariaId = request.ContaBancariaId,
                PlanoContaId = request.PlanoContaId,
                CentroCustoId = request.CentroCustoId,
                NumeroParcela = i,
                TotalParcelas = request.NumeroParcelas,
                IdParcelamento = idParcelamento,
                Observacoes = request.Observacoes?.Trim()
            };

            parcelas.Add(conta);
        }

        await _context.ContasPagar.AddRangeAsync(parcelas, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var retorno = parcelas.Select(p => new ContaPagarListItemDto(
            p.Id,
            p.FornecedorId,
            null,
            p.Descricao,
            p.NumeroDocumento,
            p.ValorOriginal,
            p.ValorPago,
            p.SaldoRestante,
            p.DataEmissao,
            p.DataVencimento,
            p.Status,
            p.NumeroParcela,
            p.TotalParcelas,
            null,
            null,
            null,
            null,
            p.DataVencimento.Date < DateTime.UtcNow.Date && p.Status != StatusContaFinanceira.Paga
        )).ToList();

        return Result<List<ContaPagarListItemDto>>.Ok(retorno);
    }
}
