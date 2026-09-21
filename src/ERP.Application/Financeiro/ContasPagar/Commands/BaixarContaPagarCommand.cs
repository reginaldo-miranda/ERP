using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Financeiro.ContasPagar.DTOs;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.ContasPagar.Commands;

public record BaixarContaPagarCommand(
    int ContaPagarId,
    int ContaBancariaId,
    int FormaPagamentoId,
    DateTime DataBaixa,
    decimal ValorPrincipal,
    decimal ValorJuros = 0,
    decimal ValorMulta = 0,
    decimal ValorDesconto = 0,
    string? Observacoes = null
) : IRequest<Result<BaixaContaPagarDto>>;

public class BaixarContaPagarCommandValidator : AbstractValidator<BaixarContaPagarCommand>
{
    public BaixarContaPagarCommandValidator()
    {
        RuleFor(x => x.ContaPagarId).GreaterThan(0).WithMessage("Conta a pagar inválida.");
        RuleFor(x => x.ContaBancariaId).GreaterThan(0).WithMessage("Conta bancária/caixa é obrigatório.");
        RuleFor(x => x.FormaPagamentoId).GreaterThan(0).WithMessage("Forma de pagamento é obrigatória.");
        RuleFor(x => x.ValorPrincipal).GreaterThan(0).WithMessage("O valor principal deve ser maior que zero.");
        RuleFor(x => x.ValorJuros).GreaterThanOrEqualTo(0).WithMessage("Juros não podem ser negativos.");
        RuleFor(x => x.ValorMulta).GreaterThanOrEqualTo(0).WithMessage("Multa não pode ser negativa.");
        RuleFor(x => x.ValorDesconto).GreaterThanOrEqualTo(0).WithMessage("Desconto não pode ser negativo.");
    }
}

public class BaixarContaPagarCommandHandler : IRequestHandler<BaixarContaPagarCommand, Result<BaixaContaPagarDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public BaixarContaPagarCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<BaixaContaPagarDto>> Handle(BaixarContaPagarCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;

        var contaPagar = await _context.ContasPagar
            .FirstOrDefaultAsync(c => c.Id == request.ContaPagarId, cancellationToken);

        if (contaPagar == null)
            return Result<BaixaContaPagarDto>.Falha("Conta a pagar não encontrada.");

        if (contaPagar.Status == StatusContaFinanceira.Paga)
            return Result<BaixaContaPagarDto>.Falha("Esta conta já está totalmente quitada.");

        if (contaPagar.Status == StatusContaFinanceira.Cancelada)
            return Result<BaixaContaPagarDto>.Falha("Não é possível baixar uma conta cancelada.");

        if (request.ValorPrincipal > contaPagar.SaldoRestante)
            return Result<BaixaContaPagarDto>.Falha($"O valor principal (R$ {request.ValorPrincipal:N2}) não pode exceder o saldo restante (R$ {contaPagar.SaldoRestante:N2}).");

        var contaBancaria = await _context.ContasBancarias
            .FirstOrDefaultAsync(c => c.Id == request.ContaBancariaId, cancellationToken);

        if (contaBancaria == null)
            return Result<BaixaContaPagarDto>.Falha("Conta bancária/caixa não encontrada.");

        var formaPagamento = await _context.FormasPagamento
            .FirstOrDefaultAsync(f => f.Id == request.FormaPagamentoId, cancellationToken);

        if (formaPagamento == null)
            return Result<BaixaContaPagarDto>.Falha("Forma de pagamento não encontrada.");

        var valorTotalPago = request.ValorPrincipal + request.ValorJuros + request.ValorMulta - request.ValorDesconto;
        if (valorTotalPago < 0)
            return Result<BaixaContaPagarDto>.Falha("O valor total líquido não pode ser negativo.");

        // 1. Atualizar saldo da Conta Bancária (Saída)
        contaBancaria.SaldoAtual -= valorTotalPago;

        // 2. Criar Movimentação Financeira no Extrato
        var movimentacao = new MovimentacaoFinanceira
        {
            EmpresaId = empresaId,
            ContaBancariaId = contaBancaria.Id,
            Tipo = TipoOperacaoFinanceira.Saida,
            DataMovimentacao = request.DataBaixa.ToUniversalTime(),
            Valor = valorTotalPago,
            Descricao = $"Pgto: {contaPagar.Descricao}",
            ContaPagarId = contaPagar.Id,
            PlanoContaId = contaPagar.PlanoContaId,
            CentroCustoId = contaPagar.CentroCustoId,
            DocumentoReferencia = contaPagar.NumeroDocumento,
            Observacoes = request.Observacoes
        };
        _context.MovimentacoesFinanceiras.Add(movimentacao);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Criar registro de Baixa
        var baixa = new BaixaContaPagar
        {
            EmpresaId = empresaId,
            ContaPagarId = contaPagar.Id,
            ContaBancariaId = contaBancaria.Id,
            FormaPagamentoId = formaPagamento.Id,
            DataBaixa = request.DataBaixa.ToUniversalTime(),
            ValorPrincipal = request.ValorPrincipal,
            ValorJuros = request.ValorJuros,
            ValorMulta = request.ValorMulta,
            ValorDesconto = request.ValorDesconto,
            ValorTotalPago = valorTotalPago,
            Observacoes = request.Observacoes,
            MovimentacaoFinanceiraId = movimentacao.Id
        };
        _context.BaixasContasPagar.Add(baixa);

        // 4. Atualizar saldo e status do Título
        contaPagar.ValorPago += request.ValorPrincipal;
        contaPagar.SaldoRestante = Math.Max(0, contaPagar.SaldoRestante - request.ValorPrincipal);
        contaPagar.Status = contaPagar.SaldoRestante == 0
            ? StatusContaFinanceira.Paga
            : StatusContaFinanceira.ParcialmentePaga;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<BaixaContaPagarDto>.Ok(new BaixaContaPagarDto(
            baixa.Id,
            baixa.ContaPagarId,
            baixa.ContaBancariaId,
            contaBancaria.Descricao,
            baixa.FormaPagamentoId,
            formaPagamento.Nome,
            baixa.DataBaixa,
            baixa.ValorPrincipal,
            baixa.ValorJuros,
            baixa.ValorMulta,
            baixa.ValorDesconto,
            baixa.ValorTotalPago,
            baixa.Observacoes
        ));
    }
}
