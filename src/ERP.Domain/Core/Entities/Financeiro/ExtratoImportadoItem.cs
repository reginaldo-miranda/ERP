using System;
using ERP.Domain.Common;
using ERP.Domain.Core.Enums;

namespace ERP.Domain.Core.Entities.Financeiro;

public class ExtratoImportadoItem : BaseEmpresaEntity
{
    public int ExtratoImportadoId { get; set; }
    public ExtratoImportado ExtratoImportado { get; set; } = null!;

    public string TransacaoId { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string TipoTransacao { get; set; } = string.Empty;
    
    public StatusConciliacao StatusConciliacao { get; set; } = StatusConciliacao.Pendente;
    
    public int? MovimentacaoFinanceiraId { get; set; }
    public MovimentacaoFinanceira? MovimentacaoFinanceira { get; set; }
    
    public int? ContaPagarId { get; set; }
    public ContaPagar? ContaPagar { get; set; }

    public int? ContaReceberId { get; set; }
    public ContaReceber? ContaReceber { get; set; }
    
    public DateTime? DataConciliacao { get; set; }
    public string? Observacoes { get; set; }
}
