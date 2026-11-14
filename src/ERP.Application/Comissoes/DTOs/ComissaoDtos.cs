using ERP.Domain.Core.Enums;

namespace ERP.Application.Comissoes.DTOs;

public class VendedorDto
{
    public int Id { get; set; }
    public string? UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Cpf { get; set; }
    public TipoRegraComissao TipoComissaoPadrao { get; set; }
    public decimal PercentualPadrao { get; set; }
    public decimal ValorFixoPadrao { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; }
    public int TotalRegras { get; set; }
}

public class SalvarVendedorDto
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
    public bool Ativo { get; set; } = true;
}

public class RegraComissaoDto
{
    public int Id { get; set; }
    public int? VendedorId { get; set; }
    public string? VendedorNome { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoRegraComissao Tipo { get; set; }
    public decimal Percentual { get; set; }
    public decimal ValorFixo { get; set; }
    public decimal? ValorMinimoVenda { get; set; }
    public int Prioridade { get; set; }
    public bool Ativo { get; set; }
}

public class SalvarRegraComissaoDto
{
    public int? VendedorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoRegraComissao Tipo { get; set; } = TipoRegraComissao.PercentualSobreVenda;
    public decimal Percentual { get; set; }
    public decimal ValorFixo { get; set; }
    public decimal? ValorMinimoVenda { get; set; }
    public int Prioridade { get; set; } = 1;
    public bool Ativo { get; set; } = true;
}

public class ComissaoVendaDto
{
    public int Id { get; set; }
    public int VendedorId { get; set; }
    public string VendedorNome { get; set; } = string.Empty;
    public int PedidoVendaId { get; set; }
    public string PedidoNumero { get; set; } = string.Empty;
    public string ClienteNome { get; set; } = string.Empty;
    public int? FaturamentoId { get; set; }
    public string? FaturamentoNumero { get; set; }
    public string? RegraNome { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal ValorBaseVenda { get; set; }
    public decimal PercentualAplicado { get; set; }
    public decimal ValorComissao { get; set; }
    public StatusComissao Status { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string? Observacoes { get; set; }
}

public class ComissaoResumoPeriodoDto
{
    public decimal TotalPendente { get; set; }
    public decimal TotalAprovada { get; set; }
    public decimal TotalPaga { get; set; }
    public decimal TotalVendasBase { get; set; }
    public int QuantidadeComissoes { get; set; }
    public List<ComissaoPorVendedorDto> PorVendedor { get; set; } = new();
}

public class ComissaoPorVendedorDto
{
    public int VendedorId { get; set; }
    public string VendedorNome { get; set; } = string.Empty;
    public int QuantidadeVendas { get; set; }
    public decimal TotalVendido { get; set; }
    public decimal TotalComissao { get; set; }
    public decimal Pendente { get; set; }
    public decimal Aprovada { get; set; }
    public decimal Paga { get; set; }
}
