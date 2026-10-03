using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.Conciliacao.Commands;

public record ConciliarItemCommand(
    int ExtratoImportadoItemId,
    int? MovimentacaoFinanceiraId = null,
    int? ContaPagarId = null,
    int? ContaReceberId = null
) : IRequest<Result>;

public record IgnorarItemExtratoCommand(
    int ExtratoImportadoItemId,
    string? Motivo = null
) : IRequest<Result>;

public record DesfazerConciliacaoCommand(
    int ExtratoImportadoItemId
) : IRequest<Result>;

public record CriarMovimentacaoDeExtratoCommand(
    int ExtratoImportadoItemId,
    int PlanoContaId,
    int? CentroCustoId = null
) : IRequest<Result>;

public class ConciliacaoCommandsHandler :
    IRequestHandler<ConciliarItemCommand, Result>,
    IRequestHandler<IgnorarItemExtratoCommand, Result>,
    IRequestHandler<DesfazerConciliacaoCommand, Result>,
    IRequestHandler<CriarMovimentacaoDeExtratoCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public ConciliacaoCommandsHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result> Handle(ConciliarItemCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        if (empresaId == 0) return Result.Falha("Nenhuma empresa ativa selecionada.");

        var item = await _context.ExtratosImportadosItens
            .Include(i => i.ExtratoImportado)
            .ThenInclude(e => e.ContaBancaria)
            .FirstOrDefaultAsync(i => i.Id == request.ExtratoImportadoItemId && i.EmpresaId == empresaId, cancellationToken);

        if (item == null)
            return Result.Falha("Item do extrato não encontrado.");

        if (item.StatusConciliacao == StatusConciliacao.Conciliado)
            return Result.Falha("Este item já foi conciliado.");

        var contaBancaria = item.ExtratoImportado.ContaBancaria;

        // Caso 1: Conciliar com Movimentação Financeira existente
        if (request.MovimentacaoFinanceiraId.HasValue)
        {
            var mov = await _context.MovimentacoesFinanceiras
                .FirstOrDefaultAsync(m => m.Id == request.MovimentacaoFinanceiraId.Value && m.EmpresaId == empresaId, cancellationToken);

            if (mov == null)
                return Result.Falha("Movimentação financeira não encontrada.");

            item.MovimentacaoFinanceiraId = mov.Id;
            item.StatusConciliacao = StatusConciliacao.Conciliado;
            item.DataConciliacao = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        // Caso 2: Conciliar com Conta a Pagar pendente (Baixa direta assistida)
        if (request.ContaPagarId.HasValue)
        {
            var contaPagar = await _context.ContasPagar
                .FirstOrDefaultAsync(c => c.Id == request.ContaPagarId.Value && c.EmpresaId == empresaId, cancellationToken);

            if (contaPagar == null)
                return Result.Falha("Conta a pagar não encontrada.");

            if (contaPagar.Status == StatusContaFinanceira.Paga || contaPagar.Status == StatusContaFinanceira.Cancelada)
                return Result.Falha("A conta a pagar selecionada já está quitada ou cancelada.");

            var valorBaixa = Math.Min(Math.Abs(item.Valor), contaPagar.SaldoRestante);

            // 1. Atualizar saldo da conta bancária (Saída)
            contaBancaria.SaldoAtual -= valorBaixa;

            // 2. Criar Movimentação Financeira
            var mov = new MovimentacaoFinanceira
            {
                EmpresaId = empresaId,
                ContaBancariaId = contaBancaria.Id,
                Tipo = TipoOperacaoFinanceira.Saida,
                DataMovimentacao = item.Data,
                Valor = valorBaixa,
                Descricao = $"Conciliação OFX: {contaPagar.Descricao}",
                ContaPagarId = contaPagar.Id,
                PlanoContaId = contaPagar.PlanoContaId,
                CentroCustoId = contaPagar.CentroCustoId,
                DocumentoReferencia = item.TransacaoId,
                Observacoes = $"Gerado via Conciliação Bancária - Extrato Item #{item.Id}"
            };
            _context.MovimentacoesFinanceiras.Add(mov);
            await _context.SaveChangesAsync(cancellationToken);

            var formaPagamentoPagarId = contaPagar.FormaPagamentoId ?? (await _context.FormasPagamento.Where(f => f.EmpresaId == empresaId).Select(f => f.Id).FirstOrDefaultAsync(cancellationToken));

            // 3. Registrar Baixa
            var baixa = new BaixaContaPagar
            {
                EmpresaId = empresaId,
                ContaPagarId = contaPagar.Id,
                ContaBancariaId = contaBancaria.Id,
                FormaPagamentoId = formaPagamentoPagarId,
                DataBaixa = item.Data,
                ValorPrincipal = valorBaixa,
                ValorTotalPago = valorBaixa,
                MovimentacaoFinanceiraId = mov.Id,
                Observacoes = $"Baixa automática via Conciliação OFX (Item #{item.Id})"
            };
            _context.BaixasContasPagar.Add(baixa);

            // 4. Atualizar Conta a Pagar
            contaPagar.ValorPago += valorBaixa;
            contaPagar.SaldoRestante -= valorBaixa;
            if (contaPagar.SaldoRestante <= 0)
            {
                contaPagar.SaldoRestante = 0;
                contaPagar.Status = StatusContaFinanceira.Paga;
            }
            else
            {
                contaPagar.Status = StatusContaFinanceira.ParcialmentePaga;
            }

            // 5. Vincular item do extrato
            item.MovimentacaoFinanceiraId = mov.Id;
            item.ContaPagarId = contaPagar.Id;
            item.StatusConciliacao = StatusConciliacao.Conciliado;
            item.DataConciliacao = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        // Caso 3: Conciliar com Conta a Receber pendente (Baixa direta assistida)
        if (request.ContaReceberId.HasValue)
        {
            var contaReceber = await _context.ContasReceber
                .FirstOrDefaultAsync(c => c.Id == request.ContaReceberId.Value && c.EmpresaId == empresaId, cancellationToken);

            if (contaReceber == null)
                return Result.Falha("Conta a receber não encontrada.");

            if (contaReceber.Status == StatusContaFinanceira.Paga || contaReceber.Status == StatusContaFinanceira.Cancelada)
                return Result.Falha("A conta a receber selecionada já está quitada ou cancelada.");

            var valorBaixa = Math.Min(Math.Abs(item.Valor), contaReceber.SaldoRestante);

            // 1. Atualizar saldo da conta bancária (Entrada)
            contaBancaria.SaldoAtual += valorBaixa;

            // 2. Criar Movimentação Financeira
            var mov = new MovimentacaoFinanceira
            {
                EmpresaId = empresaId,
                ContaBancariaId = contaBancaria.Id,
                Tipo = TipoOperacaoFinanceira.Entrada,
                DataMovimentacao = item.Data,
                Valor = valorBaixa,
                Descricao = $"Conciliação OFX: {contaReceber.Descricao}",
                ContaReceberId = contaReceber.Id,
                PlanoContaId = contaReceber.PlanoContaId,
                CentroCustoId = contaReceber.CentroCustoId,
                DocumentoReferencia = item.TransacaoId,
                Observacoes = $"Gerado via Conciliação Bancária - Extrato Item #{item.Id}"
            };
            _context.MovimentacoesFinanceiras.Add(mov);
            await _context.SaveChangesAsync(cancellationToken);

            var formaPagamentoReceberId = contaReceber.FormaPagamentoId ?? (await _context.FormasPagamento.Where(f => f.EmpresaId == empresaId).Select(f => f.Id).FirstOrDefaultAsync(cancellationToken));

            // 3. Registrar Baixa
            var baixa = new BaixaContaReceber
            {
                EmpresaId = empresaId,
                ContaReceberId = contaReceber.Id,
                ContaBancariaId = contaBancaria.Id,
                FormaPagamentoId = formaPagamentoReceberId,
                DataBaixa = item.Data,
                ValorPrincipal = valorBaixa,
                ValorTotalRecebido = valorBaixa,
                MovimentacaoFinanceiraId = mov.Id,
                Observacoes = $"Baixa automática via Conciliação OFX (Item #{item.Id})"
            };
            _context.BaixasContasReceber.Add(baixa);

            // 4. Atualizar Conta a Receber
            contaReceber.ValorRecebido += valorBaixa;
            contaReceber.SaldoRestante -= valorBaixa;
            if (contaReceber.SaldoRestante <= 0)
            {
                contaReceber.SaldoRestante = 0;
                contaReceber.Status = StatusContaFinanceira.Paga;
            }
            else
            {
                contaReceber.Status = StatusContaFinanceira.ParcialmentePaga;
            }

            // 5. Vincular item do extrato
            item.MovimentacaoFinanceiraId = mov.Id;
            item.ContaReceberId = contaReceber.Id;
            item.StatusConciliacao = StatusConciliacao.Conciliado;
            item.DataConciliacao = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        return Result.Falha("Informe uma Movimentação Financeira ou um Título a Pagar/Receber para conciliar.");
    }

    public async Task<Result> Handle(IgnorarItemExtratoCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        if (empresaId == 0) return Result.Falha("Nenhuma empresa ativa selecionada.");

        var item = await _context.ExtratosImportadosItens
            .FirstOrDefaultAsync(i => i.Id == request.ExtratoImportadoItemId && i.EmpresaId == empresaId, cancellationToken);

        if (item == null)
            return Result.Falha("Item do extrato não encontrado.");

        item.StatusConciliacao = StatusConciliacao.Ignorado;
        if (!string.IsNullOrWhiteSpace(request.Motivo))
            item.Observacoes = request.Motivo.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> Handle(DesfazerConciliacaoCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        if (empresaId == 0) return Result.Falha("Nenhuma empresa ativa selecionada.");

        var item = await _context.ExtratosImportadosItens
            .Include(i => i.ExtratoImportado)
            .ThenInclude(e => e.ContaBancaria)
            .FirstOrDefaultAsync(i => i.Id == request.ExtratoImportadoItemId && i.EmpresaId == empresaId, cancellationToken);

        if (item == null)
            return Result.Falha("Item do extrato não encontrado.");

        if (item.StatusConciliacao != StatusConciliacao.Conciliado)
            return Result.Falha("O item não está com status conciliado.");

        var contaBancaria = item.ExtratoImportado.ContaBancaria;

        // Reversão de Conta a Pagar baixada via extrato
        if (item.ContaPagarId.HasValue)
        {
            var cp = await _context.ContasPagar.FirstOrDefaultAsync(c => c.Id == item.ContaPagarId.Value && c.EmpresaId == empresaId, cancellationToken);
            if (cp != null && item.MovimentacaoFinanceiraId.HasValue)
            {
                var mov = await _context.MovimentacoesFinanceiras.FirstOrDefaultAsync(m => m.Id == item.MovimentacaoFinanceiraId.Value && m.EmpresaId == empresaId, cancellationToken);
                if (mov != null)
                {
                    contaBancaria.SaldoAtual += mov.Valor;
                    cp.ValorPago = Math.Max(0, cp.ValorPago - mov.Valor);
                    cp.SaldoRestante = Math.Min(cp.ValorOriginal, cp.SaldoRestante + mov.Valor);
                    cp.Status = cp.SaldoRestante >= cp.ValorOriginal ? StatusContaFinanceira.Pendente : StatusContaFinanceira.ParcialmentePaga;

                    var baixa = await _context.BaixasContasPagar
                        .Where(b => b.ContaPagarId == cp.Id && b.ContaBancariaId == contaBancaria.Id)
                        .OrderByDescending(b => b.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (baixa != null) _context.BaixasContasPagar.Remove(baixa);
                    _context.MovimentacoesFinanceiras.Remove(mov);
                }
            }
            item.ContaPagarId = null;
        }
        // Reversão de Conta a Receber baixada via extrato
        else if (item.ContaReceberId.HasValue)
        {
            var cr = await _context.ContasReceber.FirstOrDefaultAsync(c => c.Id == item.ContaReceberId.Value && c.EmpresaId == empresaId, cancellationToken);
            if (cr != null && item.MovimentacaoFinanceiraId.HasValue)
            {
                var mov = await _context.MovimentacoesFinanceiras.FirstOrDefaultAsync(m => m.Id == item.MovimentacaoFinanceiraId.Value && m.EmpresaId == empresaId, cancellationToken);
                if (mov != null)
                {
                    contaBancaria.SaldoAtual -= mov.Valor;
                    cr.ValorRecebido = Math.Max(0, cr.ValorRecebido - mov.Valor);
                    cr.SaldoRestante = Math.Min(cr.ValorOriginal, cr.SaldoRestante + mov.Valor);
                    cr.Status = cr.SaldoRestante >= cr.ValorOriginal ? StatusContaFinanceira.Pendente : StatusContaFinanceira.ParcialmentePaga;

                    var baixa = await _context.BaixasContasReceber
                        .Where(b => b.ContaReceberId == cr.Id && b.ContaBancariaId == contaBancaria.Id)
                        .OrderByDescending(b => b.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (baixa != null) _context.BaixasContasReceber.Remove(baixa);
                    _context.MovimentacoesFinanceiras.Remove(mov);
                }
            }
            item.ContaReceberId = null;
        }
        // Reversão de movimentação criada diretamente pelo extrato
        else if (item.MovimentacaoFinanceiraId.HasValue)
        {
            var mov = await _context.MovimentacoesFinanceiras.FirstOrDefaultAsync(m => m.Id == item.MovimentacaoFinanceiraId.Value && m.EmpresaId == empresaId, cancellationToken);
            if (mov != null && (mov.Observacoes?.Contains("Criado a partir do extrato") == true || mov.Observacoes?.Contains("Conciliação Bancária") == true))
            {
                if (mov.Tipo == TipoOperacaoFinanceira.Entrada)
                    contaBancaria.SaldoAtual -= mov.Valor;
                else
                    contaBancaria.SaldoAtual += mov.Valor;

                _context.MovimentacoesFinanceiras.Remove(mov);
            }
        }

        item.MovimentacaoFinanceiraId = null;
        item.DataConciliacao = null;
        item.StatusConciliacao = StatusConciliacao.Pendente;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> Handle(CriarMovimentacaoDeExtratoCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        if (empresaId == 0) return Result.Falha("Nenhuma empresa ativa selecionada.");

        var item = await _context.ExtratosImportadosItens
            .Include(i => i.ExtratoImportado)
            .ThenInclude(e => e.ContaBancaria)
            .FirstOrDefaultAsync(i => i.Id == request.ExtratoImportadoItemId && i.EmpresaId == empresaId, cancellationToken);

        if (item == null)
            return Result.Falha("Item do extrato não encontrado.");

        if (item.StatusConciliacao == StatusConciliacao.Conciliado)
            return Result.Falha("Item já está conciliado.");

        var contaBancaria = item.ExtratoImportado.ContaBancaria;
        var tipoOperacao = item.Valor >= 0 ? TipoOperacaoFinanceira.Entrada : TipoOperacaoFinanceira.Saida;
        var valorAbs = Math.Abs(item.Valor);

        if (tipoOperacao == TipoOperacaoFinanceira.Entrada)
            contaBancaria.SaldoAtual += valorAbs;
        else
            contaBancaria.SaldoAtual -= valorAbs;

        var mov = new MovimentacaoFinanceira
        {
            EmpresaId = empresaId,
            ContaBancariaId = contaBancaria.Id,
            Tipo = tipoOperacao,
            DataMovimentacao = item.Data,
            Valor = valorAbs,
            Descricao = item.Descricao,
            PlanoContaId = request.PlanoContaId,
            CentroCustoId = request.CentroCustoId,
            DocumentoReferencia = item.TransacaoId,
            Observacoes = "Criado a partir do extrato OFX"
        };
        _context.MovimentacoesFinanceiras.Add(mov);
        await _context.SaveChangesAsync(cancellationToken);

        item.MovimentacaoFinanceiraId = mov.Id;
        item.StatusConciliacao = StatusConciliacao.Conciliado;
        item.DataConciliacao = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
