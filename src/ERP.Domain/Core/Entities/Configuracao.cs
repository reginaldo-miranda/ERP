using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities;

/// <summary>
/// Configuração por empresa no formato chave/valor.
/// Permite armazenar configurações personalizadas por empresa,
/// como preferências de exibição, parâmetros fiscais, etc.
/// </summary>
public class Configuracao : BaseEmpresaEntity
{
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Grupo { get; set; } // Ex: "Geral", "Fiscal", "Estoque"
    public string? TipoDado { get; set; } // Ex: "string", "int", "bool", "json"
}
