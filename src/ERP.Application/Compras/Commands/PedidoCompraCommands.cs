using ERP.Application.Common.Interfaces;
using ERP.Application.Compras.DTOs;
using ERP.Domain.Core.Entities.Compras;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Compras.Commands;

public record CriarPedidoCompraCommand(CriarPedidoCompraDto Dto) : IRequest<int>;

public class CriarPedidoCompraCommandHandler : IRequestHandler<CriarPedidoCompraCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CriarPedidoCompraCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<int> Handle(CriarPedidoCompraCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (dto.Itens == null || !dto.Itens.Any())
            throw new InvalidOperationException("O pedido de compra deve conter ao menos um item.");

        var empresaId = _empresaService.EmpresaId ?? 1;
        var ano = DateTime.UtcNow.Year;
        var countAno = await _context.PedidosCompra
            .IgnoreQueryFilters()
            .CountAsync(p => p.EmpresaId == empresaId && p.CriadoEm.Year == ano, cancellationToken);
        var numeroFormatado = $"PC-{ano}-{(countAno + 1):D6}";

        decimal valorBruto = 0m;
        decimal valorDesconto = 0m;

        var pedido = new PedidoCompra
        {
            EmpresaId = empresaId,
            Numero = numeroFormatado,
            FornecedorId = dto.FornecedorId,
            SolicitacaoCompraId = dto.SolicitacaoCompraId,
            DataPedido = dto.DataPedido,
            PrevisaoEntrega = dto.PrevisaoEntrega,
            Status = StatusPedidoCompra.Pendente,
            CompradorNome = dto.CompradorNome,
            DepositoDestinoId = dto.DepositoDestinoId,
            FormaPagamentoId = dto.FormaPagamentoId,
            NumeroParcelas = dto.NumeroParcelas > 0 ? dto.NumeroParcelas : 1,
            IntervaloDias = dto.IntervaloDias > 0 ? dto.IntervaloDias : 30,
            DiasPrimeiroVencimento = dto.DiasPrimeiroVencimento,
            ValorFrete = dto.ValorFrete,
            ValorSeguro = dto.ValorSeguro,
            ValorOutrasDespesas = dto.ValorOutrasDespesas,
            Observacoes = dto.Observacoes,
            CriadoEm = DateTime.UtcNow,
            Ativo = true
        };

        foreach (var item in dto.Itens)
        {
            if (item.Quantidade <= 0)
                throw new InvalidOperationException("A quantidade do item deve ser maior que zero.");
            if (item.PrecoUnitario <= 0)
                throw new InvalidOperationException("O preço unitário deve ser maior que zero.");

            var itemSubTotal = (item.Quantidade * item.PrecoUnitario) - item.ValorDesconto;
            if (itemSubTotal < 0) itemSubTotal = 0;

            valorBruto += (item.Quantidade * item.PrecoUnitario);
            valorDesconto += item.ValorDesconto;

            pedido.Itens.Add(new PedidoCompraItem
            {
                EmpresaId = empresaId,
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade,
                QuantidadeEntregue = 0,
                PrecoUnitario = item.PrecoUnitario,
                ValorDesconto = item.ValorDesconto,
                SubTotal = itemSubTotal,
                Observacao = item.Observacao,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            });
        }

        pedido.ValorBruto = valorBruto;
        pedido.ValorDesconto = valorDesconto;
        pedido.ValorTotal = (valorBruto - valorDesconto) + dto.ValorFrete + dto.ValorSeguro + dto.ValorOutrasDespesas;

        _context.PedidosCompra.Add(pedido);

        // Se veio de solicitação, atualiza a solicitação para ConvertidaPedido
        if (dto.SolicitacaoCompraId.HasValue)
        {
            var sol = await _context.SolicitacoesCompra.FirstOrDefaultAsync(s => s.Id == dto.SolicitacaoCompraId.Value, cancellationToken);
            if (sol != null)
            {
                sol.Status = StatusSolicitacaoCompra.ConvertidaPedido;
                sol.AlteradoEm = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return pedido.Id;
    }
}

public record AprovarPedidoCompraCommand(int Id) : IRequest<bool>;

public class AprovarPedidoCompraCommandHandler : IRequestHandler<AprovarPedidoCompraCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AprovarPedidoCompraCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AprovarPedidoCompraCommand request, CancellationToken cancellationToken)
    {
        var pedido = await _context.PedidosCompra.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (pedido == null)
            throw new KeyNotFoundException("Pedido de compra não encontrado.");

        if (pedido.Status != StatusPedidoCompra.Pendente)
            throw new InvalidOperationException($"Não é possível aprovar um pedido com status '{pedido.Status}'.");

        pedido.Status = StatusPedidoCompra.Aprovado;
        pedido.DataAprovacao = DateTime.UtcNow;
        pedido.AlteradoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record CancelarPedidoCompraCommand(int Id, string Motivo) : IRequest<bool>;

public class CancelarPedidoCompraCommandHandler : IRequestHandler<CancelarPedidoCompraCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CancelarPedidoCompraCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelarPedidoCompraCommand request, CancellationToken cancellationToken)
    {
        var pedido = await _context.PedidosCompra
            .Include(p => p.Entradas)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pedido == null)
            throw new KeyNotFoundException("Pedido de compra não encontrado.");

        if (pedido.Entradas.Any(e => e.Status == StatusEntradaMercadoria.Conferida))
            throw new InvalidOperationException("Não é possível cancelar um pedido que já possui entregas conferidas. Cancele as entradas primeiro.");

        pedido.Status = StatusPedidoCompra.Cancelado;
        pedido.DataCancelamento = DateTime.UtcNow;
        pedido.MotivoCancelamento = request.Motivo;
        pedido.AlteradoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record ConverterSolicitacaoEmPedidoCommand(int SolicitacaoId, int FornecedorId, int DepositoDestinoId) : IRequest<int>;

public class ConverterSolicitacaoEmPedidoCommandHandler : IRequestHandler<ConverterSolicitacaoEmPedidoCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public ConverterSolicitacaoEmPedidoCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<int> Handle(ConverterSolicitacaoEmPedidoCommand request, CancellationToken cancellationToken)
    {
        var sol = await _context.SolicitacoesCompra
            .Include(s => s.Itens)
                .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(s => s.Id == request.SolicitacaoId, cancellationToken);

        if (sol == null)
            throw new KeyNotFoundException("Solicitação de compra não encontrada.");

        if (sol.Status != StatusSolicitacaoCompra.Aprovada && sol.Status != StatusSolicitacaoCompra.Cotada)
            throw new InvalidOperationException("Apenas solicitações Aprovadas ou Cotadas podem ser convertidas em Pedido de Compra.");

        var empresaId = _empresaService.EmpresaId ?? 1;
        var ano = DateTime.UtcNow.Year;
        var countAno = await _context.PedidosCompra
            .IgnoreQueryFilters()
            .CountAsync(p => p.EmpresaId == empresaId && p.CriadoEm.Year == ano, cancellationToken);
        var numeroFormatado = $"PC-{ano}-{(countAno + 1):D6}";

        var pedido = new PedidoCompra
        {
            EmpresaId = empresaId,
            Numero = numeroFormatado,
            FornecedorId = request.FornecedorId,
            SolicitacaoCompraId = sol.Id,
            DataPedido = DateTime.UtcNow,
            PrevisaoEntrega = sol.DataNecessidade ?? DateTime.UtcNow.AddDays(7),
            Status = StatusPedidoCompra.Pendente,
            CompradorNome = "Comprador Responsável",
            DepositoDestinoId = request.DepositoDestinoId,
            NumeroParcelas = 1,
            IntervaloDias = 30,
            Observacoes = $"Gerado a partir da Solicitação {sol.Numero}. {sol.Observacoes}",
            CriadoEm = DateTime.UtcNow,
            Ativo = true
        };

        decimal valorBruto = 0m;
        foreach (var item in sol.Itens)
        {
            var precoBase = item.Produto.PrecoCusto > 0 ? item.Produto.PrecoCusto : 1m;
            var subTotal = item.Quantidade * precoBase;
            valorBruto += subTotal;

            pedido.Itens.Add(new PedidoCompraItem
            {
                EmpresaId = empresaId,
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade,
                QuantidadeEntregue = 0,
                PrecoUnitario = precoBase,
                ValorDesconto = 0,
                SubTotal = subTotal,
                Observacao = item.Observacao,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            });
        }

        pedido.ValorBruto = valorBruto;
        pedido.ValorTotal = valorBruto;

        _context.PedidosCompra.Add(pedido);

        sol.Status = StatusSolicitacaoCompra.ConvertidaPedido;
        sol.AlteradoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return pedido.Id;
    }
}
