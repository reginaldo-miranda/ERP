using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Cadastros;

/// <summary>
/// Cadastro de clientes (PF ou PJ).
/// Suporte a múltiplos endereços e auditoria automática via BaseEmpresaEntity.
/// </summary>
public class Cliente : BaseEmpresaEntity, IAggregateRoot
{
    public string Nome { get; set; } = string.Empty;
    public TipoPessoa TipoPessoa { get; set; }
    public string CpfCnpj { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? NomeFantasia { get; set; }
    public string? Observacoes { get; set; }
    public decimal? LimiteCredito { get; set; }

    // Navegação
    public ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();
}
