namespace ERP.Application.Compras.DTOs;

public enum OrigemMapeamentoProduto
{
    NaoEncontrado = 0,
    DeParaFornecedor = 1,
    CodigoBarrasEan = 2,
    CodigoFornecedor = 3
}

public class NfeXmlItemDto
{
    public int NumeroItem { get; set; }
    public string CodigoProdutoFornecedor { get; set; } = string.Empty;
    public string DescricaoProdutoFornecedor { get; set; } = string.Empty;
    public string? CodigoBarrasEan { get; set; }
    public string? Ncm { get; set; }
    public string? Cest { get; set; }
    public string? Cfop { get; set; }
    public string UnidadeMedidaFornecedor { get; set; } = "UN";
    public decimal Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorOutrasDespesas { get; set; }

    // Rastreabilidade / Lote se vier no XML
    public string? CodigoLote { get; set; }
    public DateTime? DataFabricacaoLote { get; set; }
    public DateTime? DataValidadeLote { get; set; }

    // Associação com Produto interno do ERP
    public int? ProdutoIdSugerido { get; set; }
    public string? ProdutoCodigoSugerido { get; set; }
    public string? ProdutoNomeSugerido { get; set; }
    public string? UnidadeMedidaInterna { get; set; }
    public bool ControlaLote { get; set; }
    public bool ControlaSerie { get; set; }
    public OrigemMapeamentoProduto OrigemMapeamento { get; set; } = OrigemMapeamentoProduto.NaoEncontrado;

    // Vínculo com Pedido de Compra se houver
    public int? PedidoCompraItemId { get; set; }
    public decimal? QuantidadePedida { get; set; }
}

public class NfeXmlDuplicataDto
{
    public string NumeroDuplicata { get; set; } = string.Empty;
    public DateTime DataVencimento { get; set; }
    public decimal Valor { get; set; }
}

public class NfeXmlFornecedorDto
{
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }

    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Cep { get; set; }

    public bool JaCadastrado { get; set; }
    public int? FornecedorIdExistente { get; set; }
}

public class NfeXmlDestinatarioDto
{
    public string CnpjCpf { get; set; } = string.Empty;
    public string? RazaoSocial { get; set; }
    public bool CnpjValidoParaEmpresaAtual { get; set; }
    public string? CnpjEmpresaAtual { get; set; }
}

public class NfeXmlPedidoSugeridoDto
{
    public int PedidoId { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public decimal ValorTotal { get; set; }
    public int ItensCoincidentes { get; set; }
}

public class NfeXmlParseResultDto
{
    public string ChaveAcessoNfe { get; set; } = string.Empty;
    public string NumeroNotaFiscal { get; set; } = string.Empty;
    public string? SerieNotaFiscal { get; set; }
    public DateTime DataEmissao { get; set; }
    public string? NaturezaOperacao { get; set; }

    public NfeXmlFornecedorDto Fornecedor { get; set; } = new();
    public NfeXmlDestinatarioDto Destinatario { get; set; } = new();

    public decimal ValorProdutos { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorSeguro { get; set; }
    public decimal ValorOutrasDespesas { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotalNota { get; set; }

    public List<NfeXmlItemDto> Itens { get; set; } = new();
    public List<NfeXmlDuplicataDto> Duplicatas { get; set; } = new();
    public List<NfeXmlPedidoSugeridoDto> PedidosSugeridos { get; set; } = new();

    public bool ChaveJaCadastrada { get; set; }
    public string? ChaveExistenteEntradaNumero { get; set; }

    public bool PodeProsseguir { get; set; } = true;
    public string? MensagemErroBloqueante { get; set; }
    public string? MensagemAlerta { get; set; }

    public string XmlConteudo { get; set; } = string.Empty;
}
