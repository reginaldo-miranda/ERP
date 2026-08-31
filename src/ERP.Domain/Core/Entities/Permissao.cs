using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities;

/// <summary>
/// Permissão granular do sistema.
/// Formato: "Modulo.SubModulo.Acao" (ex: "Vendas.Pedidos.Criar")
/// </summary>
public class Permissao : BaseEntity
{
    public string Codigo { get; set; } = string.Empty; // Ex: "Vendas.Pedidos.Criar"
    public string Modulo { get; set; } = string.Empty;  // Ex: "Vendas"
    public string SubModulo { get; set; } = string.Empty; // Ex: "Pedidos"
    public string Acao { get; set; } = string.Empty;    // Ex: "Criar"
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    // Navegação
    public ICollection<PapelPermissao> PapelPermissoes { get; set; } = new List<PapelPermissao>();
}
