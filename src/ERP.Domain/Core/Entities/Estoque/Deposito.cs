using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Estoque;

/// <summary>
/// Depósito ou armazém para estocagem física de produtos por empresa.
/// </summary>
public class Deposito : BaseEmpresaEntity, IAggregateRoot
{
    public string Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Endereco { get; set; }
    public string? Responsavel { get; set; }
    public bool Ativo { get; set; } = true;
    public bool Padrao { get; set; } = false;

    // Relacionamentos
    public ICollection<EstoqueProduto> EstoquesProdutos { get; set; } = new List<EstoqueProduto>();
}
