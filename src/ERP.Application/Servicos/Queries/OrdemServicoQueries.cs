using ERP.Application.Common.Interfaces;
using ERP.Application.Servicos.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Servicos.Queries;

public record GetOrdensServicoQuery(
    StatusOrdemServico? Status = null,
    int? ClienteId = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    string? TermoBusca = null
) : IRequest<List<OrdemServicoListItemDto>>;

public record GetOrdemServicoByIdQuery(int Id) : IRequest<OrdemServicoDetalhesDto?>;

public record GetProximoNumeroOrdemServicoQuery : IRequest<string>;

public record GetResumoDashboardOrdensServicoQuery : IRequest<ResumoDashboardOrdensServicoDto>;

public class OrdemServicoQueriesHandler :
    IRequestHandler<GetOrdensServicoQuery, List<OrdemServicoListItemDto>>,
    IRequestHandler<GetOrdemServicoByIdQuery, OrdemServicoDetalhesDto?>,
    IRequestHandler<GetProximoNumeroOrdemServicoQuery, string>,
    IRequestHandler<GetResumoDashboardOrdensServicoQuery, ResumoDashboardOrdensServicoDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public OrdemServicoQueriesHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<List<OrdemServicoListItemDto>> Handle(GetOrdensServicoQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var query = _context.OrdensServico
            .AsNoTracking()
            .Include(o => o.Cliente)
            .Where(o => o.EmpresaId == empresaId);

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        if (request.ClienteId.HasValue)
        {
            query = query.Where(o => o.ClienteId == request.ClienteId.Value);
        }

        if (request.DataInicio.HasValue)
        {
            var dataIniUtc = DateTime.SpecifyKind(request.DataInicio.Value.Date, DateTimeKind.Utc);
            query = query.Where(o => o.DataAbertura >= dataIniUtc);
        }

        if (request.DataFim.HasValue)
        {
            var dataFimUtc = DateTime.SpecifyKind(request.DataFim.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(o => o.DataAbertura <= dataFimUtc);
        }

        if (!string.IsNullOrWhiteSpace(request.TermoBusca))
        {
            var termo = request.TermoBusca.Trim().ToLower();
            query = query.Where(o =>
                o.Numero.ToLower().Contains(termo) ||
                o.Cliente.Nome.ToLower().Contains(termo) ||
                o.Equipamento.ToLower().Contains(termo) ||
                (o.Marca != null && o.Marca.ToLower().Contains(termo)) ||
                (o.Modelo != null && o.Modelo.ToLower().Contains(termo)) ||
                (o.NumeroSerie != null && o.NumeroSerie.ToLower().Contains(termo)) ||
                (o.TecnicoResponsavel != null && o.TecnicoResponsavel.ToLower().Contains(termo))
            );
        }

        var lista = await query
            .OrderByDescending(o => o.DataAbertura)
            .Select(o => new OrdemServicoListItemDto(
                o.Id,
                o.Numero,
                o.ClienteId,
                o.Cliente.Nome,
                o.Cliente.Telefone,
                o.Equipamento,
                !string.IsNullOrEmpty(o.Marca) && !string.IsNullOrEmpty(o.Modelo)
                    ? $"{o.Marca} {o.Modelo}"
                    : (o.Marca ?? o.Modelo ?? string.Empty),
                o.NumeroSerie,
                o.Status,
                o.Prioridade,
                o.DataAbertura,
                o.PrevisaoConclusao,
                o.DataConclusao,
                o.TecnicoResponsavel,
                o.ValorServicos,
                o.ValorPecas,
                o.ValorDesconto,
                o.ValorTotal
            ))
            .ToListAsync(cancellationToken);

        return lista;
    }

    public async Task<OrdemServicoDetalhesDto?> Handle(GetOrdemServicoByIdQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var os = await _context.OrdensServico
            .AsNoTracking()
            .Include(o => o.Cliente)
                .ThenInclude(c => c.Enderecos)
            .Include(o => o.FormaPagamento)
            .Include(o => o.ItensPecas)
                .ThenInclude(p => p.Produto)
            .Include(o => o.ItensPecas)
                .ThenInclude(p => p.Deposito)
            .Include(o => o.ItensServicos)
                .ThenInclude(s => s.Servico)
            .Include(o => o.HistoricoStatus)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.EmpresaId == empresaId, cancellationToken);

        if (os == null)
            return null;

        string? enderecoCliente = null;
        var end = os.Cliente.Enderecos.FirstOrDefault();
        if (end != null)
        {
            enderecoCliente = $"{end.Logradouro}, {end.Numero} {end.Bairro} - {end.Cidade}/{end.Uf} CEP: {end.Cep}";
        }

        return new OrdemServicoDetalhesDto(
            os.Id,
            os.Numero,
            os.ClienteId,
            os.Cliente.Nome,
            os.Cliente.CpfCnpj,
            os.Cliente.Telefone,
            os.Cliente.Email,
            enderecoCliente,
            os.Status,
            os.Prioridade,
            os.DataAbertura,
            os.PrevisaoConclusao,
            os.DataConclusao,
            os.DataEntrega,
            os.TecnicoResponsavel,
            os.Equipamento,
            os.Marca,
            os.Modelo,
            os.NumeroSerie,
            os.Acessorios,
            os.CondicoesGerais,
            os.DefeitoRelatado,
            os.LaudoTecnico,
            os.SolucaoTecnica,
            os.ValorServicos,
            os.ValorPecas,
            os.ValorDesconto,
            os.ValorTotal,
            os.FormaPagamentoId,
            os.FormaPagamento?.Nome,
            os.NumeroParcelas,
            os.IdParcelamento,
            os.Observacoes,
            os.TermoGarantia,
            os.DataCancelamento,
            os.MotivoCancelamento,
            os.ItensPecas.Select(p => new OrdemServicoPecaDto(
                p.Id,
                p.ProdutoId,
                p.Produto?.Codigo,
                p.Produto?.Nome,
                p.Descricao,
                p.Quantidade,
                p.ValorUnitario,
                p.ValorDesconto,
                p.ValorTotal,
                p.CustoUnitario,
                p.DepositoId,
                p.Deposito?.Nome,
                p.BaixadoEstoque
            )).ToList(),
            os.ItensServicos.Select(s => new OrdemServicoServicoDto(
                s.Id,
                s.ServicoId,
                s.Servico?.Codigo,
                s.Servico?.Nome,
                s.Descricao,
                s.Quantidade,
                s.ValorUnitario,
                s.ValorDesconto,
                s.ValorTotal,
                s.Tecnico
            )).ToList(),
            os.HistoricoStatus.OrderByDescending(h => h.Data).Select(h => new OrdemServicoHistoricoDto(
                h.Id,
                h.StatusAnterior,
                h.StatusNovo,
                h.Data,
                h.UsuarioNome,
                h.Observacao
            )).ToList()
        );
    }

    public async Task<string> Handle(GetProximoNumeroOrdemServicoQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        int ano = DateTime.UtcNow.Year;

        int totalAno = await _context.OrdensServico
            .CountAsync(o => o.EmpresaId == empresaId && o.DataAbertura.Year == ano, cancellationToken);

        return $"OS-{ano}-{(totalAno + 1):D4}";
    }

    public async Task<ResumoDashboardOrdensServicoDto> Handle(GetResumoDashboardOrdensServicoQuery request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var agora = DateTime.UtcNow;
        var inicioMes = new DateTime(agora.Year, agora.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var todas = await _context.OrdensServico
            .AsNoTracking()
            .Where(o => o.EmpresaId == empresaId)
            .Select(o => new { o.Status, o.ValorTotal, o.DataConclusao })
            .ToListAsync(cancellationToken);

        int totalGeral = todas.Count;
        int abertas = todas.Count(o => o.Status == StatusOrdemServico.Aberta);
        int aguardando = todas.Count(o => o.Status == StatusOrdemServico.AguardandoAprovacao);
        int emExecucao = todas.Count(o => o.Status == StatusOrdemServico.Aprovada || o.Status == StatusOrdemServico.AguardandoPecas);
        int concluidas = todas.Count(o => o.Status == StatusOrdemServico.Concluida);
        int faturadas = todas.Count(o => o.Status == StatusOrdemServico.Faturada);

        decimal valorFaturadoMes = todas
            .Where(o => o.Status == StatusOrdemServico.Faturada && o.DataConclusao >= inicioMes)
            .Sum(o => o.ValorTotal);

        return new ResumoDashboardOrdensServicoDto(
            totalGeral,
            abertas,
            aguardando,
            emExecucao,
            concluidas,
            faturadas,
            valorFaturadoMes
        );
    }
}
