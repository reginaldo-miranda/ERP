namespace ERP.Application.Vendas.Voice;

public interface IVoiceAssistantService
{
    Task<VoiceProcessResult> ProcessarComandoVozAsync(VoiceSessionState estadoAtual, string comandoVoz, CancellationToken cancellationToken = default);
    Task<VoiceProcessResult> SelecionarProdutoCandidatoAsync(VoiceSessionState estadoAtual, int produtoId, decimal quantidade, CancellationToken cancellationToken = default);
    Task<VoiceProcessResult> SelecionarClienteCandidatoAsync(VoiceSessionState estadoAtual, int clienteId, CancellationToken cancellationToken = default);
    Task<VoiceProcessResult> RemoverItemPedidoAsync(VoiceSessionState estadoAtual, int produtoId);
    Task<VoiceProcessResult> AlterarQuantidadeItemAsync(VoiceSessionState estadoAtual, int produtoId, decimal novaQuantidade);
    Task<VoiceProcessResult> CadastrarClienteRapidoAsync(VoiceSessionState estadoAtual, string nome, string? telefone, string? cpf, CancellationToken cancellationToken = default);
    Task<VoiceProcessResult> FinalizarPedidoManualAsync(VoiceSessionState estadoAtual, CancellationToken cancellationToken = default);
    Task<VoiceProcessResult> EmitirNfeManualAsync(VoiceSessionState estadoAtual, CancellationToken cancellationToken = default);
}

