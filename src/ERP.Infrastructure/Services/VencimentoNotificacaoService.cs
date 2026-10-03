using ERP.Domain.Core.Entities;
using ERP.Domain.Core.Enums;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services;

public class VencimentoNotificacaoService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VencimentoNotificacaoService> _logger;

    public VencimentoNotificacaoService(
        IServiceScopeFactory scopeFactory,
        ILogger<VencimentoNotificacaoService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit on startup to let the app fully initialize
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarVencimentos(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar vencimentos financeiros.");
            }

            // Run once per day
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task VerificarVencimentos(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hoje = DateTime.UtcNow.Date;

        _logger.LogInformation("Verificando contas vencidas em {Data}...", hoje);

        // Get all empresas
        var empresas = await context.Empresas
            .AsNoTracking()
            .Where(e => e.Ativo)
            .Select(e => e.Id)
            .ToListAsync(ct);

        foreach (var empresaId in empresas)
        {
            // Contas a Pagar vencidas
            var contasPagarVencidas = await context.ContasPagar
                .IgnoreQueryFilters() // Bypass tenant filter
                .Where(c => c.EmpresaId == empresaId
                    && c.DataVencimento < hoje
                    && (c.Status == StatusContaFinanceira.Pendente || c.Status == StatusContaFinanceira.ParcialmentePaga))
                .ToListAsync(ct);

            // Contas a Receber vencidas
            var contasReceberVencidas = await context.ContasReceber
                .IgnoreQueryFilters()
                .Where(c => c.EmpresaId == empresaId
                    && c.DataVencimento < hoje
                    && (c.Status == StatusContaFinanceira.Pendente || c.Status == StatusContaFinanceira.ParcialmentePaga))
                .ToListAsync(ct);

            if (!contasPagarVencidas.Any() && !contasReceberVencidas.Any())
                continue;

            // Find users for this empresa
            var usuarioIds = await context.UsuarioEmpresas
                .IgnoreQueryFilters()
                .Where(ue => ue.EmpresaId == empresaId)
                .Select(ue => ue.UsuarioId)
                .ToListAsync(ct);

            // Check if we already sent notifications today for this empresa
            var jaNotificouHoje = await context.Notificacoes
                .AnyAsync(n => n.EmpresaId == empresaId
                    && n.Modulo == "Financeiro"
                    && n.Titulo.Contains("Vencida")
                    && n.CriadoEm.Date == hoje, ct);

            if (jaNotificouHoje)
                continue;

            foreach (var usuarioId in usuarioIds)
            {
                if (contasPagarVencidas.Any())
                {
                    var total = contasPagarVencidas.Sum(c => c.SaldoRestante);
                    context.Notificacoes.Add(new Notificacao
                    {
                        EmpresaId = empresaId,
                        UsuarioId = usuarioId,
                        Titulo = "Contas a Pagar Vencidas",
                        Mensagem = $"Existem {contasPagarVencidas.Count} conta(s) a pagar vencida(s) totalizando R$ {total:N2}.",
                        Tipo = "Warning",
                        Modulo = "Financeiro",
                        Link = "/financeiro/contas-pagar",
                        CriadoEm = DateTime.UtcNow
                    });
                }

                if (contasReceberVencidas.Any())
                {
                    var total = contasReceberVencidas.Sum(c => c.SaldoRestante);
                    context.Notificacoes.Add(new Notificacao
                    {
                        EmpresaId = empresaId,
                        UsuarioId = usuarioId,
                        Titulo = "Contas a Receber Vencidas",
                        Mensagem = $"Existem {contasReceberVencidas.Count} conta(s) a receber vencida(s) totalizando R$ {total:N2}.",
                        Tipo = "Warning",
                        Modulo = "Financeiro",
                        Link = "/financeiro/contas-receber",
                        CriadoEm = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Notificações de vencimento criadas para Empresa {EmpresaId}: {CP} a pagar, {CR} a receber.",
                empresaId, contasPagarVencidas.Count, contasReceberVencidas.Count);
        }
    }
}
