using ERP.Application.Common.Interfaces;
using ERP.Application.Vendas.Common;
using ERP.Application.Vendas.DTOs;
using ERP.Domain.Core.Entities.Vendas;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Vendas.Commands;

public record CriarOrcamentoCommand(CriarOrcamentoDto Dto) : IRequest<int>;
public record AtualizarOrcamentoCommand(int Id, AtualizarOrcamentoDto Dto) : IRequest<bool>;
public record RejeitarOrcamentoCommand(int Id, string? Motivo) : IRequest<bool>;
public record CancelarOrcamentoCommand(int Id) : IRequest<bool>;
public record ConverterOrcamentoEmPedidoCommand(int Id) : IRequest<int>;

public class OrcamentoCommandsHandler :
    IRequestHandler<CriarOrcamentoCommand, int>,
    IRequestHandler<AtualizarOrcamentoCommand, bool>,
    IRequestHandler<RejeitarOrcamentoCommand, bool>,
    IRequestHandler<CancelarOrcamentoCommand, bool>,
    IRequestHandler<ConverterOrcamentoEmPedidoCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public OrcamentoCommandsHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<int> Handle(CriarOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == dto.ClienteId, cancellationToken);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} não encontrado.");

        if (dto.Itens == null || !dto.Itens.Any())
            throw new InvalidOperationException("O orçamento deve conter pelo menos um item.");

        int ano = dto.DataOrcamento.Year;
        int totalAno = await _context.Orcamentos
            .CountAsync(o => o.EmpresaId == empresaId && o.DataOrcamento.Year == ano, cancellationToken);
        string numero = $"ORC-{ano}-{(totalAno + 1):D4}";

        var orcamento = new Orcamento
        {
            EmpresaId = empresaId,
            Numero = numero,
            ClienteId = dto.ClienteId,
            VendedorId = dto.VendedorId,
            VendedorNome = dto.VendedorNome?.Trim(),
            DataOrcamento = dto.DataOrcamento.ToUniversalTime(),
            DataValidade = dto.DataValidade.ToUniversalTime(),
            Status = StatusOrcamento.Aberto,
            DescontoTipo = dto.DescontoTipo,
            DescontoGeral = dto.DescontoGeral,
            Observacao = dto.Observacao?.Trim()
        };

        var itensEntidade = new List<OrcamentoItem>();
        foreach (var itemDto in dto.Itens)
        {
            var itemEntidade = await MontarItemOrcamentoAsync(empresaId, itemDto, cancellationToken);
            itensEntidade.Add(itemEntidade);
        }

        var totais = VendaCalculosHelper.CalcularTotais(
            itensEntidade.Select(i => i.SubTotal),
            dto.DescontoTipo,
            dto.DescontoGeral
        );

        orcamento.ValorBruto = totais.ValorBruto;
        orcamento.ValorDesconto = totais.ValorDescontoGeral;
        orcamento.ValorTotal = totais.ValorTotal;
        orcamento.Itens = itensEntidade;

        _context.Orcamentos.Add(orcamento);
        await _context.SaveChangesAsync(cancellationToken);

        return orcamento.Id;
    }

    public async Task<bool> Handle(AtualizarOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var orcamento = await _context.Orcamentos
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (orcamento == null)
            throw new KeyNotFoundException($"Orçamento com ID {request.Id} não encontrado.");

        if (orcamento.Status != StatusOrcamento.Aberto)
            throw new InvalidOperationException($"Não é possível editar um orçamento com status '{orcamento.Status}'.");

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == dto.ClienteId, cancellationToken);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} não encontrado.");

        if (dto.Itens == null || !dto.Itens.Any())
            throw new InvalidOperationException("O orçamento deve conter pelo menos um item.");

        orcamento.ClienteId = dto.ClienteId;
        orcamento.VendedorId = dto.VendedorId;
        orcamento.VendedorNome = dto.VendedorNome?.Trim();
        orcamento.DataOrcamento = dto.DataOrcamento.ToUniversalTime();
        orcamento.DataValidade = dto.DataValidade.ToUniversalTime();
        orcamento.DescontoTipo = dto.DescontoTipo;
        orcamento.DescontoGeral = dto.DescontoGeral;
        orcamento.Observacao = dto.Observacao?.Trim();

        // Remove itens antigos
        _context.OrcamentoItens.RemoveRange(orcamento.Itens);
        orcamento.Itens.Clear();

        // Adiciona itens novos
        var novosItens = new List<OrcamentoItem>();
        foreach (var itemDto in dto.Itens)
        {
            var item = await MontarItemOrcamentoAsync(empresaId, itemDto, cancellationToken);
            novosItens.Add(item);
        }

        var totais = VendaCalculosHelper.CalcularTotais(
            novosItens.Select(i => i.SubTotal),
            dto.DescontoTipo,
            dto.DescontoGeral
        );

        orcamento.ValorBruto = totais.ValorBruto;
        orcamento.ValorDesconto = totais.ValorDescontoGeral;
        orcamento.ValorTotal = totais.ValorTotal;
        orcamento.Itens = novosItens;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(RejeitarOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var orcamento = await _context.Orcamentos.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (orcamento == null) return false;

        if (orcamento.Status != StatusOrcamento.Aberto)
            throw new InvalidOperationException($"Apenas orçamentos com status Aberto podem ser rejeitados.");

        orcamento.Status = StatusOrcamento.Rejeitado;
        if (!string.IsNullOrWhiteSpace(request.Motivo))
        {
            orcamento.Observacao = string.IsNullOrWhiteSpace(orcamento.Observacao)
                ? $"Rejeitado: {request.Motivo}"
                : $"{orcamento.Observacao} | Rejeitado: {request.Motivo}";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(CancelarOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var orcamento = await _context.Orcamentos.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (orcamento == null) return false;

        if (orcamento.Status == StatusOrcamento.Convertido)
            throw new InvalidOperationException("Não é possível cancelar um orçamento já convertido em pedido de venda.");

        orcamento.Status = StatusOrcamento.Cancelado;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(ConverterOrcamentoEmPedidoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var orcamento = await _context.Orcamentos
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (orcamento == null)
            throw new KeyNotFoundException($"Orçamento com ID {request.Id} não encontrado.");

        if (orcamento.Status != StatusOrcamento.Aberto)
            throw new InvalidOperationException($"Apenas orçamentos com status 'Aberto' podem ser convertidos em pedido de venda. Status atual: '{orcamento.Status}'.");

        int ano = DateTime.UtcNow.Year;
        int totalAno = await _context.PedidosVenda
            .CountAsync(p => p.EmpresaId == empresaId && p.DataPedido.Year == ano, cancellationToken);
        string numeroPedido = $"PED-{ano}-{(totalAno + 1):D4}";

        var pedido = new PedidoVenda
        {
            EmpresaId = empresaId,
            Numero = numeroPedido,
            ClienteId = orcamento.ClienteId,
            OrcamentoId = orcamento.Id,
            VendedorId = orcamento.VendedorId,
            VendedorNome = orcamento.VendedorNome,
            DataPedido = DateTime.UtcNow,
            Status = StatusPedidoVenda.Pendente,
            NumeroParcelas = 1,
            IntervaloDias = 30,
            DiasPrimeiroVencimento = 0,
            ValorBruto = orcamento.ValorBruto,
            DescontoTipo = orcamento.DescontoTipo,
            DescontoGeral = orcamento.DescontoGeral,
            ValorDesconto = orcamento.ValorDesconto,
            ValorTotal = orcamento.ValorTotal,
            Observacao = $"Convertido a partir do Orçamento {orcamento.Numero}. {orcamento.Observacao}".Trim()
        };

        var itensPedido = new List<PedidoVendaItem>();
        foreach (var itemOrc in orcamento.Itens)
        {
            itensPedido.Add(new PedidoVendaItem
            {
                EmpresaId = empresaId,
                TipoItem = itemOrc.TipoItem,
                ProdutoId = itemOrc.ProdutoId,
                ServicoId = itemOrc.ServicoId,
                Descricao = itemOrc.Descricao,
                Quantidade = itemOrc.Quantidade,
                PrecoUnitario = itemOrc.PrecoUnitario,
                DescontoTipo = itemOrc.DescontoTipo,
                Desconto = itemOrc.Desconto,
                ValorDesconto = itemOrc.ValorDesconto,
                SubTotal = itemOrc.SubTotal,
                CustoUnitario = 0m
            });
        }
        pedido.Itens = itensPedido;

        _context.PedidosVenda.Add(pedido);
        await _context.SaveChangesAsync(cancellationToken);

        // Atualiza orçamento
        orcamento.Status = StatusOrcamento.Convertido;
        orcamento.PedidoVendaId = pedido.Id;
        await _context.SaveChangesAsync(cancellationToken);

        return pedido.Id;
    }

    private async Task<OrcamentoItem> MontarItemOrcamentoAsync(
        int empresaId,
        ItemVendaInputDto dto,
        CancellationToken cancellationToken)
    {
        string descricao = dto.Descricao?.Trim() ?? string.Empty;
        decimal precoUnitario = dto.PrecoUnitario;

        if (dto.TipoItem == TipoItemVenda.Produto)
        {
            if (!dto.ProdutoId.HasValue)
                throw new InvalidOperationException("Para itens do tipo Produto, o ProdutoId é obrigatório.");

            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == dto.ProdutoId.Value, cancellationToken);
            if (produto == null)
                throw new KeyNotFoundException($"Produto {dto.ProdutoId} não encontrado.");

            if (string.IsNullOrWhiteSpace(descricao))
                descricao = produto.Nome;

            if (precoUnitario <= 0)
                precoUnitario = produto.PrecoVenda;
        }
        else
        {
            if (!dto.ServicoId.HasValue)
                throw new InvalidOperationException("Para itens do tipo Serviço, o ServicoId é obrigatório.");

            var servico = await _context.Servicos.FirstOrDefaultAsync(s => s.Id == dto.ServicoId.Value, cancellationToken);
            if (servico == null)
                throw new KeyNotFoundException($"Serviço {dto.ServicoId} não encontrado.");

            if (string.IsNullOrWhiteSpace(descricao))
                descricao = servico.Nome;

            if (precoUnitario <= 0)
                precoUnitario = servico.PrecoBase;
        }

        var calculo = VendaCalculosHelper.CalcularItem(
            dto.Quantidade,
            precoUnitario,
            dto.DescontoTipo,
            dto.Desconto
        );

        return new OrcamentoItem
        {
            EmpresaId = empresaId,
            TipoItem = dto.TipoItem,
            ProdutoId = dto.TipoItem == TipoItemVenda.Produto ? dto.ProdutoId : null,
            ServicoId = dto.TipoItem == TipoItemVenda.Servico ? dto.ServicoId : null,
            Descricao = descricao,
            Quantidade = dto.Quantidade,
            PrecoUnitario = precoUnitario,
            DescontoTipo = dto.DescontoTipo,
            Desconto = dto.Desconto,
            ValorDesconto = calculo.ValorDesconto,
            SubTotal = calculo.SubTotal
        };
    }
}
