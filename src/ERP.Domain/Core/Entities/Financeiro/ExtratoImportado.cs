using System;
using System.Collections.Generic;
using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Financeiro;

public class ExtratoImportado : BaseEmpresaEntity, IAggregateRoot
{
    public int ContaBancariaId { get; set; }
    public ContaBancaria ContaBancaria { get; set; } = null!;

    public string NomeArquivo { get; set; } = string.Empty;
    public DateTime DataImportacao { get; set; } = DateTime.UtcNow;
    public DateTime DataInicioExtrato { get; set; }
    public DateTime DataFimExtrato { get; set; }
    
    public int TotalRegistros { get; set; }
    public decimal TotalCreditos { get; set; }
    public decimal TotalDebitos { get; set; }
    
    public string? Observacoes { get; set; }

    public ICollection<ExtratoImportadoItem> Itens { get; set; } = new List<ExtratoImportadoItem>();
}
