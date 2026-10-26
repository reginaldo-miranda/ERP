using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Compras;

/// <summary>
/// Registro de Entrada e Recebimento de Mercadorias (conferência física da NF).
/// Ao ser confirmada:
/// 1. Incrementa o saldo do estoque e recalcula o Custo Médio Ponderado.
/// 2. Cria ou alimenta lotes e números de série informados.
/// 3. Gera os títulos em Contas a Pagar no Financeiro com rateio exato de centavos.
/// </summary>
public class EntradaMercadoria : BaseEmpresaEntity, IAggregateRoot
{
    /// <summary>Número sequencial formatado, ex.: REC-2026-000001.</summary>
    public string Numero { get; set; } = string.Empty;

    public int? PedidoCompraId { get; set; }
    public PedidoCompra? PedidoCompra { get; set; }

    public int FornecedorId { get; set; }
    public Fornecedor Fornecedor { get; set; } = null!;

    public int DepositoId { get; set; }
    public Deposito Deposito { get; set; } = null!;

    // Dados da Nota Fiscal do Fornecedor
    public string NumeroNotaFiscal { get; set; } = string.Empty;
    public string? SerieNotaFiscal { get; set; }
    public string? ChaveAcessoNfe { get; set; }
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
    public DateTime DataEntrada { get; set; } = DateTime.UtcNow;

    public StatusEntradaMercadoria Status { get; set; } = StatusEntradaMercadoria.Conferida;
    public string? ConferenteNome { get; set; }

    // Valores da NF
    public decimal ValorProdutos { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorSeguro { get; set; }
    public decimal ValorOutrasDespesas { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    /// <summary>Agrupa os títulos gerados no Contas a Pagar para este recebimento.</summary>
    public Guid? IdParcelamento { get; set; }

    public string? Observacoes { get; set; }

    public DateTime? DataCancelamento { get; set; }
    public string? MotivoCancelamento { get; set; }

    /// <summary>Conteúdo integral do XML da NF-e importada para auditoria fiscal e consulta.</summary>
    public string? XmlNfeOriginal { get; set; }

    public ICollection<EntradaMercadoriaItem> Itens { get; set; } = new List<EntradaMercadoriaItem>();
}
