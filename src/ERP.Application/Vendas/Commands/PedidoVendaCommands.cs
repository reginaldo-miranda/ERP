using ERP.Application.Common.Interfaces;
using ERP.Application.Vendas.Common;
using ERP.Application.Vendas.DTOs;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Entities.Vendas;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Vendas.Commands;

public record CriarPedidoVendaCommand(CriarPedidoVendaDto Dto) : IRequest<int>;
public record AtualizarPedidoVendaCommand(int Id, AtualizarPedidoVendaDto Dto) : IRequest<bool>;
public record AprovarPedidoVendaCommand(int Id) : IRequest<bool>;
public record CancelarPedidoVendaCommand(int Id, string? Motivo) : IRequest<bool>;
public record FaturarPedidoVendaCommand(FaturarPedidoDto Dto) : IRequest<int>;

public class PedidoVendaCommandsHandler :
    IRequestHandler<CriarPedidoVendaCommand, int>,
    IRequestHandler<AtualizarPedidoVendaCommand, bool>,
    IRequestHandler<AprovarPedidoVendaCommand, bool>,
    IRequestHandler<CancelarPedidoVendaCommand, bool>,
    IRequestHandler<FaturarPedidoVendaCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public PedidoVendaCommandsHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<int> Handle(CriarPedidoVendaCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == dto.ClienteId, cancellationToken);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} não encontrado.");

        if (dto.Itens == null || !dto.Itens.Any())
            throw new InvalidOperationException("O pedido de venda deve conter pelo menos um item.");

        int ano = dto.DataPedido.Year;
        int totalAno = await _context.PedidosVenda
            .CountAsync(p => p.EmpresaId == empresaId && p.DataPedido.Year == ano, cancellationToken);
        string numero = $"PED-{ano}-{(totalAno + 1):D4}";

        var pedido = new PedidoVenda
        {
            EmpresaId = empresaId,
            Numero = numero,
            ClienteId = dto.ClienteId,
            OrcamentoId = dto.OrcamentoId,
            VendedorId = dto.VendedorId,
            VendedorNome = dto.VendedorNome?.Trim(),
            DataPedido = dto.DataPedido.ToUniversalTime(),
            Status = StatusPedidoVenda.Pendente,
            DepositoId = dto.DepositoId,
            FormaPagamentoId = dto.FormaPagamentoId,
            NumeroParcelas = Math.Max(1, dto.NumeroParcelas),
            IntervaloDias = Math.Max(1, dto.IntervaloDias),
            DiasPrimeiroVencimento = Math.Max(0, dto.DiasPrimeiroVencimento),
            DescontoTipo = dto.DescontoTipo,
            DescontoGeral = dto.DescontoGeral,
            Observacao = dto.Observacao?.Trim()
        };

        var itensEntidade = new List<PedidoVendaItem>();
        foreach (var itemDto in dto.Itens)
        {
            var itemEntidade = await MontarItemPedidoAsync(empresaId, itemDto, cancellationToken);
            itensEntidade.Add(itemEntidade);
        }

        var totais = VendaCalculosHelper.CalcularTotais(
            itensEntidade.Select(i => i.SubTotal),
            dto.DescontoTipo,
            dto.DescontoGeral
        );

        pedido.ValorBruto = totais.ValorBruto;
        pedido.ValorDesconto = totais.ValorDescontoGeral;
        pedido.ValorTotal = totais.ValorTotal;
        pedido.Itens = itensEntidade;

        _context.PedidosVenda.Add(pedido);
        await _context.SaveChangesAsync(cancellationToken);

        return pedido.Id;
    }

    public async Task<bool> Handle(AtualizarPedidoVendaCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var pedido = await _context.PedidosVenda
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pedido == null)
            throw new KeyNotFoundException($"Pedido de venda com ID {request.Id} não encontrado.");

        if (pedido.Status != StatusPedidoVenda.Pendente)
            throw new InvalidOperationException($"Apenas pedidos pendentes podem ser editados. Status atual: '{pedido.Status}'.");

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == dto.ClienteId, cancellationToken);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} não encontrado.");

        if (dto.Itens == null || !dto.Itens.Any())
            throw new InvalidOperationException("O pedido de venda deve conter pelo menos um item.");

        pedido.ClienteId = dto.ClienteId;
        pedido.VendedorId = dto.VendedorId;
        pedido.VendedorNome = dto.VendedorNome?.Trim();
        pedido.DataPedido = dto.DataPedido.ToUniversalTime();
        pedido.DepositoId = dto.DepositoId;
        pedido.FormaPagamentoId = dto.FormaPagamentoId;
        pedido.NumeroParcelas = Math.Max(1, dto.NumeroParcelas);
        pedido.IntervaloDias = Math.Max(1, dto.IntervaloDias);
        pedido.DiasPrimeiroVencimento = Math.Max(0, dto.DiasPrimeiroVencimento);
        pedido.DescontoTipo = dto.DescontoTipo;
        pedido.DescontoGeral = dto.DescontoGeral;
        pedido.Observacao = dto.Observacao?.Trim();

        _context.PedidoVendaItens.RemoveRange(pedido.Itens);
        pedido.Itens.Clear();

        var novosItens = new List<PedidoVendaItem>();
        foreach (var itemDto in dto.Itens)
        {
            var item = await MontarItemPedidoAsync(empresaId, itemDto, cancellationToken);
            novosItens.Add(item);
        }

        var totais = VendaCalculosHelper.CalcularTotais(
            novosItens.Select(i => i.SubTotal),
            dto.DescontoTipo,
            dto.DescontoGeral
        );

        pedido.ValorBruto = totais.ValorBruto;
        pedido.ValorDesconto = totais.ValorDescontoGeral;
        pedido.ValorTotal = totais.ValorTotal;
        pedido.Itens = novosItens;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(AprovarPedidoVendaCommand request, CancellationToken cancellationToken)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pedido == null)
            throw new KeyNotFoundException($"Pedido de venda com ID {request.Id} não encontrado.");

        if (pedido.Status != StatusPedidoVenda.Pendente)
            throw new InvalidOperationException($"Apenas pedidos pendentes podem ser aprovados. Status atual: '{pedido.Status}'.");

        int empresaId = _currentEmpresaService.EmpresaId ?? pedido.EmpresaId;

        // Validação de saldo disponível sem reservar (conforme alinhamento)
        int? depositoId = pedido.DepositoId;
        if (!depositoId.HasValue)
        {
            var depositoPadrao = await _context.Depositos
                .FirstOrDefaultAsync(d => d.EmpresaId == empresaId && d.Padrao && d.Ativo, cancellationToken);
            depositoId = depositoPadrao?.Id;
        }

        var itensProduto = pedido.Itens.Where(i => i.TipoItem == TipoItemVenda.Produto && i.ProdutoId.HasValue).ToList();
        if (itensProduto.Any() && depositoId.HasValue)
        {
            foreach (var item in itensProduto)
            {
                var estoque = await _context.EstoqueProdutos
                    .FirstOrDefaultAsync(ep => ep.ProdutoId == item.ProdutoId!.Value && ep.DepositoId == depositoId.Value, cancellationToken);

                decimal saldo = estoque?.Quantidade ?? 0m;
                if (saldo < item.Quantidade)
                {
                    var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId!.Value, cancellationToken);
                    string nomeProd = produto?.Nome ?? item.Descricao;
                    throw new InvalidOperationException(
                        $"Saldo insuficiente no estoque para aprovação. Produto: '{nomeProd}'. Saldo atual: {saldo:N2}, Solicitado: {item.Quantidade:N2}."
                    );
                }
            }
        }

        pedido.Status = StatusPedidoVenda.Aprovado;
        pedido.DataAprovacao = DateTime.UtcNow;
        if (!pedido.DepositoId.HasValue && depositoId.HasValue)
        {
            pedido.DepositoId = depositoId.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(CancelarPedidoVendaCommand request, CancellationToken cancellationToken)
    {
        var pedido = await _context.PedidosVenda
            .Include(p => p.Itens)
            .Include(p => p.Faturamentos)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pedido == null)
            throw new KeyNotFoundException($"Pedido de venda com ID {request.Id} não encontrado.");

        if (pedido.Status == StatusPedidoVenda.Cancelado)
            return true;

        int empresaId = _currentEmpresaService.EmpresaId ?? pedido.EmpresaId;

        // Se o pedido já foi FATURADO, precisa estornar estoque e cancelar parcelas
        if (pedido.Status == StatusPedidoVenda.Faturado)
        {
            var faturamentoAtivo = pedido.Faturamentos
                .FirstOrDefault(f => f.Status == StatusFaturamento.Emitido);

            if (faturamentoAtivo != null)
            {
                // 1. Verifica se alguma conta a receber já foi recebida
                var contasReceber = await _context.ContasReceber
                    .Where(c => c.EmpresaId == empresaId && c.IdParcelamento == faturamentoAtivo.IdParcelamento)
                    .ToListAsync(cancellationToken);

                bool temParcelaPaga = contasReceber.Any(c =>
                    c.Status == StatusContaFinanceira.Paga || c.ValorRecebido > 0);

                if (temParcelaPaga)
                {
                    throw new InvalidOperationException(
                        "Não é possível cancelar o pedido faturado pois existem parcelas já recebidas ou baixadas no financeiro. Realize o estorno manual das baixas antes de prosseguir."
                    );
                }

                // Cancela as contas a receber
                foreach (var conta in contasReceber)
                {
                    conta.Status = StatusContaFinanceira.Cancelada;
                }

                // 2. Estorna o estoque dos itens do tipo Produto
                int? depositoId = pedido.DepositoId;
                if (!depositoId.HasValue)
                {
                    var depPadrao = await _context.Depositos
                        .FirstOrDefaultAsync(d => d.EmpresaId == empresaId && d.Padrao, cancellationToken);
                    depositoId = depPadrao?.Id;
                }

                if (depositoId.HasValue)
                {
                    foreach (var item in pedido.Itens.Where(i => i.TipoItem == TipoItemVenda.Produto && i.ProdutoId.HasValue))
                    {
                        var estoqueProduto = await _context.EstoqueProdutos
                            .FirstOrDefaultAsync(ep => ep.ProdutoId == item.ProdutoId!.Value && ep.DepositoId == depositoId.Value, cancellationToken);

                        if (estoqueProduto != null)
                        {
                            estoqueProduto.Quantidade += item.Quantidade;
                        }

                        // Registra a movimentação de estorno
                        var movEstorno = new MovimentacaoEstoque
                        {
                            EmpresaId = empresaId,
                            ProdutoId = item.ProdutoId!.Value,
                            DepositoOrigemId = depositoId.Value,
                            Tipo = TipoMovimentacaoEstoque.Entrada,
                            Origem = OrigemMovimentacaoEstoque.CancelamentoVenda,
                            Quantidade = item.Quantidade,
                            CustoUnitario = item.CustoUnitario,
                            CustoTotal = Math.Round(item.Quantidade * item.CustoUnitario, 2),
                            DataMovimentacao = DateTime.UtcNow,
                            DocumentoOrigem = pedido.Numero,
                            DocumentoOrigemId = pedido.Id,
                            Observacao = $"Estorno automático por cancelamento do Pedido {pedido.Numero}"
                        };
                        _context.MovimentacoesEstoque.Add(movEstorno);
                    }
                }

                faturamentoAtivo.Status = StatusFaturamento.Cancelado;
                faturamentoAtivo.DataCancelamento = DateTime.UtcNow;
            }
        }

        pedido.Status = StatusPedidoVenda.Cancelado;
        pedido.DataCancelamento = DateTime.UtcNow;
        pedido.MotivoCancelamento = request.Motivo?.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(FaturarPedidoVendaCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var pedido = await _context.PedidosVenda
            .Include(p => p.Itens)
            .Include(p => p.Faturamentos)
            .FirstOrDefaultAsync(p => p.Id == dto.PedidoVendaId, cancellationToken);

        if (pedido == null)
            throw new KeyNotFoundException($"Pedido de venda com ID {dto.PedidoVendaId} não encontrado.");

        if (pedido.Status != StatusPedidoVenda.Aprovado)
            throw new InvalidOperationException($"Apenas pedidos com status 'Aprovado' podem ser faturados. Status atual: '{pedido.Status}'.");

        // Aplica ajustes de pagamento se informados na tela de faturamento
        if (dto.DepositoId.HasValue) pedido.DepositoId = dto.DepositoId.Value;
        if (dto.FormaPagamentoId.HasValue) pedido.FormaPagamentoId = dto.FormaPagamentoId.Value;
        if (dto.NumeroParcelas.HasValue && dto.NumeroParcelas.Value > 0) pedido.NumeroParcelas = dto.NumeroParcelas.Value;
        if (dto.IntervaloDias.HasValue && dto.IntervaloDias.Value > 0) pedido.IntervaloDias = dto.IntervaloDias.Value;
        if (dto.DiasPrimeiroVencimento.HasValue && dto.DiasPrimeiroVencimento.Value >= 0) pedido.DiasPrimeiroVencimento = dto.DiasPrimeiroVencimento.Value;

        // Garante depósito para saída de estoque
        int? depositoId = pedido.DepositoId;
        if (!depositoId.HasValue)
        {
            var depositoPadrao = await _context.Depositos
                .FirstOrDefaultAsync(d => d.EmpresaId == empresaId && d.Padrao && d.Ativo, cancellationToken);

            if (depositoPadrao == null)
                depositoPadrao = await _context.Depositos.FirstOrDefaultAsync(d => d.EmpresaId == empresaId && d.Ativo, cancellationToken);

            if (depositoPadrao == null)
                throw new InvalidOperationException("Nenhum depósito cadastrado ou selecionado para a baixa de estoque na venda.");

            depositoId = depositoPadrao.Id;
            pedido.DepositoId = depositoId;
        }

        var itensProduto = pedido.Itens.Where(i => i.TipoItem == TipoItemVenda.Produto && i.ProdutoId.HasValue).ToList();

        // 1. Valida estoque disponível de todos os itens antes de movimentar
        foreach (var item in itensProduto)
        {
            var estoque = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(ep => ep.ProdutoId == item.ProdutoId!.Value && ep.DepositoId == depositoId.Value, cancellationToken);

            decimal saldo = estoque?.Quantidade ?? 0m;
            if (saldo < item.Quantidade)
            {
                var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId!.Value, cancellationToken);
                string nome = produto?.Nome ?? item.Descricao;
                throw new InvalidOperationException(
                    $"Saldo insuficiente para faturamento. Produto: '{nome}'. Saldo atual: {saldo:N2}, Solicitado: {item.Quantidade:N2}."
                );
            }
        }

        // 2. Dá baixa de estoque e calcula CMV
        decimal custoTotalVenda = 0m;
        foreach (var item in itensProduto)
        {
            var estoque = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(ep => ep.ProdutoId == item.ProdutoId!.Value && ep.DepositoId == depositoId.Value, cancellationToken);

            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == item.ProdutoId!.Value, cancellationToken);

            decimal custoUnitario = estoque?.CustoMedio > 0
                ? estoque.CustoMedio
                : (produto?.PrecoCusto ?? 0m);

            item.CustoUnitario = custoUnitario;
            custoTotalVenda += Math.Round(item.Quantidade * custoUnitario, 2);

            if (estoque != null)
            {
                estoque.Quantidade -= item.Quantidade;
            }

            var mov = new MovimentacaoEstoque
            {
                EmpresaId = empresaId,
                ProdutoId = item.ProdutoId!.Value,
                DepositoOrigemId = depositoId.Value,
                Tipo = TipoMovimentacaoEstoque.Saida,
                Origem = OrigemMovimentacaoEstoque.Venda,
                Quantidade = item.Quantidade,
                CustoUnitario = custoUnitario,
                CustoTotal = Math.Round(item.Quantidade * custoUnitario, 2),
                DataMovimentacao = DateTime.UtcNow,
                DocumentoOrigem = pedido.Numero,
                DocumentoOrigemId = pedido.Id,
                Observacao = $"Venda Faturada - Pedido {pedido.Numero}"
            };
            _context.MovimentacoesEstoque.Add(mov);
        }

        // 3. Cria o Faturamento
        int ano = DateTime.UtcNow.Year;
        int totalFatAno = await _context.Faturamentos
            .CountAsync(f => f.EmpresaId == empresaId && f.DataFaturamento.Year == ano, cancellationToken);
        string numeroFat = $"FAT-{ano}-{(totalFatAno + 1):D4}";

        var idParcelamento = Guid.NewGuid();

        var faturamento = new Faturamento
        {
            EmpresaId = empresaId,
            Numero = numeroFat,
            PedidoVendaId = pedido.Id,
            DataFaturamento = DateTime.UtcNow,
            Status = StatusFaturamento.Emitido,
            ValorTotal = pedido.ValorTotal,
            CustoTotal = custoTotalVenda,
            IdParcelamento = idParcelamento
        };
        _context.Faturamentos.Add(faturamento);

        // 4. Gera as Contas a Receber no Financeiro com rateio de centavos
        int parcelasQtd = Math.Max(1, pedido.NumeroParcelas);
        decimal valorParcelaBase = Math.Round(pedido.ValorTotal / parcelasQtd, 2);
        decimal diferencaCentavos = pedido.ValorTotal - (valorParcelaBase * parcelasQtd);

        var hoje = DateTime.UtcNow.Date;
        var dataPrimeiroVencimento = hoje.AddDays(pedido.DiasPrimeiroVencimento);

        var contasReceber = new List<ContaReceber>();
        for (int i = 1; i <= parcelasQtd; i++)
        {
            decimal valorParcela = valorParcelaBase;
            if (i == 1) valorParcela += diferencaCentavos; // Ajusta centavos na 1ª parcela

            DateTime vencimento = parcelasQtd == 1
                ? dataPrimeiroVencimento
                : dataPrimeiroVencimento.AddDays((i - 1) * pedido.IntervaloDias);

            string descricao = parcelasQtd > 1
                ? $"Venda {pedido.Numero} ({i}/{parcelasQtd})"
                : $"Venda {pedido.Numero}";

            contasReceber.Add(new ContaReceber
            {
                EmpresaId = empresaId,
                ClienteId = pedido.ClienteId,
                Descricao = descricao,
                NumeroDocumento = pedido.Numero,
                ValorOriginal = valorParcela,
                ValorRecebido = 0m,
                SaldoRestante = valorParcela,
                DataEmissao = DateTime.UtcNow,
                DataVencimento = vencimento.ToUniversalTime(),
                DataCompetencia = hoje.ToUniversalTime(),
                Status = StatusContaFinanceira.Pendente,
                FormaPagamentoId = pedido.FormaPagamentoId,
                NumeroParcela = i,
                TotalParcelas = parcelasQtd,
                IdParcelamento = idParcelamento,
                Observacoes = $"Faturamento {numeroFat} ref. Pedido de Venda {pedido.Numero}."
            });
        }

        await _context.ContasReceber.AddRangeAsync(contasReceber, cancellationToken);

        // 5. Atualiza status do Pedido para Faturado
        pedido.Status = StatusPedidoVenda.Faturado;

        await _context.SaveChangesAsync(cancellationToken);
        return faturamento.Id;
    }

    private async Task<PedidoVendaItem> MontarItemPedidoAsync(
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

        return new PedidoVendaItem
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
            SubTotal = calculo.SubTotal,
            CustoUnitario = 0m
        };
    }
}
