using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.Common;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Entities.Fiscal;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Fiscal.Commands;

public class EmitirNotaFiscalCommand : IRequest<EmissaoFiscalResultDto>
{
    public int? PedidoVendaId { get; set; }
    public int? EntradaMercadoriaId { get; set; }
    public ModeloDocumentoFiscal Modelo { get; set; } = ModeloDocumentoFiscal.NFe;
    public TipoOperacaoFiscal TipoOperacao { get; set; } = TipoOperacaoFiscal.Saida;
    public string NaturezaOperacao { get; set; } = "VENDA DE MERCADORIAS";

    // Destinatário
    public int? ClienteId { get; set; }
    public int? FornecedorId { get; set; }
    public string? DestinatarioCpfCnpj { get; set; }
    public string? DestinatarioRazaoSocial { get; set; }
    public string? DestinatarioInscricaoEstadual { get; set; }
    public string? DestinatarioEmail { get; set; }
    public string? DestinatarioTelefone { get; set; }

    public string? DestinatarioLogradouro { get; set; }
    public string? DestinatarioNumero { get; set; }
    public string? DestinatarioComplemento { get; set; }
    public string? DestinatarioBairro { get; set; }
    public string? DestinatarioMunicipio { get; set; }
    public string? DestinatarioCodigoIbgeMunicipio { get; set; }
    public string DestinatarioUf { get; set; } = "SP";
    public string? DestinatarioCep { get; set; }

    // Itens da Nota
    public List<CalculoItemInputDto> Itens { get; set; } = new();
    public string? InformacoesComplementares { get; set; }
    public bool TransmitirImediatamente { get; set; } = true;
}

public class EmitirNotaFiscalCommandHandler : IRequestHandler<EmitirNotaFiscalCommand, EmissaoFiscalResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;
    private readonly ICalculoTributarioService _calculoTributarioService;
    private readonly IFiscalService _fiscalService;

    public EmitirNotaFiscalCommandHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService,
        ICalculoTributarioService calculoTributarioService,
        IFiscalService fiscalService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
        _calculoTributarioService = calculoTributarioService;
        _fiscalService = fiscalService;
    }

    public async Task<EmissaoFiscalResultDto> Handle(EmitirNotaFiscalCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada na sessão.");

        var regime = await _context.RegimesTributarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId, cancellationToken);

        var empresa = await _context.Empresas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == empresaId, cancellationToken)
            ?? throw new InvalidOperationException("Dados da empresa emitente não encontrados.");

        var itensCalculo = new List<CalculoItemInputDto>();
        string ufDestino = request.DestinatarioUf;
        int? clienteId = request.ClienteId;
        string cpfCnpj = request.DestinatarioCpfCnpj ?? "";
        string razaoSocial = request.DestinatarioRazaoSocial ?? "";
        string? ie = request.DestinatarioInscricaoEstadual;
        string? email = request.DestinatarioEmail;
        string? telefone = request.DestinatarioTelefone;
        string? logradouro = request.DestinatarioLogradouro;
        string? numero = request.DestinatarioNumero;
        string? complemento = request.DestinatarioComplemento;
        string? bairro = request.DestinatarioBairro;
        string? municipio = request.DestinatarioMunicipio;
        string? cMun = request.DestinatarioCodigoIbgeMunicipio;
        string? cep = request.DestinatarioCep;
        int? faturamentoId = null;

        // 1. Se originado de Pedido de Venda
        if (request.PedidoVendaId.HasValue && request.PedidoVendaId.Value > 0)
        {
            var pedido = await _context.PedidosVenda.AsNoTracking()
                .Include(p => p.Cliente)
                    .ThenInclude(c => c!.Enderecos)
                .Include(p => p.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(p => p.Id == request.PedidoVendaId.Value && p.EmpresaId == empresaId, cancellationToken)
                ?? throw new KeyNotFoundException($"Pedido de Venda #{request.PedidoVendaId} não encontrado.");

            clienteId = pedido.ClienteId;
            cpfCnpj = pedido.Cliente?.CpfCnpj ?? cpfCnpj;
            razaoSocial = pedido.Cliente?.Nome ?? razaoSocial;
            ie = pedido.Cliente?.InscricaoEstadual ?? ie;
            email = pedido.Cliente?.Email ?? email;
            telefone = pedido.Cliente?.Telefone ?? telefone;

            var enderecoCliente = pedido.Cliente?.Enderecos.FirstOrDefault(e => e.Principal) 
                               ?? pedido.Cliente?.Enderecos.FirstOrDefault();
            if (enderecoCliente != null)
            {
                logradouro = enderecoCliente.Logradouro;
                numero = enderecoCliente.Numero ?? numero;
                bairro = enderecoCliente.Bairro ?? bairro;
                municipio = enderecoCliente.Cidade;
                ufDestino = !string.IsNullOrWhiteSpace(enderecoCliente.Uf) ? enderecoCliente.Uf : ufDestino;
                cep = enderecoCliente.Cep;
                cMun = enderecoCliente.CodigoIbge?.ToString() ?? cMun;
            }

            // Faturamento associado (se houver)
            var faturamento = await _context.Faturamentos.AsNoTracking()
                .FirstOrDefaultAsync(f => f.PedidoVendaId == pedido.Id && f.EmpresaId == empresaId, cancellationToken);
            faturamentoId = faturamento?.Id;

            foreach (var itemPedido in pedido.Itens.Where(i => i.TipoItem == TipoItemVenda.Produto))
            {
                itensCalculo.Add(new CalculoItemInputDto
                {
                    ProdutoId = itemPedido.ProdutoId,
                    Codigo = itemPedido.Produto?.Codigo ?? $"PRD-{itemPedido.ProdutoId}",
                    Descricao = itemPedido.Produto?.Nome ?? "Produto",
                    Ncm = itemPedido.Produto?.Ncm ?? "00000000",
                    Cfop = itemPedido.Produto?.Cfop ?? (ufDestino == empresa.Uf ? "5102" : "6102"),
                    UnidadeMedida = "UN",
                    Quantidade = itemPedido.Quantidade,
                    ValorUnitario = itemPedido.PrecoUnitario,
                    Desconto = itemPedido.ValorDesconto,
                    Frete = 0,
                    Seguro = 0,
                    OutrasDespesas = 0,
                    OrigemMercadoria = 0
                });
            }
        }
        else if (request.Itens.Any())
        {
            itensCalculo.AddRange(request.Itens);
        }
        else
        {
            throw new InvalidOperationException("A nota fiscal deve conter ao menos um item de produto.");
        }

        if (string.IsNullOrWhiteSpace(cpfCnpj))
        {
            cpfCnpj = request.Modelo == ModeloDocumentoFiscal.NFCe ? "00000000000" : "00000000000000";
        }

        if (string.IsNullOrWhiteSpace(razaoSocial))
        {
            razaoSocial = "CONSUMIDOR FINAL";
        }

        // 2. Calcular Tributos dos Itens
        var resultadoCalculo = await _calculoTributarioService.CalcularTributosAsync(
            empresaId,
            request.TipoOperacao,
            ufDestino,
            itensCalculo,
            cancellationToken);

        // 3. Montar Cabeçalho da Nota Fiscal
        int serie = request.Modelo == ModeloDocumentoFiscal.NFe
            ? (regime?.SerieNFePadrao ?? 1)
            : (regime?.SerieNFCePadrao ?? 1);

        int proximoNumero = (request.Modelo == ModeloDocumentoFiscal.NFe
            ? (regime?.UltimoNumeroNFe ?? 0)
            : (regime?.UltimoNumeroNFCe ?? 0)) + 1;

        var notaFiscal = new NotaFiscal
        {
            EmpresaId = empresaId,
            Modelo = request.Modelo,
            Serie = serie,
            Numero = proximoNumero,
            NaturezaOperacao = request.NaturezaOperacao,
            TipoOperacao = request.TipoOperacao,
            TipoEmissao = TipoEmissaoFiscal.Normal,
            Ambiente = regime?.Ambiente ?? AmbienteFiscal.Homologacao,
            Status = StatusNotaFiscal.Rascunho,
            DataEmissao = DateTime.UtcNow,
            DataSaidaEntrada = DateTime.UtcNow,

            ClienteId = clienteId,
            FornecedorId = request.FornecedorId,
            DestinatarioCpfCnpj = cpfCnpj,
            DestinatarioRazaoSocial = razaoSocial,
            DestinatarioInscricaoEstadual = ie,
            DestinatarioEmail = email,
            DestinatarioTelefone = telefone,
            DestinatarioLogradouro = logradouro,
            DestinatarioNumero = numero,
            DestinatarioComplemento = complemento,
            DestinatarioBairro = bairro,
            DestinatarioMunicipio = municipio,
            DestinatarioCodigoIbgeMunicipio = cMun,
            DestinatarioUf = ufDestino,
            DestinatarioCep = cep,

            PedidoVendaId = request.PedidoVendaId,
            FaturamentoId = faturamentoId,
            EntradaMercadoriaId = request.EntradaMercadoriaId,

            // Totais
            ValorProdutos = resultadoCalculo.ValorProdutos,
            ValorFrete = resultadoCalculo.ValorFrete,
            ValorSeguro = resultadoCalculo.ValorSeguro,
            ValorDesconto = resultadoCalculo.ValorDesconto,
            ValorOutrasDespesas = resultadoCalculo.ValorOutrasDespesas,
            ValorTotalNota = resultadoCalculo.ValorTotalNota,

            BaseCalculoIcms = resultadoCalculo.BaseCalculoIcms,
            ValorIcms = resultadoCalculo.ValorIcms,
            BaseCalculoIcmsSt = resultadoCalculo.BaseCalculoIcmsSt,
            ValorIcmsSt = resultadoCalculo.ValorIcmsSt,

            ValorPis = resultadoCalculo.ValorPis,
            ValorCofins = resultadoCalculo.ValorCofins,
            ValorIpi = resultadoCalculo.ValorIpi,
            ValorIbs = resultadoCalculo.ValorIbs,
            ValorCbs = resultadoCalculo.ValorCbs,
            ValorAproximadoTributos = resultadoCalculo.ValorAproximadoTributos,

            InformacoesComplementares = string.IsNullOrWhiteSpace(request.InformacoesComplementares)
                ? resultadoCalculo.InformacoesComplementaresSugeridas
                : $"{request.InformacoesComplementares} {resultadoCalculo.InformacoesComplementaresSugeridas}".Trim(),

            Ativo = true
        };

        // 4. Adicionar Itens calculados
        foreach (var itemCalc in resultadoCalculo.Itens)
        {
            notaFiscal.Itens.Add(new NotaFiscalItem
            {
                EmpresaId = empresaId,
                NumeroItem = itemCalc.NumeroItem,
                ProdutoId = itemCalc.ProdutoId,
                CodigoProduto = itemCalc.CodigoProduto,
                DescricaoProduto = itemCalc.DescricaoProduto,
                Ncm = itemCalc.Ncm,
                Cfop = itemCalc.Cfop,
                UnidadeMedida = itemCalc.UnidadeMedida,
                Quantidade = itemCalc.Quantidade,
                ValorUnitario = itemCalc.ValorUnitario,
                ValorTotalBruto = itemCalc.ValorTotalBruto,
                ValorDesconto = itemCalc.ValorDesconto,
                ValorFrete = itemCalc.ValorFrete,
                ValorSeguro = itemCalc.ValorSeguro,
                ValorOutrasDespesas = itemCalc.ValorOutrasDespesas,
                ValorTotalLiquido = itemCalc.ValorTotalLiquido,

                OrigemMercadoria = itemCalc.OrigemMercadoria,
                CstIcms = itemCalc.CstIcms,
                Csosn = itemCalc.Csosn,
                BaseCalculoIcms = itemCalc.BaseCalculoIcms,
                AliquotaIcms = itemCalc.AliquotaIcms,
                ValorIcms = itemCalc.ValorIcms,
                PercentualReducaoBaseIcms = itemCalc.PercentualReducaoBaseIcms,

                BaseCalculoIcmsSt = itemCalc.BaseCalculoIcmsSt,
                AliquotaIcmsSt = itemCalc.AliquotaIcmsSt,
                ValorIcmsSt = itemCalc.ValorIcmsSt,
                PercentualMvaSt = itemCalc.PercentualMvaSt,

                CstPis = itemCalc.CstPis,
                BaseCalculoPis = itemCalc.BaseCalculoPis,
                AliquotaPis = itemCalc.AliquotaPis,
                ValorPis = itemCalc.ValorPis,

                CstCofins = itemCalc.CstCofins,
                BaseCalculoCofins = itemCalc.BaseCalculoCofins,
                AliquotaCofins = itemCalc.AliquotaCofins,
                ValorCofins = itemCalc.ValorCofins,

                CstIpi = itemCalc.CstIpi,
                BaseCalculoIpi = itemCalc.BaseCalculoIpi,
                AliquotaIpi = itemCalc.AliquotaIpi,
                ValorIpi = itemCalc.ValorIpi,

                AliquotaIbs = itemCalc.AliquotaIbs,
                ValorIbs = itemCalc.ValorIbs,
                AliquotaCbs = itemCalc.AliquotaCbs,
                ValorCbs = itemCalc.ValorCbs,
                ValorAproximadoTributos = itemCalc.ValorAproximadoTributos,

                Ativo = true
            });
        }

        _context.NotasFiscais.Add(notaFiscal);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Transmitir e Homologar se solicitado
        if (request.TransmitirImediatamente)
        {
            return await _fiscalService.TransmitirNotaFiscalAsync(notaFiscal, cancellationToken);
        }

        return new EmissaoFiscalResultDto
        {
            Sucesso = true,
            Mensagem = "Nota fiscal salva como rascunho com sucesso. Pronta para conferência e emissão.",
            NotaFiscalId = notaFiscal.Id,
            ChaveAcesso = notaFiscal.ChaveAcesso,
            CodigoStatus = 0,
            MotivoStatus = "Rascunho criado"
        };
    }
}
