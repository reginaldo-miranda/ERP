namespace ERP.Application.Vendas.Voice;

public enum VoiceSessionStatus
{
    AguardandoComando = 0,
    AguardandoCliente = 1,
    CriandoPedidoAguardandoItens = 2,
    AguardandoEscolhaProduto = 3,
    PedidoConcluidoAguardandoNfe = 4,
    NfeEmitida = 5,
    Encerrado = 6
}

public class VoiceProductCandidateDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public string UnidadeMedida { get; set; } = "UN";
}

public class VoiceClientCandidateDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
}

public class VoiceOrderItemDto
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public string ProdutoCodigo { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal => Math.Round(Quantidade * ValorUnitario, 2);
}

public class VoiceSessionState
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public VoiceSessionStatus Status { get; set; } = VoiceSessionStatus.AguardandoComando;
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public string? CondicaoPagamento { get; set; } = "30 dias";
    public int DiasVencimento { get; set; } = 30;
    public List<VoiceOrderItemDto> Itens { get; set; } = new();
    
    // Candidatos para desambiguação e seleção
    public List<VoiceProductCandidateDto> CandidatosProdutos { get; set; } = new();
    public decimal QuantidadePendente { get; set; } = 1;
    public List<VoiceClientCandidateDto> CandidatosClientes { get; set; } = new();
    public string? TermoClienteNaoEncontrado { get; set; }
    public string? TermoProdutoNaoEncontrado { get; set; }

    public int? PedidoCriadoId { get; set; }
    public string? PedidoCriadoNumero { get; set; }
    public decimal ValorTotal => Itens.Sum(i => i.ValorTotal);
}

public class VoiceChatMessageDto
{
    public string Remetente { get; set; } = "Assistente"; // "Usuario" ou "Assistente"
    public string Texto { get; set; } = string.Empty;
    public DateTime Horario { get; set; } = DateTime.Now;
    public bool EhErro { get; set; }
}

public class VoiceProcessResult
{
    public string MensagemResposta { get; set; } = string.Empty;
    public bool DeveFalar { get; set; } = true;
    public VoiceSessionState Estado { get; set; } = new();
    public bool PedidoGerado { get; set; }
    public bool NfeEmitida { get; set; }
    public int? NotaFiscalId { get; set; }
    public string? LinkDanfe { get; set; }
}

