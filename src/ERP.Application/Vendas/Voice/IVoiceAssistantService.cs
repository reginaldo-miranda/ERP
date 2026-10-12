namespace ERP.Application.Vendas.Voice;

public interface IVoiceAssistantService
{
    Task<VoiceProcessResult> ProcessarComandoVozAsync(VoiceSessionState estadoAtual, string comandoVoz, CancellationToken cancellationToken = default);
    Task<VoiceProcessResult> FinalizarPedidoManualAsync(VoiceSessionState estadoAtual, CancellationToken cancellationToken = default);
    Task<VoiceProcessResult> EmitirNfeManualAsync(VoiceSessionState estadoAtual, CancellationToken cancellationToken = default);
}
