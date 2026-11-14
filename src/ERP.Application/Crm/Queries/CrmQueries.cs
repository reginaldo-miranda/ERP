using ERP.Application.Common.Interfaces;
using ERP.Application.Crm.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Crm.Queries;

public record ListarLeadsQuery(
    string? Busca = null,
    StatusLead? Status = null,
    OrigemLead? Origem = null,
    string? ResponsavelId = null
) : IRequest<List<LeadDto>>;

public record ObterLeadPorIdQuery(int Id) : IRequest<LeadDto?>;

public record ListarOportunidadesQuery(
    string? Busca = null,
    EtapaFunil? Etapa = null,
    string? ResponsavelId = null
) : IRequest<List<OportunidadeDto>>;

public record ObterOportunidadePorIdQuery(int Id) : IRequest<OportunidadeDto?>;

public record ListarAtividadesCrmQuery(
    int? LeadId = null,
    int? OportunidadeId = null,
    bool? ApenasPendentes = null,
    bool? ApenasAtrasadas = null
) : IRequest<List<AtividadeCrmDto>>;

public record ObterResumoKpiCrmQuery() : IRequest<CrmResumoKpiDto>;

public class CrmQueriesHandler :
    IRequestHandler<ListarLeadsQuery, List<LeadDto>>,
    IRequestHandler<ObterLeadPorIdQuery, LeadDto?>,
    IRequestHandler<ListarOportunidadesQuery, List<OportunidadeDto>>,
    IRequestHandler<ObterOportunidadePorIdQuery, OportunidadeDto?>,
    IRequestHandler<ListarAtividadesCrmQuery, List<AtividadeCrmDto>>,
    IRequestHandler<ObterResumoKpiCrmQuery, CrmResumoKpiDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public CrmQueriesHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<List<LeadDto>> Handle(ListarLeadsQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var query = _context.Leads
            .AsNoTracking()
            .Where(l => l.EmpresaId == empresaId);

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        if (request.Origem.HasValue)
            query = query.Where(l => l.Origem == request.Origem.Value);

        if (!string.IsNullOrWhiteSpace(request.ResponsavelId))
            query = query.Where(l => l.ResponsavelId == request.ResponsavelId);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(l =>
                l.Nome.ToLower().Contains(termo) ||
                (l.EmpresaContato != null && l.EmpresaContato.ToLower().Contains(termo)) ||
                (l.Email != null && l.Email.ToLower().Contains(termo)) ||
                (l.Telefone != null && l.Telefone.ToLower().Contains(termo)) ||
                (l.Celular != null && l.Celular.ToLower().Contains(termo)));
        }

        return await query
            .OrderByDescending(l => l.CriadoEm)
            .Select(l => new LeadDto
            {
                Id = l.Id,
                Nome = l.Nome,
                EmpresaContato = l.EmpresaContato,
                TipoPessoa = l.TipoPessoa,
                CpfCnpj = l.CpfCnpj,
                Email = l.Email,
                Telefone = l.Telefone,
                Celular = l.Celular,
                Cargo = l.Cargo,
                Origem = l.Origem,
                Status = l.Status,
                Observacoes = l.Observacoes,
                ResponsavelId = l.ResponsavelId,
                ResponsavelNome = l.ResponsavelNome,
                ClienteConvertidoId = l.ClienteConvertidoId,
                ClienteConvertidoNome = l.ClienteConvertido != null ? l.ClienteConvertido.Nome : null,
                DataConversao = l.DataConversao,
                CriadoEm = l.CriadoEm,
                TotalOportunidades = l.Oportunidades.Count,
                AtividadesPendentes = l.Atividades.Count(a => !a.Concluida)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<LeadDto?> Handle(ObterLeadPorIdQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        return await _context.Leads
            .AsNoTracking()
            .Where(l => l.EmpresaId == empresaId && l.Id == request.Id)
            .Select(l => new LeadDto
            {
                Id = l.Id,
                Nome = l.Nome,
                EmpresaContato = l.EmpresaContato,
                TipoPessoa = l.TipoPessoa,
                CpfCnpj = l.CpfCnpj,
                Email = l.Email,
                Telefone = l.Telefone,
                Celular = l.Celular,
                Cargo = l.Cargo,
                Origem = l.Origem,
                Status = l.Status,
                Observacoes = l.Observacoes,
                ResponsavelId = l.ResponsavelId,
                ResponsavelNome = l.ResponsavelNome,
                ClienteConvertidoId = l.ClienteConvertidoId,
                ClienteConvertidoNome = l.ClienteConvertido != null ? l.ClienteConvertido.Nome : null,
                DataConversao = l.DataConversao,
                CriadoEm = l.CriadoEm,
                TotalOportunidades = l.Oportunidades.Count,
                AtividadesPendentes = l.Atividades.Count(a => !a.Concluida)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<OportunidadeDto>> Handle(ListarOportunidadesQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var hoje = DateTime.UtcNow.Date;

        var query = _context.Oportunidades
            .AsNoTracking()
            .Where(o => o.EmpresaId == empresaId);

        if (request.Etapa.HasValue)
            query = query.Where(o => o.Etapa == request.Etapa.Value);

        if (!string.IsNullOrWhiteSpace(request.ResponsavelId))
            query = query.Where(o => o.ResponsavelId == request.ResponsavelId);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var termo = request.Busca.Trim().ToLower();
            query = query.Where(o =>
                o.Codigo.ToLower().Contains(termo) ||
                o.Titulo.ToLower().Contains(termo) ||
                (o.Lead != null && o.Lead.Nome.ToLower().Contains(termo)) ||
                (o.Cliente != null && o.Cliente.Nome.ToLower().Contains(termo)));
        }

        return await query
            .OrderByDescending(o => o.CriadoEm)
            .Select(o => new OportunidadeDto
            {
                Id = o.Id,
                Codigo = o.Codigo,
                Titulo = o.Titulo,
                Descricao = o.Descricao,
                LeadId = o.LeadId,
                LeadNome = o.Lead != null ? o.Lead.Nome : null,
                ClienteId = o.ClienteId,
                ClienteNome = o.Cliente != null ? o.Cliente.Nome : null,
                Etapa = o.Etapa,
                ValorEstimado = o.ValorEstimado,
                Probabilidade = o.Probabilidade,
                DataFechamentoPrevista = o.DataFechamentoPrevista,
                DataFechamentoReal = o.DataFechamentoReal,
                ResponsavelId = o.ResponsavelId,
                ResponsavelNome = o.ResponsavelNome,
                MotivoPerda = o.MotivoPerda,
                OrcamentoId = o.OrcamentoId,
                OrcamentoNumero = o.Orcamento != null ? o.Orcamento.Numero : null,
                PedidoVendaId = o.PedidoVendaId,
                PedidoVendaNumero = o.PedidoVenda != null ? o.PedidoVenda.Numero : null,
                CriadoEm = o.CriadoEm,
                AtividadesPendentes = o.Atividades.Count(a => !a.Concluida),
                TemAtividadeAtrasada = o.Atividades.Any(a => !a.Concluida && a.DataPrevista.Date < hoje)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OportunidadeDto?> Handle(ObterOportunidadePorIdQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var hoje = DateTime.UtcNow.Date;

        return await _context.Oportunidades
            .AsNoTracking()
            .Where(o => o.EmpresaId == empresaId && o.Id == request.Id)
            .Select(o => new OportunidadeDto
            {
                Id = o.Id,
                Codigo = o.Codigo,
                Titulo = o.Titulo,
                Descricao = o.Descricao,
                LeadId = o.LeadId,
                LeadNome = o.Lead != null ? o.Lead.Nome : null,
                ClienteId = o.ClienteId,
                ClienteNome = o.Cliente != null ? o.Cliente.Nome : null,
                Etapa = o.Etapa,
                ValorEstimado = o.ValorEstimado,
                Probabilidade = o.Probabilidade,
                DataFechamentoPrevista = o.DataFechamentoPrevista,
                DataFechamentoReal = o.DataFechamentoReal,
                ResponsavelId = o.ResponsavelId,
                ResponsavelNome = o.ResponsavelNome,
                MotivoPerda = o.MotivoPerda,
                OrcamentoId = o.OrcamentoId,
                OrcamentoNumero = o.Orcamento != null ? o.Orcamento.Numero : null,
                PedidoVendaId = o.PedidoVendaId,
                PedidoVendaNumero = o.PedidoVenda != null ? o.PedidoVenda.Numero : null,
                CriadoEm = o.CriadoEm,
                AtividadesPendentes = o.Atividades.Count(a => !a.Concluida),
                TemAtividadeAtrasada = o.Atividades.Any(a => !a.Concluida && a.DataPrevista.Date < hoje)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<AtividadeCrmDto>> Handle(ListarAtividadesCrmQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var hoje = DateTime.UtcNow.Date;

        var query = _context.AtividadesCrm
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId);

        if (request.LeadId.HasValue)
            query = query.Where(a => a.LeadId == request.LeadId.Value);

        if (request.OportunidadeId.HasValue)
            query = query.Where(a => a.OportunidadeId == request.OportunidadeId.Value);

        if (request.ApenasPendentes == true)
            query = query.Where(a => !a.Concluida);

        if (request.ApenasAtrasadas == true)
            query = query.Where(a => !a.Concluida && a.DataPrevista.Date < hoje);

        return await query
            .OrderBy(a => a.Concluida)
            .ThenBy(a => a.DataPrevista)
            .Select(a => new AtividadeCrmDto
            {
                Id = a.Id,
                Tipo = a.Tipo,
                Assunto = a.Assunto,
                Descricao = a.Descricao,
                DataPrevista = a.DataPrevista,
                DataConclusao = a.DataConclusao,
                Concluida = a.Concluida,
                LeadId = a.LeadId,
                LeadNome = a.Lead != null ? a.Lead.Nome : null,
                OportunidadeId = a.OportunidadeId,
                OportunidadeTitulo = a.Oportunidade != null ? a.Oportunidade.Titulo : null,
                ResponsavelId = a.ResponsavelId,
                ResponsavelNome = a.ResponsavelNome,
                CriadoEm = a.CriadoEm
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CrmResumoKpiDto> Handle(ObterResumoKpiCrmQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var leads = await _context.Leads
            .AsNoTracking()
            .Where(l => l.EmpresaId == empresaId)
            .Select(l => l.Status)
            .ToListAsync(cancellationToken);

        var oportunidades = await _context.Oportunidades
            .AsNoTracking()
            .Where(o => o.EmpresaId == empresaId)
            .Select(o => new { o.Etapa, o.ValorEstimado, o.DataFechamentoReal })
            .ToListAsync(cancellationToken);

        var atividadesPendentes = await _context.AtividadesCrm
            .AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && !a.Concluida)
            .Select(a => a.DataPrevista)
            .ToListAsync(cancellationToken);

        int totalLeads = leads.Count;
        int convertidos = leads.Count(s => s == StatusLead.Convertido);
        decimal taxaConversao = totalLeads > 0 ? Math.Round((decimal)convertidos / totalLeads * 100m, 1) : 0m;

        var abertas = oportunidades.Where(o => o.Etapa != EtapaFunil.Ganha && o.Etapa != EtapaFunil.Perdida).ToList();
        decimal ganhoMes = oportunidades
            .Where(o => o.Etapa == EtapaFunil.Ganha && o.DataFechamentoReal.HasValue && o.DataFechamentoReal.Value >= inicioMes)
            .Sum(o => o.ValorEstimado);

        return new CrmResumoKpiDto
        {
            TotalLeads = totalLeads,
            LeadsNovosOuEmContato = leads.Count(s => s == StatusLead.Novo || s == StatusLead.EmContato),
            OportunidadesAbertas = abertas.Count,
            ValorPipeline = abertas.Sum(o => o.ValorEstimado),
            ValorGanhoMes = ganhoMes,
            TaxaConversaoPercent = taxaConversao,
            AtividadesAtrasadas = atividadesPendentes.Count(d => d.Date < hoje),
            AtividadesHoje = atividadesPendentes.Count(d => d.Date == hoje)
        };
    }
}
