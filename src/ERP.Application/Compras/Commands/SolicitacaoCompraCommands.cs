using ERP.Application.Common.Interfaces;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Entities.Compras;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Compras.Commands;

public record CriarSolicitacaoCompraCommand(CriarSolicitacaoCompraDto Dto) : IRequest<int>;

public class CriarSolicitacaoCompraCommandHandler : IRequestHandler<CriarSolicitacaoCompraCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CriarSolicitacaoCompraCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<int> Handle(CriarSolicitacaoCompraCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (dto.Itens == null || !dto.Itens.Any())
            throw new InvalidOperationException("A solicitação de compra deve conter ao menos um item.");

        var empresaId = _empresaService.EmpresaId ?? 1;
        var ano = DateTime.UtcNow.Year;
        var countAno = await _context.SolicitacoesCompra
            .IgnoreQueryFilters()
            .CountAsync(s => s.EmpresaId == empresaId && s.CriadoEm.Year == ano, cancellationToken);
        var numeroFormatado = $"SOL-{ano}-{(countAno + 1):D6}";

        var solicitacao = new SolicitacaoCompra
        {
            EmpresaId = empresaId,
            Numero = numeroFormatado,
            SolicitanteId = "usuario_atual",
            SolicitanteNome = "Usuário do Sistema",
            DataSolicitacao = DateTime.UtcNow,
            DataNecessidade = dto.DataNecessidade,
            Status = StatusSolicitacaoCompra.Pendente,
            Prioridade = dto.Prioridade,
            Departamento = dto.Departamento,
            Justificativa = dto.Justificativa,
            Observacoes = dto.Observacoes,
            CriadoEm = DateTime.UtcNow,
            Ativo = true
        };

        foreach (var item in dto.Itens)
        {
            if (item.Quantidade <= 0)
                throw new InvalidOperationException("A quantidade solicitada deve ser maior que zero.");

            solicitacao.Itens.Add(new SolicitacaoCompraItem
            {
                EmpresaId = empresaId,
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade,
                QuantidadeAtendida = 0,
                Observacao = item.Observacao,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            });
        }

        _context.SolicitacoesCompra.Add(solicitacao);
        await _context.SaveChangesAsync(cancellationToken);

        return solicitacao.Id;
    }
}

public record AprovarSolicitacaoCompraCommand(int Id, string AprovadorNome) : IRequest<bool>;

public class AprovarSolicitacaoCompraCommandHandler : IRequestHandler<AprovarSolicitacaoCompraCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AprovarSolicitacaoCompraCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AprovarSolicitacaoCompraCommand request, CancellationToken cancellationToken)
    {
        var sol = await _context.SolicitacoesCompra.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (sol == null)
            throw new KeyNotFoundException("Solicitação de compra não encontrada.");

        if (sol.Status != StatusSolicitacaoCompra.Pendente)
            throw new InvalidOperationException($"Não é possível aprovar uma solicitação com status '{sol.Status}'.");

        sol.Status = StatusSolicitacaoCompra.Aprovada;
        sol.DataAprovacao = DateTime.UtcNow;
        sol.AprovadorNome = request.AprovadorNome;
        sol.AlteradoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record RejeitarSolicitacaoCompraCommand(int Id, string Motivo) : IRequest<bool>;

public class RejeitarSolicitacaoCompraCommandHandler : IRequestHandler<RejeitarSolicitacaoCompraCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RejeitarSolicitacaoCompraCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RejeitarSolicitacaoCompraCommand request, CancellationToken cancellationToken)
    {
        var sol = await _context.SolicitacoesCompra.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (sol == null)
            throw new KeyNotFoundException("Solicitação de compra não encontrada.");

        if (sol.Status != StatusSolicitacaoCompra.Pendente)
            throw new InvalidOperationException($"Não é possível rejeitar uma solicitação com status '{sol.Status}'.");

        sol.Status = StatusSolicitacaoCompra.Rejeitada;
        sol.MotivoRejeicao = request.Motivo;
        sol.AlteradoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record CancelarSolicitacaoCompraCommand(int Id) : IRequest<bool>;

public class CancelarSolicitacaoCompraCommandHandler : IRequestHandler<CancelarSolicitacaoCompraCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CancelarSolicitacaoCompraCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelarSolicitacaoCompraCommand request, CancellationToken cancellationToken)
    {
        var sol = await _context.SolicitacoesCompra.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (sol == null)
            throw new KeyNotFoundException("Solicitação de compra não encontrada.");

        if (sol.Status == StatusSolicitacaoCompra.ConvertidaPedido)
            throw new InvalidOperationException("Solicitação já convertida em pedido de compra não pode ser cancelada diretamente.");

        sol.Status = StatusSolicitacaoCompra.Cancelada;
        sol.AlteradoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
