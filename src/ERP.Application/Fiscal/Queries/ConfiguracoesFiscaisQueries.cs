using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.Common;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Fiscal.Queries;

#region Regime e Regras Tributárias

public record GetRegimeTributarioQuery : IRequest<RegimeTributarioDto>;

public class GetRegimeTributarioQueryHandler : IRequestHandler<GetRegimeTributarioQuery, RegimeTributarioDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetRegimeTributarioQueryHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<RegimeTributarioDto> Handle(GetRegimeTributarioQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var regime = await _context.RegimesTributarios.AsNoTracking()
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId, cancellationToken);

        if (regime == null)
        {
            return new RegimeTributarioDto
            {
                EmpresaId = empresaId,
                Regime = RegimeTributarioEmpresa.SimplesNacional,
                Ambiente = AmbienteFiscal.Homologacao,
                SerieNFePadrao = 1,
                UltimoNumeroNFe = 0,
                SerieNFCePadrao = 1,
                UltimoNumeroNFCe = 0
            };
        }

        return new RegimeTributarioDto
        {
            Id = regime.Id,
            EmpresaId = regime.EmpresaId,
            Regime = regime.Regime,
            Ambiente = regime.Ambiente,
            SerieNFePadrao = regime.SerieNFePadrao,
            UltimoNumeroNFe = regime.UltimoNumeroNFe,
            SerieNFCePadrao = regime.SerieNFCePadrao,
            UltimoNumeroNFCe = regime.UltimoNumeroNFCe,
            IdCscToken = regime.IdCscToken,
            CodigoCsc = regime.CodigoCsc,
            AliquotaCreditoSimples = regime.AliquotaCreditoSimples,
            IncentivadorCultural = regime.IncentivadorCultural,
            PermiteCreditoIcms = regime.PermiteCreditoIcms,
            MensagemPadraoNotas = regime.MensagemPadraoNotas
        };
    }
}

public record GetRegrasTributariasQuery(TipoOperacaoFiscal? TipoOperacao = null) : IRequest<List<RegraTributariaDto>>;

public class GetRegrasTributariasQueryHandler : IRequestHandler<GetRegrasTributariasQuery, List<RegraTributariaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetRegrasTributariasQueryHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<List<RegraTributariaDto>> Handle(GetRegrasTributariasQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var query = _context.RegrasTributarias.AsNoTracking()
            .Where(r => r.EmpresaId == empresaId && r.Ativo);

        if (request.TipoOperacao.HasValue)
        {
            query = query.Where(r => r.TipoOperacao == request.TipoOperacao.Value);
        }

        return await query
            .OrderBy(r => r.TipoOperacao)
            .ThenBy(r => r.CfopCodigo)
            .Select(r => new RegraTributariaDto
            {
                Id = r.Id,
                EmpresaId = r.EmpresaId,
                Descricao = r.Descricao,
                TipoOperacao = r.TipoOperacao,
                NcmCodigo = r.NcmCodigo,
                CfopCodigo = r.CfopCodigo,
                UfOrigem = r.UfOrigem,
                UfDestino = r.UfDestino,
                CstIcms = r.CstIcms,
                Csosn = r.Csosn,
                AliquotaIcms = r.AliquotaIcms,
                ReducaoBaseIcms = r.ReducaoBaseIcms,
                AliquotaIcmsSt = r.AliquotaIcmsSt,
                MvaIcmsSt = r.MvaIcmsSt,
                CstPis = r.CstPis,
                AliquotaPis = r.AliquotaPis,
                CstCofins = r.CstCofins,
                AliquotaCofins = r.AliquotaCofins,
                CstIpi = r.CstIpi,
                AliquotaIpi = r.AliquotaIpi,
                AliquotaIbs = r.AliquotaIbs,
                AliquotaCbs = r.AliquotaCbs,
                ObservacoesFiscais = r.ObservacoesFiscais,
                Ativo = r.Ativo
            })
            .ToListAsync(cancellationToken);
    }
}

#endregion

#region Certificados Digitais

public record GetCertificadoDigitalQuery : IRequest<CertificadoDigitalDto?>;

public class GetCertificadoDigitalQueryHandler : IRequestHandler<GetCertificadoDigitalQuery, CertificadoDigitalDto?>
{
    private readonly ICertificadoDigitalService _certificadoService;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public GetCertificadoDigitalQueryHandler(
        ICertificadoDigitalService certificadoService,
        ICurrentEmpresaService currentEmpresaService)
    {
        _certificadoService = certificadoService;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<CertificadoDigitalDto?> Handle(GetCertificadoDigitalQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        return await _certificadoService.ObterCertificadoAtivoAsync(empresaId, cancellationToken);
    }
}

#endregion

#region Status SEFAZ

public record ConsultarStatusSefazQuery : IRequest<ConsultaStatusSefazResultDto>;

public class ConsultarStatusSefazQueryHandler : IRequestHandler<ConsultarStatusSefazQuery, ConsultaStatusSefazResultDto>
{
    private readonly IFiscalService _fiscalService;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public ConsultarStatusSefazQueryHandler(
        IFiscalService fiscalService,
        ICurrentEmpresaService currentEmpresaService)
    {
        _fiscalService = fiscalService;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<ConsultaStatusSefazResultDto> Handle(ConsultarStatusSefazQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        return await _fiscalService.ConsultarStatusSefazAsync(empresaId, cancellationToken);
    }
}

#endregion

#region NCM e CFOP

public record GetNcmsQuery(string? Busca = null) : IRequest<List<NcmDto>>;

public class GetNcmsQueryHandler : IRequestHandler<GetNcmsQuery, List<NcmDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNcmsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NcmDto>> Handle(GetNcmsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Ncms.AsNoTracking().Where(n => n.Ativo);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            string busca = request.Busca.Trim().ToLower();
            query = query.Where(n => n.Codigo.Contains(busca) || n.Descricao.ToLower().Contains(busca));
        }

        return await query
            .OrderBy(n => n.Codigo)
            .Take(50)
            .Select(n => new NcmDto
            {
                Id = n.Id,
                Codigo = n.Codigo,
                Descricao = n.Descricao,
                AliquotaIpi = n.AliquotaIpi,
                AliquotaII = n.AliquotaII,
                AliquotaNacional = n.AliquotaNacional,
                AliquotaImportado = n.AliquotaImportado,
                AliquotaEstadual = n.AliquotaEstadual,
                AliquotaMunicipal = n.AliquotaMunicipal
            })
            .ToListAsync(cancellationToken);
    }
}

public record GetCfopsQuery(string? Busca = null, TipoOperacaoFiscal? TipoOperacao = null) : IRequest<List<CfopDto>>;

public class GetCfopsQueryHandler : IRequestHandler<GetCfopsQuery, List<CfopDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCfopsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CfopDto>> Handle(GetCfopsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Cfops.AsNoTracking().Where(c => c.Ativo);

        if (request.TipoOperacao.HasValue)
        {
            query = query.Where(c => c.TipoOperacao == request.TipoOperacao.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            string busca = request.Busca.Trim().ToLower();
            query = query.Where(c => c.Codigo.Contains(busca) || c.Descricao.ToLower().Contains(busca));
        }

        return await query
            .OrderBy(c => c.Codigo)
            .Take(50)
            .Select(c => new CfopDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Descricao = c.Descricao,
                TipoOperacao = c.TipoOperacao,
                Aplicacao = c.Aplicacao,
                GeraFinanceiro = c.GeraFinanceiro,
                MovimentaEstoque = c.MovimentaEstoque
            })
            .ToListAsync(cancellationToken);
    }
}

#endregion
