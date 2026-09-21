using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.ContasPagar.Commands;

public record CancelarContaPagarCommand(int Id) : IRequest<Result<bool>>;

public class CancelarContaPagarCommandHandler : IRequestHandler<CancelarContaPagarCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public CancelarContaPagarCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(CancelarContaPagarCommand request, CancellationToken cancellationToken)
    {
        var conta = await _context.ContasPagar.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (conta == null) return Result<bool>.Falha("Conta a pagar não encontrada.");

        if (conta.ValorPago > 0)
            return Result<bool>.Falha("Não é possível cancelar uma conta com pagamentos realizados. Estorne as baixas primeiro.");

        conta.Status = StatusContaFinanceira.Cancelada;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}

public record EstornarBaixaContaPagarCommand(int BaixaId) : IRequest<Result<bool>>;

public class EstornarBaixaContaPagarCommandHandler : IRequestHandler<EstornarBaixaContaPagarCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public EstornarBaixaContaPagarCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(EstornarBaixaContaPagarCommand request, CancellationToken cancellationToken)
    {
        var baixa = await _context.BaixasContasPagar
            .Include(b => b.ContaPagar)
            .Include(b => b.ContaBancaria)
            .FirstOrDefaultAsync(b => b.Id == request.BaixaId, cancellationToken);

        if (baixa == null) return Result<bool>.Falha("Registro de baixa não encontrado.");

        // 1. Devolver saldo para a conta bancária
        baixa.ContaBancaria.SaldoAtual += baixa.ValorTotalPago;

        // 2. Restaurar saldo do título
        baixa.ContaPagar.ValorPago = Math.Max(0, baixa.ContaPagar.ValorPago - baixa.ValorPrincipal);
        baixa.ContaPagar.SaldoRestante += baixa.ValorPrincipal;
        baixa.ContaPagar.Status = baixa.ContaPagar.ValorPago > 0
            ? StatusContaFinanceira.ParcialmentePaga
            : StatusContaFinanceira.Pendente;

        // 3. Remover movimentação vinculada
        if (baixa.MovimentacaoFinanceiraId.HasValue)
        {
            var mov = await _context.MovimentacoesFinanceiras
                .FirstOrDefaultAsync(m => m.Id == baixa.MovimentacaoFinanceiraId.Value, cancellationToken);
            if (mov != null) _context.MovimentacoesFinanceiras.Remove(mov);
        }

        // 4. Remover registro de baixa
        _context.BaixasContasPagar.Remove(baixa);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
