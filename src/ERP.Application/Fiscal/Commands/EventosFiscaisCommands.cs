using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.Common;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Entities.Fiscal;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Fiscal.Commands;

public record CancelarNotaFiscalCommand(int Id, string Justificativa) : IRequest<CancelamentoFiscalResultDto>;

public class CancelarNotaFiscalCommandHandler : IRequestHandler<CancelarNotaFiscalCommand, CancelamentoFiscalResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;
    private readonly IFiscalService _fiscalService;

    public CancelarNotaFiscalCommandHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService,
        IFiscalService fiscalService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
        _fiscalService = fiscalService;
    }

    public async Task<CancelamentoFiscalResultDto> Handle(CancelarNotaFiscalCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var nota = await _context.NotasFiscais
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Nota Fiscal #{request.Id} não encontrada.");

        return await _fiscalService.CancelarNotaFiscalAsync(nota, request.Justificativa, cancellationToken);
    }
}

public record EnviarCartaCorrecaoCommand(int NotaFiscalId, string CorrecaoTexto) : IRequest<CartaCorrecaoResultDto>;

public class EnviarCartaCorrecaoCommandHandler : IRequestHandler<EnviarCartaCorrecaoCommand, CartaCorrecaoResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;
    private readonly IFiscalService _fiscalService;

    public EnviarCartaCorrecaoCommandHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService,
        IFiscalService fiscalService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
        _fiscalService = fiscalService;
    }

    public async Task<CartaCorrecaoResultDto> Handle(EnviarCartaCorrecaoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var nota = await _context.NotasFiscais
            .FirstOrDefaultAsync(n => n.Id == request.NotaFiscalId && n.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Nota Fiscal #{request.NotaFiscalId} não encontrada.");

        var cce = new CartaCorrecao
        {
            EmpresaId = empresaId,
            NotaFiscalId = nota.Id,
            CorrecaoTexto = request.CorrecaoTexto.Trim(),
            DataEvento = DateTime.UtcNow,
            Ativo = true
        };

        _context.CartasCorrecao.Add(cce);
        await _context.SaveChangesAsync(cancellationToken);

        return await _fiscalService.TransmitirCartaCorrecaoAsync(nota, cce, cancellationToken);
    }
}
