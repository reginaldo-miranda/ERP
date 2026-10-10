using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Fiscal;

/// <summary>
/// Código Fiscal de Operações e Prestações (CFOP).
/// Tabela universal compartilhada entre empresas do ERP.
/// </summary>
public class Cfop : BaseAuditableEntity
{
    /// <summary>Código CFOP de 4 dígitos, ex.: "5102", "6102", "1102".</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Descrição sucinta da operação fiscal.</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Tipo de operação (Entrada ou Saída).</summary>
    public TipoOperacaoFiscal TipoOperacao { get; set; } = TipoOperacaoFiscal.Saida;

    /// <summary>Texto explicativo da aplicação oficial do CFOP.</summary>
    public string? Aplicacao { get; set; }

    /// <summary>Indica se a operação gera título a pagar/receber no Financeiro.</summary>
    public bool GeraFinanceiro { get; set; } = true;

    /// <summary>Indica se a operação movimenta o saldo físico de Estoque.</summary>
    public bool MovimentaEstoque { get; set; } = true;
}
