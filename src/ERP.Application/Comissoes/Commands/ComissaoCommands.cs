using ERP.Application.Comissoes.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Core.Entities.Comissoes;
using ERP.Domain.Core.Entities.Vendas;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Comissoes.Commands;

public record CriarVendedorCommand(SalvarVendedorDto Dto) : IRequest<int>;
public record AtualizarVendedorCommand(int Id, SalvarVendedorDto Dto) : IRequest<bool>;

public record CriarRegraComissaoCommand(SalvarRegraComissaoDto Dto) : IRequest<int>;
public record AtualizarRegraComissaoCommand(int Id, SalvarRegraComissaoDto Dto) : IRequest<bool>;
public record ExcluirRegraComissaoCommand(int Id) : IRequest<bool>;

public record AprovarComissaoCommand(int Id) : IRequest<bool>;
public record PagarComissaoCommand(int Id) : IRequest<bool>;
public record CancelarComissaoCommand(int Id) : IRequest<bool>;
public record AprovarComissoesLoteCommand(List<int> Ids) : IRequest<int>;
public record PagarComissoesLoteCommand(List<int> Ids) : IRequest<int>;
public record ProcessarComissoesPendentesCommand() : IRequest<int>;

public class ComissaoCommandsHandler :
    IRequestHandler<CriarVendedorCommand, int>,
    IRequestHandler<AtualizarVendedorCommand, bool>,
    IRequestHandler<CriarRegraComissaoCommand, int>,
    IRequestHandler<AtualizarRegraComissaoCommand, bool>,
    IRequestHandler<ExcluirRegraComissaoCommand, bool>,
    IRequestHandler<AprovarComissaoCommand, bool>,
    IRequestHandler<PagarComissaoCommand, bool>,
    IRequestHandler<CancelarComissaoCommand, bool>,
    IRequestHandler<AprovarComissoesLoteCommand, int>,
    IRequestHandler<PagarComissoesLoteCommand, int>,
    IRequestHandler<ProcessarComissoesPendentesCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public ComissaoCommandsHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<int> Handle(CriarVendedorCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        var vendedor = new Vendedor
        {
            EmpresaId = empresaId,
            UsuarioId = dto.UsuarioId,
            Nome = dto.Nome.Trim(),
            Email = dto.Email?.Trim(),
            Telefone = dto.Telefone?.Trim(),
            Cpf = dto.Cpf?.Trim(),
            TipoComissaoPadrao = dto.TipoComissaoPadrao,
            PercentualPadrao = dto.PercentualPadrao,
            ValorFixoPadrao = dto.ValorFixoPadrao,
            Observacoes = dto.Observacoes?.Trim(),
            Ativo = dto.Ativo
        };

        _context.Vendedores.Add(vendedor);
        await _context.SaveChangesAsync(cancellationToken);
        return vendedor.Id;
    }

    public async Task<bool> Handle(AtualizarVendedorCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var v = await _context.Vendedores
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);
        if (v == null) return false;

        var dto = request.Dto;
        v.UsuarioId = dto.UsuarioId;
        v.Nome = dto.Nome.Trim();
        v.Email = dto.Email?.Trim();
        v.Telefone = dto.Telefone?.Trim();
        v.Cpf = dto.Cpf?.Trim();
        v.TipoComissaoPadrao = dto.TipoComissaoPadrao;
        v.PercentualPadrao = dto.PercentualPadrao;
        v.ValorFixoPadrao = dto.ValorFixoPadrao;
        v.Observacoes = dto.Observacoes?.Trim();
        v.Ativo = dto.Ativo;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(CriarRegraComissaoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        var regra = new RegraComissao
        {
            EmpresaId = empresaId,
            VendedorId = dto.VendedorId,
            Nome = dto.Nome.Trim(),
            Tipo = dto.Tipo,
            Percentual = dto.Percentual,
            ValorFixo = dto.ValorFixo,
            ValorMinimoVenda = dto.ValorMinimoVenda,
            Prioridade = dto.Prioridade,
            Ativo = dto.Ativo
        };

        _context.RegrasComissao.Add(regra);
        await _context.SaveChangesAsync(cancellationToken);
        return regra.Id;
    }

    public async Task<bool> Handle(AtualizarRegraComissaoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var r = await _context.RegrasComissao
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);
        if (r == null) return false;

        var dto = request.Dto;
        r.VendedorId = dto.VendedorId;
        r.Nome = dto.Nome.Trim();
        r.Tipo = dto.Tipo;
        r.Percentual = dto.Percentual;
        r.ValorFixo = dto.ValorFixo;
        r.ValorMinimoVenda = dto.ValorMinimoVenda;
        r.Prioridade = dto.Prioridade;
        r.Ativo = dto.Ativo;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(ExcluirRegraComissaoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var r = await _context.RegrasComissao
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);
        if (r == null) return false;

        _context.RegrasComissao.Remove(r);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(AprovarComissaoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var c = await _context.ComissoesVenda
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);
        if (c == null || c.Status != StatusComissao.Pendente) return false;

        c.Status = StatusComissao.Aprovada;
        c.DataAprovacao = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(PagarComissaoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var c = await _context.ComissoesVenda
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);
        if (c == null || (c.Status != StatusComissao.Pendente && c.Status != StatusComissao.Aprovada)) return false;

        c.Status = StatusComissao.Paga;
        c.DataAprovacao ??= DateTime.UtcNow;
        c.DataPagamento = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(CancelarComissaoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var c = await _context.ComissoesVenda
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.EmpresaId == empresaId, cancellationToken);
        if (c == null || c.Status == StatusComissao.Paga) return false;

        c.Status = StatusComissao.Cancelada;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(AprovarComissoesLoteCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var lista = await _context.ComissoesVenda
            .Where(x => x.EmpresaId == empresaId && request.Ids.Contains(x.Id) && x.Status == StatusComissao.Pendente)
            .ToListAsync(cancellationToken);

        foreach (var c in lista)
        {
            c.Status = StatusComissao.Aprovada;
            c.DataAprovacao = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(cancellationToken);
        return lista.Count;
    }

    public async Task<int> Handle(PagarComissoesLoteCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var lista = await _context.ComissoesVenda
            .Where(x => x.EmpresaId == empresaId && request.Ids.Contains(x.Id) && (x.Status == StatusComissao.Pendente || x.Status == StatusComissao.Aprovada))
            .ToListAsync(cancellationToken);

        foreach (var c in lista)
        {
            c.Status = StatusComissao.Paga;
            c.DataAprovacao ??= DateTime.UtcNow;
            c.DataPagamento = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(cancellationToken);
        return lista.Count;
    }

    public async Task<int> Handle(ProcessarComissoesPendentesCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var pedidosFaturados = await _context.PedidosVenda
            .Where(p => p.EmpresaId == empresaId
                        && p.Status == StatusPedidoVenda.Faturado
                        && (!string.IsNullOrEmpty(p.VendedorId) || !string.IsNullOrEmpty(p.VendedorNome)))
            .ToListAsync(cancellationToken);

        var comissoesExistentesIds = await _context.ComissoesVenda
            .Where(c => c.EmpresaId == empresaId)
            .Select(c => c.PedidoVendaId)
            .ToListAsync(cancellationToken);

        int geradas = 0;
        foreach (var pedido in pedidosFaturados.Where(p => !comissoesExistentesIds.Contains(p.Id)))
        {
            var fat = await _context.Faturamentos
                .Where(f => f.PedidoVendaId == pedido.Id && f.Status != StatusFaturamento.Cancelado)
                .OrderByDescending(f => f.DataFaturamento)
                .FirstOrDefaultAsync(cancellationToken);

            var comissao = await CalcularEGerarComissaoAsync(_context, empresaId, pedido, fat?.Id, cancellationToken);
            if (comissao != null)
                geradas++;
        }

        if (geradas > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return geradas;
    }

    /// <summary>
    /// Calcula e adiciona uma <see cref="ComissaoVenda"/> para um pedido faturado caso haja vendedor cadastrado.
    /// Chamado automaticamente no faturamento de vendas e no processamento retroativo.
    /// </summary>
    public static async Task<ComissaoVenda?> CalcularEGerarComissaoAsync(
        IApplicationDbContext context,
        int empresaId,
        PedidoVenda pedido,
        int? faturamentoId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(pedido.VendedorId) && string.IsNullOrWhiteSpace(pedido.VendedorNome))
            return null;

        bool jaExiste = await context.ComissoesVenda
            .AnyAsync(c => c.EmpresaId == empresaId && c.PedidoVendaId == pedido.Id, cancellationToken);
        if (jaExiste)
            return null;

        Vendedor? vendedor = null;
        if (!string.IsNullOrWhiteSpace(pedido.VendedorId))
        {
            vendedor = await context.Vendedores
                .FirstOrDefaultAsync(v => v.EmpresaId == empresaId && v.UsuarioId == pedido.VendedorId && v.Ativo, cancellationToken);
        }
        if (vendedor == null && !string.IsNullOrWhiteSpace(pedido.VendedorNome))
        {
            var nomeLower = pedido.VendedorNome.Trim().ToLower();
            vendedor = await context.Vendedores
                .FirstOrDefaultAsync(v => v.EmpresaId == empresaId && v.Nome.ToLower() == nomeLower && v.Ativo, cancellationToken);
        }

        if (vendedor == null)
            return null;

        // Busca regra aplicável de maior prioridade (específica do vendedor ou geral)
        var regras = await context.RegrasComissao
            .Where(r => r.EmpresaId == empresaId && r.Ativo && (r.VendedorId == vendedor.Id || r.VendedorId == null))
            .OrderByDescending(r => r.VendedorId.HasValue)
            .ThenByDescending(r => r.Prioridade)
            .ToListAsync(cancellationToken);

        var regraAplicada = regras.FirstOrDefault(r => !r.ValorMinimoVenda.HasValue || pedido.ValorTotal >= r.ValorMinimoVenda.Value);

        decimal percentualAplicado = 0m;
        decimal valorComissao = 0m;

        if (regraAplicada != null)
        {
            if (regraAplicada.Tipo == TipoRegraComissao.PercentualSobreVenda)
            {
                percentualAplicado = regraAplicada.Percentual;
                valorComissao = Math.Round(pedido.ValorTotal * (percentualAplicado / 100m), 2);
            }
            else
            {
                valorComissao = regraAplicada.ValorFixo;
            }
        }
        else
        {
            if (vendedor.TipoComissaoPadrao == TipoRegraComissao.PercentualSobreVenda)
            {
                percentualAplicado = vendedor.PercentualPadrao;
                valorComissao = Math.Round(pedido.ValorTotal * (percentualAplicado / 100m), 2);
            }
            else
            {
                valorComissao = vendedor.ValorFixoPadrao;
            }
        }

        var comissao = new ComissaoVenda
        {
            EmpresaId = empresaId,
            VendedorId = vendedor.Id,
            PedidoVendaId = pedido.Id,
            FaturamentoId = faturamentoId,
            RegraComissaoId = regraAplicada?.Id,
            DataVenda = DateTime.UtcNow,
            ValorBaseVenda = pedido.ValorTotal,
            PercentualAplicado = percentualAplicado,
            ValorComissao = valorComissao,
            Status = StatusComissao.Pendente
        };

        context.ComissoesVenda.Add(comissao);
        return comissao;
    }
}
