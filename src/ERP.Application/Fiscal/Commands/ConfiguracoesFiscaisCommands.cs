using ERP.Application.Common.Interfaces;
using ERP.Application.Fiscal.Common;
using ERP.Application.Fiscal.DTOs;
using ERP.Domain.Core.Entities.Fiscal;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Fiscal.Commands;

#region Regras Tributárias

public class SalvarRegraTributariaCommand : IRequest<RegraTributariaDto>
{
    public int? Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public TipoOperacaoFiscal TipoOperacao { get; set; } = TipoOperacaoFiscal.Saida;
    public string? NcmCodigo { get; set; }
    public string CfopCodigo { get; set; } = "5102";
    public string UfOrigem { get; set; } = "**";
    public string UfDestino { get; set; } = "**";

    public string? CstIcms { get; set; }
    public string? Csosn { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal ReducaoBaseIcms { get; set; }
    public decimal AliquotaIcmsSt { get; set; }
    public decimal MvaIcmsSt { get; set; }
    public decimal ReducaoBaseIcmsSt { get; set; }

    public string? CstPis { get; set; }
    public decimal AliquotaPis { get; set; }
    public string? CstCofins { get; set; }
    public decimal AliquotaCofins { get; set; }

    public string? CstIpi { get; set; }
    public decimal AliquotaIpi { get; set; }

    public decimal AliquotaIbs { get; set; }
    public decimal AliquotaCbs { get; set; }
    public string? ObservacoesFiscais { get; set; }
}

public class SalvarRegraTributariaCommandHandler : IRequestHandler<SalvarRegraTributariaCommand, RegraTributariaDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public SalvarRegraTributariaCommandHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<RegraTributariaDto> Handle(SalvarRegraTributariaCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        RegraTributaria? regra = null;
        if (request.Id.HasValue && request.Id.Value > 0)
        {
            regra = await _context.RegrasTributarias
                .FirstOrDefaultAsync(r => r.Id == request.Id.Value && r.EmpresaId == empresaId, cancellationToken)
                ?? throw new KeyNotFoundException($"Regra tributária #{request.Id} não encontrada.");
        }
        else
        {
            regra = new RegraTributaria
            {
                EmpresaId = empresaId,
                Ativo = true
            };
            _context.RegrasTributarias.Add(regra);
        }

        regra.Descricao = request.Descricao.Trim();
        regra.TipoOperacao = request.TipoOperacao;
        regra.NcmCodigo = !string.IsNullOrWhiteSpace(request.NcmCodigo) ? request.NcmCodigo.Replace(".", "").Trim() : null;
        regra.CfopCodigo = request.CfopCodigo.Trim();
        regra.UfOrigem = string.IsNullOrWhiteSpace(request.UfOrigem) ? "**" : request.UfOrigem.ToUpper().Trim();
        regra.UfDestino = string.IsNullOrWhiteSpace(request.UfDestino) ? "**" : request.UfDestino.ToUpper().Trim();

        regra.CstIcms = request.CstIcms;
        regra.Csosn = request.Csosn;
        regra.AliquotaIcms = request.AliquotaIcms;
        regra.ReducaoBaseIcms = request.ReducaoBaseIcms;
        regra.AliquotaIcmsSt = request.AliquotaIcmsSt;
        regra.MvaIcmsSt = request.MvaIcmsSt;
        regra.ReducaoBaseIcmsSt = request.ReducaoBaseIcmsSt;

        regra.CstPis = request.CstPis;
        regra.AliquotaPis = request.AliquotaPis;
        regra.CstCofins = request.CstCofins;
        regra.AliquotaCofins = request.AliquotaCofins;

        regra.CstIpi = request.CstIpi;
        regra.AliquotaIpi = request.AliquotaIpi;

        regra.AliquotaIbs = request.AliquotaIbs;
        regra.AliquotaCbs = request.AliquotaCbs;
        regra.ObservacoesFiscais = request.ObservacoesFiscais;

        await _context.SaveChangesAsync(cancellationToken);

        return new RegraTributariaDto
        {
            Id = regra.Id,
            EmpresaId = regra.EmpresaId,
            Descricao = regra.Descricao,
            TipoOperacao = regra.TipoOperacao,
            NcmCodigo = regra.NcmCodigo,
            CfopCodigo = regra.CfopCodigo,
            UfOrigem = regra.UfOrigem,
            UfDestino = regra.UfDestino,
            CstIcms = regra.CstIcms,
            Csosn = regra.Csosn,
            AliquotaIcms = regra.AliquotaIcms,
            ReducaoBaseIcms = regra.ReducaoBaseIcms,
            AliquotaIcmsSt = regra.AliquotaIcmsSt,
            MvaIcmsSt = regra.MvaIcmsSt,
            CstPis = regra.CstPis,
            AliquotaPis = regra.AliquotaPis,
            CstCofins = regra.CstCofins,
            AliquotaCofins = regra.AliquotaCofins,
            CstIpi = regra.CstIpi,
            AliquotaIpi = regra.AliquotaIpi,
            AliquotaIbs = regra.AliquotaIbs,
            AliquotaCbs = regra.AliquotaCbs,
            ObservacoesFiscais = regra.ObservacoesFiscais,
            Ativo = regra.Ativo
        };
    }
}

public record ExcluirRegraTributariaCommand(int Id) : IRequest<bool>;

public class ExcluirRegraTributariaCommandHandler : IRequestHandler<ExcluirRegraTributariaCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public ExcluirRegraTributariaCommandHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<bool> Handle(ExcluirRegraTributariaCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var regra = await _context.RegrasTributarias
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.EmpresaId == empresaId, cancellationToken);

        if (regra == null) return false;

        _context.RegrasTributarias.Remove(regra);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

#endregion

#region Regime Tributário

public class SalvarRegimeTributarioCommand : IRequest<RegimeTributarioDto>
{
    public RegimeTributarioEmpresa Regime { get; set; } = RegimeTributarioEmpresa.SimplesNacional;
    public AmbienteFiscal Ambiente { get; set; } = AmbienteFiscal.Homologacao;
    public int SerieNFePadrao { get; set; } = 1;
    public int UltimoNumeroNFe { get; set; }
    public int SerieNFCePadrao { get; set; } = 1;
    public int UltimoNumeroNFCe { get; set; }
    public string? IdCscToken { get; set; }
    public string? CodigoCsc { get; set; }
    public decimal? AliquotaCreditoSimples { get; set; }
    public bool IncentivadorCultural { get; set; }
    public bool PermiteCreditoIcms { get; set; }
    public string? MensagemPadraoNotas { get; set; }
}

public class SalvarRegimeTributarioCommandHandler : IRequestHandler<SalvarRegimeTributarioCommand, RegimeTributarioDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public SalvarRegimeTributarioCommandHandler(
        IApplicationDbContext context,
        ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<RegimeTributarioDto> Handle(SalvarRegimeTributarioCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        var regime = await _context.RegimesTributarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.EmpresaId == empresaId, cancellationToken);

        if (regime == null)
        {
            regime = new RegimeTributario
            {
                EmpresaId = empresaId,
                Ativo = true
            };
            _context.RegimesTributarios.Add(regime);
        }

        regime.Regime = request.Regime;
        regime.Ambiente = request.Ambiente;
        regime.SerieNFePadrao = request.SerieNFePadrao;
        if (request.UltimoNumeroNFe > 0) regime.UltimoNumeroNFe = request.UltimoNumeroNFe;
        regime.SerieNFCePadrao = request.SerieNFCePadrao;
        if (request.UltimoNumeroNFCe > 0) regime.UltimoNumeroNFCe = request.UltimoNumeroNFCe;
        regime.IdCscToken = request.IdCscToken;
        regime.CodigoCsc = request.CodigoCsc;
        regime.AliquotaCreditoSimples = request.AliquotaCreditoSimples;
        regime.IncentivadorCultural = request.IncentivadorCultural;
        regime.PermiteCreditoIcms = request.PermiteCreditoIcms;
        regime.MensagemPadraoNotas = request.MensagemPadraoNotas;

        await _context.SaveChangesAsync(cancellationToken);

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

#endregion

#region Certificados Digitais

public record SalvarCertificadoCommand(SalvarCertificadoDigitalDto Dto) : IRequest<CertificadoDigitalDto>;

public class SalvarCertificadoCommandHandler : IRequestHandler<SalvarCertificadoCommand, CertificadoDigitalDto>
{
    private readonly ICertificadoDigitalService _certificadoService;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public SalvarCertificadoCommandHandler(
        ICertificadoDigitalService certificadoService,
        ICurrentEmpresaService currentEmpresaService)
    {
        _certificadoService = certificadoService;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<CertificadoDigitalDto> Handle(SalvarCertificadoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        return await _certificadoService.SalvarCertificadoAsync(empresaId, request.Dto, cancellationToken);
    }
}

public record RemoverCertificadoCommand() : IRequest<bool>;

public class RemoverCertificadoCommandHandler : IRequestHandler<RemoverCertificadoCommand, bool>
{
    private readonly ICertificadoDigitalService _certificadoService;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public RemoverCertificadoCommandHandler(
        ICertificadoDigitalService certificadoService,
        ICurrentEmpresaService currentEmpresaService)
    {
        _certificadoService = certificadoService;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<bool> Handle(RemoverCertificadoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId
            ?? throw new InvalidOperationException("Empresa ativa não identificada.");

        await _certificadoService.RemoverCertificadoAsync(empresaId, cancellationToken);
        return true;
    }
}

#endregion
