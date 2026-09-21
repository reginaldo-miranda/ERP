using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.ContasReceber.Commands;

public record CancelarContaReceberCommand(int Id) : IRequest<Result<bool>>;

public class CancelarContaReceberCommandHandler : IRequestHandler<CancelarContaReceberCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public CancelarContaReceberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(CancelarContaReceberCommand request, CancellationToken cancellationToken)
    {
        var conta = await _context.ContasReceber.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (conta == null) return Result<bool>.Falha("Conta a receber não encontrada.");

        if (conta.ValorRecebido > 0)
            return Result<bool>.Falha("Não é possível cancelar uma conta com recebimentos realizados. Estorne as baixas primeiro.");

        conta.Status = StatusContaFinanceira.Cancelada;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}

public record EstornarBaixaContaReceberCommand(int BaixaId) : IRequest<Result<bool>>;

public class EstornarBaixaContaReceberCommandHandler : IRequestHandler<EstornarBaixaContaReceberCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public EstornarBaixaContaReceberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(EstornarBaixaContaReceberCommand request, CancellationToken cancellationToken)
    {
        var baixa = await _context.BaixasContasReceber
            .Include(b => b.ContaReceber)
            .Include(b => b.ContaBancaria)
            .FirstOrDefaultAsync(b => b.Id == request.BaixaId, cancellationToken);

        if (baixa == null) return Result<bool>.Falha("Registro de baixa não encontrado.");

        // 1. Debitar saldo da conta bancária (reverter entrada)
        baixa.ContaBancaria.SaldoAtual -= baixa.ValorTotalRecebido;

        // 2. Restaurar saldo do título
        baixa.ContaReceber.ValorRecebido = Math.Max(0, baixa.ContaReceber.ValorRecebido - baixa.ValorPrincipal);
        baixa.ContaReceber.SaldoRestante += baixa.ValorPrincipal;
        baixa.ContaReceber.Status = baixa.ContaReceber.ValorRecebido > 0
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
        _context.BaixasContasReceber.Remove(baixa);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Ok(true);
    }
}
