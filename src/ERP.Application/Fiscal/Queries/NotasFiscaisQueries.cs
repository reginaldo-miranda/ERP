using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Fiscal.Queries;

public record GetNotasFiscaisQuery(
    string? Busca = null,
    ModeloDocumentoFiscal? Modelo = null,
    StatusNotaFiscal? Status = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    int Pagina = 1,
    int TamanhoPagina = 20
) : IRequest<GetNotasFiscaisResult>;

public record GetNotasFiscaisResult(List<NotaFiscalDto> Itens, int TotalCount);

public class GetNotasFiscaisQueryHandler : IRequestHandler<GetNotasFiscaisQuery, GetNotasFiscaisResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetNotasFiscaisQueryHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<GetNotasFiscaisResult> Handle(GetNotasFiscaisQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var query = _context.NotasFiscais.AsNoTracking()
            .Where(n => n.EmpresaId == empresaId && n.Ativo);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            string busca = request.Busca.Trim().ToLower();
            query = query.Where(n =>
                n.Numero.ToString().Contains(busca) ||
                n.ChaveAcesso.Contains(busca) ||
                n.DestinatarioRazaoSocial.ToLower().Contains(busca) ||
                n.DestinatarioCpfCnpj.Contains(busca));
        }

        if (request.Modelo.HasValue)
        {
            query = query.Where(n => n.Modelo == request.Modelo.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(n => n.Status == request.Status.Value);
        }

        if (request.DataInicio.HasValue)
        {
            query = query.Where(n => n.DataEmissao >= request.DataInicio.Value);
        }

        if (request.DataFim.HasValue)
        {
            var fimAjustado = request.DataFim.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(n => n.DataEmissao <= fimAjustado);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(n => n.DataEmissao)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina)
            .Select(n => new NotaFiscalDto
            {
                Id = n.Id,
                EmpresaId = n.EmpresaId,
                Modelo = n.Modelo,
                Serie = n.Serie,
                Numero = n.Numero,
                ChaveAcesso = n.ChaveAcesso,
                NaturezaOperacao = n.NaturezaOperacao,
                TipoOperacao = n.TipoOperacao,
                Ambiente = n.Ambiente,
                Status = n.Status,
                DataEmissao = n.DataEmissao,
                DestinatarioCpfCnpj = n.DestinatarioCpfCnpj,
                DestinatarioRazaoSocial = n.DestinatarioRazaoSocial,
                DestinatarioUf = n.DestinatarioUf,
                ValorProdutos = n.ValorProdutos,
                ValorFrete = n.ValorFrete,
                ValorDesconto = n.ValorDesconto,
                ValorTotalNota = n.ValorTotalNota,
                ValorIcms = n.ValorIcms,
                ValorPis = n.ValorPis,
                ValorCofins = n.ValorCofins,
                ProtocoloAutorizacao = n.ProtocoloAutorizacao,
                DataAutorizacao = n.DataAutorizacao,
                CodigoStatusSefaz = n.CodigoStatusSefaz,
                MotivoStatusSefaz = n.MotivoStatusSefaz,
                PedidoVendaId = n.PedidoVendaId,
                FaturamentoId = n.FaturamentoId,
                EntradaMercadoriaId = n.EntradaMercadoriaId,
                QuantidadeItens = n.Itens.Count
            })
            .ToListAsync(cancellationToken);

        return new GetNotasFiscaisResult(itens, totalCount);
    }
}

public record GetNotaFiscalDetalhesQuery(int Id) : IRequest<NotaFiscalDetalhesDto>;

public class GetNotaFiscalDetalhesQueryHandler : IRequestHandler<GetNotaFiscalDetalhesQuery, NotaFiscalDetalhesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetNotaFiscalDetalhesQueryHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<NotaFiscalDetalhesDto> Handle(GetNotaFiscalDetalhesQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var nota = await _context.NotasFiscais.AsNoTracking()
            .Include(n => n.Itens)
            .Include(n => n.Xmls)
            .Include(n => n.CartasCorrecao)
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Nota Fiscal #{request.Id} não encontrada.");

        return new NotaFiscalDetalhesDto
        {
            Id = nota.Id,
            EmpresaId = nota.EmpresaId,
            Modelo = nota.Modelo,
            Serie = nota.Serie,
            Numero = nota.Numero,
            ChaveAcesso = nota.ChaveAcesso,
            NaturezaOperacao = nota.NaturezaOperacao,
            TipoOperacao = nota.TipoOperacao,
            Ambiente = nota.Ambiente,
            Status = nota.Status,
            DataEmissao = nota.DataEmissao,
            DestinatarioCpfCnpj = nota.DestinatarioCpfCnpj,
            DestinatarioRazaoSocial = nota.DestinatarioRazaoSocial,
            DestinatarioInscricaoEstadual = nota.DestinatarioInscricaoEstadual,
            DestinatarioEmail = nota.DestinatarioEmail,
            DestinatarioTelefone = nota.DestinatarioTelefone,
            DestinatarioLogradouro = nota.DestinatarioLogradouro,
            DestinatarioNumero = nota.DestinatarioNumero,
            DestinatarioComplemento = nota.DestinatarioComplemento,
            DestinatarioBairro = nota.DestinatarioBairro,
            DestinatarioMunicipio = nota.DestinatarioMunicipio,
            DestinatarioUf = nota.DestinatarioUf,
            DestinatarioCep = nota.DestinatarioCep,

            ValorProdutos = nota.ValorProdutos,
            ValorFrete = nota.ValorFrete,
            ValorDesconto = nota.ValorDesconto,
            ValorTotalNota = nota.ValorTotalNota,
            BaseCalculoIcms = nota.BaseCalculoIcms,
            ValorIcms = nota.ValorIcms,
            BaseCalculoIcmsSt = nota.BaseCalculoIcmsSt,
            ValorIcmsSt = nota.ValorIcmsSt,
            ValorPis = nota.ValorPis,
            ValorCofins = nota.ValorCofins,
            ValorIpi = nota.ValorIpi,
            ValorIbs = nota.ValorIbs,
            ValorCbs = nota.ValorCbs,
            ValorAproximadoTributos = nota.ValorAproximadoTributos,

            ProtocoloAutorizacao = nota.ProtocoloAutorizacao,
            DataAutorizacao = nota.DataAutorizacao,
            CodigoStatusSefaz = nota.CodigoStatusSefaz,
            MotivoStatusSefaz = nota.MotivoStatusSefaz,
            DataCancelamento = nota.DataCancelamento,
            MotivoCancelamento = nota.MotivoCancelamento,
            ProtocoloCancelamento = nota.ProtocoloCancelamento,
            InformacoesComplementares = nota.InformacoesComplementares,
            ObservacoesFisco = nota.ObservacoesFisco,

            PedidoVendaId = nota.PedidoVendaId,
            FaturamentoId = nota.FaturamentoId,
            EntradaMercadoriaId = nota.EntradaMercadoriaId,
            QuantidadeItens = nota.Itens.Count,

            Itens = nota.Itens.OrderBy(i => i.NumeroItem).Select(i => new NotaFiscalItemDto
            {
                Id = i.Id,
                NumeroItem = i.NumeroItem,
                ProdutoId = i.ProdutoId,
                CodigoProduto = i.CodigoProduto,
                DescricaoProduto = i.DescricaoProduto,
                Ncm = i.Ncm,
                Cfop = i.Cfop,
                UnidadeMedida = i.UnidadeMedida,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorTotalBruto = i.ValorTotalBruto,
                ValorDesconto = i.ValorDesconto,
                ValorTotalLiquido = i.ValorTotalLiquido,
                OrigemMercadoria = i.OrigemMercadoria,
                CstIcms = i.CstIcms,
                Csosn = i.Csosn,
                BaseCalculoIcms = i.BaseCalculoIcms,
                AliquotaIcms = i.AliquotaIcms,
                ValorIcms = i.ValorIcms,
                CstPis = i.CstPis,
                AliquotaPis = i.AliquotaPis,
                ValorPis = i.ValorPis,
                CstCofins = i.CstCofins,
                AliquotaCofins = i.AliquotaCofins,
                ValorCofins = i.ValorCofins,
                AliquotaIbs = i.AliquotaIbs,
                ValorIbs = i.ValorIbs,
                AliquotaCbs = i.AliquotaCbs,
                ValorCbs = i.ValorCbs
            }).ToList(),

            Xmls = nota.Xmls.OrderByDescending(x => x.DataGeracao).Select(x => new XmlNotaFiscalDto
            {
                Id = x.Id,
                TipoXml = x.TipoXml,
                XmlConteudo = x.XmlConteudo,
                ChaveAcesso = x.ChaveAcesso,
                DataGeracao = x.DataGeracao
            }).ToList(),

            CartasCorrecao = nota.CartasCorrecao.OrderBy(c => c.SequenciaEvento).Select(c => new CartaCorrecaoDto
            {
                Id = c.Id,
                SequenciaEvento = c.SequenciaEvento,
                CorrecaoTexto = c.CorrecaoTexto,
                DataEvento = c.DataEvento,
                ProtocoloHomologacao = c.ProtocoloHomologacao,
                CodigoStatusSefaz = c.CodigoStatusSefaz,
                MotivoStatusSefaz = c.MotivoStatusSefaz
            }).ToList()
        };
    }
}

public record GetXmlNotaFiscalQuery(int NotaFiscalId, TipoXmlNotaFiscal? TipoXml = null) : IRequest<string>;

public class GetXmlNotaFiscalQueryHandler : IRequestHandler<GetXmlNotaFiscalQuery, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetXmlNotaFiscalQueryHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<string> Handle(GetXmlNotaFiscalQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var query = _context.XmlNotasFiscais.AsNoTracking()
            .Where(x => x.NotaFiscalId == request.NotaFiscalId && x.EmpresaId == empresaId);

        if (request.TipoXml.HasValue)
        {
            query = query.Where(x => x.TipoXml == request.TipoXml.Value);
        }
        else
        {
            // Prefere retorno processado (-procNFe), senão envio
            query = query.OrderByDescending(x => x.TipoXml == TipoXmlNotaFiscal.DistribuicaoProc ? 2 : 1);
        }

        var xml = await query.FirstOrDefaultAsync(cancellationToken);
        if (xml == null)
            throw new KeyNotFoundException($"XML para a Nota Fiscal #{request.NotaFiscalId} não encontrado.");

        return xml.XmlConteudo;
    }
}
