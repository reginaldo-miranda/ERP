using ERP.Domain.Core.Enums;

namespace ERP.Application.Pdv.DTOs;

public class TerminalPdvDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoTerminalPdv Tipo { get; set; }
    public int? DepositoId { get; set; }
    public string? DepositoNome { get; set; }
    public int? ContaBancariaId { get; set; }
    public string? ContaBancariaNome { get; set; }
    public string? ImpressoraCupom { get; set; }
    public bool Ativo { get; set; }
    public bool PossuiSessaoAberta { get; set; }
    public string? OperadorAtual { get; set; }
}

public class SalvarTerminalPdvDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoTerminalPdv Tipo { get; set; } = TipoTerminalPdv.Hibrido;
    public int? DepositoId { get; set; }
    public int? ContaBancariaId { get; set; }
    public string? ImpressoraCupom { get; set; }
    public bool Ativo { get; set; } = true;
}

public class SessaoCaixaPdvDto
{
    public int Id { get; set; }
    public int TerminalPdvId { get; set; }
    public string TerminalNome { get; set; } = string.Empty;
    public string OperadorNome { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public decimal ValorAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public decimal? ValorFechamentoInformado { get; set; }
    public decimal? ValorFechamentoCalculado { get; set; }
    public decimal? DiferencaQuebra { get; set; }
    public StatusSessaoCaixa Status { get; set; }
    public decimal TotalVendas { get; set; }
    public int QuantidadeVendas { get; set; }
    public decimal TotalSuprimentos { get; set; }
    public decimal TotalSangrias { get; set; }
    public decimal SaldoEsperado => ValorAbertura + TotalVendas + TotalSuprimentos - TotalSangrias;
}

public class ProdutoPdvDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public string? ImagemUrl { get; set; }
    public string? UnidadeSigla { get; set; }
}

public class VendaPdvItemInputDto
{
    public int ProdutoId { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public string NomeProduto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorDesconto { get; set; }
    public string? ImagemUrl { get; set; }
}

public class VendaPdvPagamentoInputDto
{
    public int? FormaPagamentoId { get; set; }
    public string DescricaoForma { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public class SalvarVendaPdvDto
{
    public int? PreVendaId { get; set; }
    public int TerminalPdvId { get; set; }
    public int? SessaoCaixaPdvId { get; set; }
    public int? ClienteId { get; set; }
    public string? ConsumidorNome { get; set; }
    public string? ConsumidorCpf { get; set; }
    public int? VendedorId { get; set; }
    public string? VendedorNome { get; set; }
    public decimal ValorDesconto { get; set; }
    public string? Observacoes { get; set; }
    public List<VendaPdvItemInputDto> Itens { get; set; } = new();
    public List<VendaPdvPagamentoInputDto> Pagamentos { get; set; } = new();
}

public class VendaPdvDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int TerminalPdvId { get; set; }
    public string TerminalNome { get; set; } = string.Empty;
    public int? ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public string? ConsumidorNome { get; set; }
    public string? ConsumidorCpf { get; set; }
    public int? VendedorId { get; set; }
    public string? VendedorNome { get; set; }
    public StatusVendaPdv Status { get; set; }
    public DateTime DataVenda { get; set; }
    public decimal ValorBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal Troco { get; set; }
    public string? Observacoes { get; set; }
    public List<VendaPdvItemInputDto> Itens { get; set; } = new();
    public List<VendaPdvPagamentoInputDto> Pagamentos { get; set; } = new();
}
