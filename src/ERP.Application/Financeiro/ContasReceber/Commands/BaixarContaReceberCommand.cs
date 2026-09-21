using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Financeiro.ContasReceber.DTOs;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.ContasReceber.Commands;

public record BaixarContaReceberCommand(
    int ContaReceberId,
    int ContaBancariaId,
    int FormaPagamentoId,
    DateTime DataBaixa,
    decimal ValorPrincipal,
    decimal ValorJuros = 0,
    decimal ValorMulta = 0,
    decimal ValorDesconto = 0,
    string? Observacoes = null
) : IRequest<Result<BaixaContaReceberDto>>;

public class BaixarContaReceberCommandValidator : AbstractValidator<BaixarContaReceberCommand>
{
    public BaixarContaReceberCommandValidator()
    {
        RuleFor(x => x.ContaReceberId).GreaterThan(0).WithMessage("Conta a receber inválida.");
        RuleFor(x => x.ContaBancariaId).GreaterThan(0).WithMessage("Conta bancária/caixa é obrigatório.");
        RuleFor(x => x.FormaPagamentoId).GreaterThan(0).WithMessage("Forma de pagamento é obrigatória.");
        RuleFor(x => x.ValorPrincipal).GreaterThan(0).WithMessage("O valor principal deve ser maior que zero.");
        RuleFor(x => x.ValorJuros).GreaterThanOrEqualTo(0).WithMessage("Juros não podem ser negativos.");
        RuleFor(x => x.ValorMulta).GreaterThanOrEqualTo(0).WithMessage("Multa não pode ser negativa.");
        RuleFor(x => x.ValorDesconto).GreaterThanOrEqualTo(0).WithMessage("Desconto não pode ser negativo.");
    }
}

public class BaixarContaReceberCommandHandler : IRequestHandler<BaixarContaReceberCommand, Result<BaixaContaReceberDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public BaixarContaReceberCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<BaixaContaReceberDto>> Handle(BaixarContaReceberCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;

        var contaReceber = await _context.ContasReceber
            .FirstOrDefaultAsync(c => c.Id == request.ContaReceberId, cancellationToken);

        if (contaReceber == null)
            return Result<BaixaContaReceberDto>.Falha("Conta a receber não encontrada.");

        if (contaReceber.Status == StatusContaFinanceira.Paga)
            return Result<BaixaContaReceberDto>.Falha("Esta conta já está totalmente recebida/quitada.");

        if (contaReceber.Status == StatusContaFinanceira.Cancelada)
            return Result<BaixaContaReceberDto>.Falha("Não é possível baixar uma conta cancelada.");

        if (request.ValorPrincipal > contaReceber.SaldoRestante)
            return Result<BaixaContaReceberDto>.Falha($"O valor principal (R$ {request.ValorPrincipal:N2}) não pode exceder o saldo restante (R$ {contaReceber.SaldoRestante:N2}).");

        var contaBancaria = await _context.ContasBancarias
            .FirstOrDefaultAsync(c => c.Id == request.ContaBancariaId, cancellationToken);

        if (contaBancaria == null)
            return Result<BaixaContaReceberDto>.Falha("Conta bancária/caixa não encontrada.");

        var formaPagamento = await _context.FormasPagamento
            .FirstOrDefaultAsync(f => f.Id == request.FormaPagamentoId, cancellationToken);

        if (formaPagamento == null)
            return Result<BaixaContaReceberDto>.Falha("Forma de pagamento não encontrada.");

        var valorTotalRecebido = request.ValorPrincipal + request.ValorJuros + request.ValorMulta - request.ValorDesconto;
        if (valorTotalRecebido < 0)
            return Result<BaixaContaReceberDto>.Falha("O valor total líquido não pode ser negativo.");

        // 1. Atualizar saldo da Conta Bancária (Entrada)
        contaBancaria.SaldoAtual += valorTotalRecebido;

        // 2. Criar Movimentação Financeira no Extrato
        var movimentacao = new MovimentacaoFinanceira
        {
            EmpresaId = empresaId,
            ContaBancariaId = contaBancaria.Id,
            Tipo = TipoOperacaoFinanceira.Entrada,
            DataMovimentacao = request.DataBaixa.ToUniversalTime(),
            Valor = valorTotalRecebido,
            Descricao = $"Rec: {contaReceber.Descricao}",
            ContaReceberId = contaReceber.Id,
            PlanoContaId = contaReceber.PlanoContaId,
            CentroCustoId = contaReceber.CentroCustoId,
            DocumentoReferencia = contaReceber.NumeroDocumento,
            Observacoes = request.Observacoes
        };
        _context.MovimentacoesFinanceiras.Add(movimentacao);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Criar registro de Baixa
        var baixa = new BaixaContaReceber
        {
            EmpresaId = empresaId,
            ContaReceberId = contaReceber.Id,
            ContaBancariaId = contaBancaria.Id,
            FormaPagamentoId = formaPagamento.Id,
            DataBaixa = request.DataBaixa.ToUniversalTime(),
            ValorPrincipal = request.ValorPrincipal,
            ValorJuros = request.ValorJuros,
            ValorMulta = request.ValorMulta,
            ValorDesconto = request.ValorDesconto,
            ValorTotalRecebido = valorTotalRecebido,
            Observacoes = request.Observacoes,
            MovimentacaoFinanceiraId = movimentacao.Id
        };
        _context.BaixasContasReceber.Add(baixa);

        // 4. Atualizar saldo e status do Título
        contaReceber.ValorRecebido += request.ValorPrincipal;
        contaReceber.SaldoRestante = Math.Max(0, contaReceber.SaldoRestante - request.ValorPrincipal);
        contaReceber.Status = contaReceber.SaldoRestante == 0
            ? StatusContaFinanceira.Paga
            : StatusContaFinanceira.ParcialmentePaga;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<BaixaContaReceberDto>.Ok(new BaixaContaReceberDto(
            baixa.Id,
            baixa.ContaReceberId,
            baixa.ContaBancariaId,
            contaBancaria.Descricao,
            baixa.FormaPagamentoId,
            formaPagamento.Nome,
            baixa.DataBaixa,
            baixa.ValorPrincipal,
            baixa.ValorJuros,
            baixa.ValorMulta,
            baixa.ValorDesconto,
            baixa.ValorTotalRecebido,
            baixa.Observacoes
        ));
    }
}
