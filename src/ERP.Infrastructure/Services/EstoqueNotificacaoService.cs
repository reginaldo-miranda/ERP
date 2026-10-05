using ERP.Domain.Core.Entities;
using ERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Serviço em segundo plano que monitora diariamente produtos com estoque abaixo do mínimo
/// e lotes vencidos ou próximos do vencimento (30 dias), gerando notificações para os usuários.
/// </summary>
public class EstoqueNotificacaoService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EstoqueNotificacaoService> _logger;

    public EstoqueNotificacaoService(
        IServiceScopeFactory scopeFactory,
        ILogger<EstoqueNotificacaoService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda 35 segundos na inicialização para deixar o app subir completamente
        await Task.Delay(TimeSpan.FromSeconds(35), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarAlertasEstoque(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar alertas de estoque e validade de lotes.");
            }

            // Executa uma vez a cada 24 horas
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task VerificarAlertasEstoque(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hoje = DateTime.UtcNow.Date;
        var em30Dias = hoje.AddDays(30);

        _logger.LogInformation("Verificando alertas de estoque mínimo e validade de lotes em {Data}...", hoje);

        var empresas = await context.Empresas
            .AsNoTracking()
            .Where(e => e.Ativo)
            .Select(e => e.Id)
            .ToListAsync(ct);

        foreach (var empresaId in empresas)
        {
            // 1. Produtos com estoque crítico (abaixo do mínimo)
            var produtosAbaixoMinimo = await context.EstoqueProdutos
                .IgnoreQueryFilters()
                .Include(ep => ep.Produto)
                .Where(ep => ep.EmpresaId == empresaId
                    && ep.EstoqueMinimo > 0
                    && ep.Quantidade <= ep.EstoqueMinimo)
                .ToListAsync(ct);

            // 2. Lotes vencidos ou vencendo nos próximos 30 dias com saldo em estoque
            var lotesCriticos = await context.Lotes
                .IgnoreQueryFilters()
                .Include(l => l.Produto)
                .Where(l => l.EmpresaId == empresaId
                    && l.Ativo
                    && l.QuantidadeAtual > 0
                    && l.DataValidade <= em30Dias)
                .ToListAsync(ct);

            if (!produtosAbaixoMinimo.Any() && !lotesCriticos.Any())
                continue;

            // Usuários vinculados a esta empresa
            var usuarioIds = await context.UsuarioEmpresas
                .IgnoreQueryFilters()
                .Where(ue => ue.EmpresaId == empresaId)
                .Select(ue => ue.UsuarioId)
                .ToListAsync(ct);

            // Evita duplicar notificações geradas no mesmo dia
            var jaNotificouEstoqueHoje = await context.Notificacoes
                .AnyAsync(n => n.EmpresaId == empresaId
                    && n.Modulo == "Estoque"
                    && n.CriadoEm.Date == hoje, ct);

            if (jaNotificouEstoqueHoje)
                continue;

            foreach (var usuarioId in usuarioIds)
            {
                if (produtosAbaixoMinimo.Any())
                {
                    context.Notificacoes.Add(new Notificacao
                    {
                        EmpresaId = empresaId,
                        UsuarioId = usuarioId,
                        Titulo = "Alerta: Estoque Mínimo Atingido",
                        Mensagem = $"Existem {produtosAbaixoMinimo.Count} item(ns) com saldo igual ou abaixo do estoque mínimo.",
                        Tipo = "Warning",
                        Modulo = "Estoque",
                        Link = "/estoque/posicao",
                        CriadoEm = DateTime.UtcNow
                    });
                }

                if (lotesCriticos.Any())
                {
                    var lotesVencidos = lotesCriticos.Count(l => l.DataValidade < hoje);
                    var lotesAVencer = lotesCriticos.Count(l => l.DataValidade >= hoje);

                    string detalhe = lotesVencidos > 0
                        ? $"{lotesVencidos} lote(s) vencido(s) e {lotesAVencer} vencendo nos próximos 30 dias."
                        : $"{lotesAVencer} lote(s) vencendo nos próximos 30 dias.";

                    context.Notificacoes.Add(new Notificacao
                    {
                        EmpresaId = empresaId,
                        UsuarioId = usuarioId,
                        Titulo = "Alerta de Validade de Lotes",
                        Mensagem = $"Atenção: {detalhe}",
                        Tipo = lotesVencidos > 0 ? "Error" : "Warning",
                        Modulo = "Estoque",
                        Link = "/estoque/lotes",
                        CriadoEm = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Notificações de estoque geradas para Empresa {EmpresaId}: {QtdMin} abaixo do mínimo, {QtdLotes} lotes críticos.",
                empresaId, produtosAbaixoMinimo.Count, lotesCriticos.Count);
        }
    }
}
