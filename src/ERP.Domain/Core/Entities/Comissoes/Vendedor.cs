using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Comissoes;

/// <summary>
/// Cadastro de vendedor e suas condições padrão de comissão.
/// Pode ser vinculado a um usuário do sistema (Identity) para integração com CRM e Vendas.
/// </summary>
public class Vendedor : BaseEmpresaEntity, IAggregateRoot
{
    public string? UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Cpf { get; set; }

    public TipoRegraComissao TipoComissaoPadrao { get; set; } = TipoRegraComissao.PercentualSobreVenda;
    public decimal PercentualPadrao { get; set; } = 5.00m;
    public decimal ValorFixoPadrao { get; set; }

    public string? Observacoes { get; set; }

    public ICollection<RegraComissao> Regras { get; set; } = new List<RegraComissao>();
    public ICollection<ComissaoVenda> Comissoes { get; set; } = new List<ComissaoVenda>();
}
