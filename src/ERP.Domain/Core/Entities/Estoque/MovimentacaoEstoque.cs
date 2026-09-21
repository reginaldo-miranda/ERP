using ERP.Domain.Common;
using ERP.Domain.Core.Entities.Cadastros;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Estoque;

/// <summary>
/// Registro histórico de qualquer movimentação de estoque (entrada, saída, ajuste, transferência).
/// </summary>
public class MovimentacaoEstoque : BaseEmpresaEntity, IAggregateRoot
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int DepositoOrigemId { get; set; }
    public Deposito DepositoOrigem { get; set; } = null!;

    public int? DepositoDestinoId { get; set; }
    public Deposito? DepositoDestino { get; set; }

    public TipoMovimentacaoEstoque Tipo { get; set; }
    public OrigemMovimentacaoEstoque Origem { get; set; }

    public decimal Quantidade { get; set; }
    public decimal CustoUnitario { get; set; }
    public decimal CustoTotal { get; set; }

    public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;
    public string? DocumentoOrigem { get; set; }
    public int? DocumentoOrigemId { get; set; }
    public string? Observacao { get; set; }
}
