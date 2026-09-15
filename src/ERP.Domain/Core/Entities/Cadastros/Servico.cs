using ERP.Domain.Common;

namespace ERP.Domain.Core.Entities.Cadastros;

/// <summary>
/// Cadastro de serviços prestados pela empresa.
/// </summary>
public class Servico : BaseEmpresaEntity, IAggregateRoot
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public decimal PrecoBase { get; set; }
    public string? Unidade { get; set; } // ex: Hora, Diária, Unidade

    // Dados fiscais de serviço
    public string? CodigoServicoCnae { get; set; }
    public string? CodigoServiceLc116 { get; set; }
    public decimal? AliquotaIss { get; set; }
    public string? Observacoes { get; set; }
}
