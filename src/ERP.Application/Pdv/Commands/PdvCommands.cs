using ERP.Application.Common.Interfaces;
using ERP.Application.Pdv.DTOs;
using ERP.Domain.Core.Entities.Comissoes;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Entities.Pdv;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Pdv.Commands;

public record SalvarTerminalPdvCommand(int? Id, SalvarTerminalPdvDto Dto) : IRequest<int>;
public record AbrirSessaoCaixaCommand(int TerminalPdvId, decimal ValorAbertura, string? OperadorId, string OperadorNome) : IRequest<int>;
public record RegistrarMovimentoCaixaCommand(int SessaoCaixaPdvId, TipoMovimentoCaixa Tipo, decimal Valor, string Motivo, string? OperadorNome) : IRequest<bool>;
public record FecharSessaoCaixaCommand(int SessaoCaixaPdvId, decimal ValorFechamentoInformado, string? Observacoes) : IRequest<bool>;

public record CriarPreVendaPdvCommand(SalvarVendaPdvDto Dto) : IRequest<int>;
public record FinalizarVendaPdvCommand(SalvarVendaPdvDto Dto) : IRequest<int>;
public record CancelarVendaPdvCommand(int VendaPdvId) : IRequest<bool>;

public class PdvCommandsHandler :
    IRequestHandler<SalvarTerminalPdvCommand, int>,
    IRequestHandler<AbrirSessaoCaixaCommand, int>,
    IRequestHandler<RegistrarMovimentoCaixaCommand, bool>,
    IRequestHandler<FecharSessaoCaixaCommand, bool>,
    IRequestHandler<CriarPreVendaPdvCommand, int>,
    IRequestHandler<FinalizarVendaPdvCommand, int>,
    IRequestHandler<CancelarVendaPdvCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public PdvCommandsHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<int> Handle(SalvarTerminalPdvCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        TerminalPdv terminal;
        if (request.Id.HasValue)
        {
            terminal = await _context.TerminaisPdv
                .FirstOrDefaultAsync(t => t.Id == request.Id.Value && t.EmpresaId == empresaId, cancellationToken)
                ?? throw new KeyNotFoundException("Terminal PDV não encontrado.");
        }
        else
        {
            terminal = new TerminalPdv { EmpresaId = empresaId };
            _context.TerminaisPdv.Add(terminal);
        }

        terminal.Codigo = dto.Codigo.Trim().ToUpperInvariant();
        terminal.Nome = dto.Nome.Trim();
        terminal.Tipo = dto.Tipo;
        terminal.DepositoId = dto.DepositoId;
        terminal.ContaBancariaId = dto.ContaBancariaId;
        terminal.ImpressoraCupom = dto.ImpressoraCupom?.Trim();
        terminal.Ativo = dto.Ativo;

        await _context.SaveChangesAsync(cancellationToken);
        return terminal.Id;
    }

    public async Task<int> Handle(AbrirSessaoCaixaCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        bool jaAberta = await _context.SessoesCaixaPdv
            .AnyAsync(s => s.EmpresaId == empresaId && s.TerminalPdvId == request.TerminalPdvId && s.Status == StatusSessaoCaixa.Aberta, cancellationToken);
        if (jaAberta)
            throw new InvalidOperationException("Este terminal já possui uma sessão de caixa aberta.");

        var sessao = new SessaoCaixaPdv
        {
            EmpresaId = empresaId,
            TerminalPdvId = request.TerminalPdvId,
            OperadorId = request.OperadorId,
            OperadorNome = string.IsNullOrWhiteSpace(request.OperadorNome) ? "Operador" : request.OperadorNome.Trim(),
            DataAbertura = DateTime.UtcNow,
            ValorAbertura = request.ValorAbertura,
            Status = StatusSessaoCaixa.Aberta
        };

        _context.SessoesCaixaPdv.Add(sessao);
        await _context.SaveChangesAsync(cancellationToken);
        return sessao.Id;
    }

    public async Task<bool> Handle(RegistrarMovimentoCaixaCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var sessao = await _context.SessoesCaixaPdv
            .FirstOrDefaultAsync(s => s.Id == request.SessaoCaixaPdvId && s.EmpresaId == empresaId && s.Status == StatusSessaoCaixa.Aberta, cancellationToken);
        if (sessao == null) return false;

        _context.MovimentosCaixaPdv.Add(new MovimentoCaixaPdv
        {
            EmpresaId = empresaId,
            SessaoCaixaPdvId = sessao.Id,
            Tipo = request.Tipo,
            Valor = request.Valor,
            Motivo = request.Motivo.Trim(),
            DataMovimento = DateTime.UtcNow,
            OperadorNome = request.OperadorNome
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(FecharSessaoCaixaCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var sessao = await _context.SessoesCaixaPdv
            .Include(s => s.Movimentos)
            .Include(s => s.Vendas)
            .FirstOrDefaultAsync(s => s.Id == request.SessaoCaixaPdvId && s.EmpresaId == empresaId && s.Status == StatusSessaoCaixa.Aberta, cancellationToken);
        if (sessao == null) return false;

        decimal totalVendas = sessao.Vendas.Where(v => v.Status == StatusVendaPdv.Finalizada).Sum(v => v.ValorTotal);
        decimal suprimentos = sessao.Movimentos.Where(m => m.Tipo == TipoMovimentoCaixa.Suprimento).Sum(m => m.Valor);
        decimal sangrias = sessao.Movimentos.Where(m => m.Tipo == TipoMovimentoCaixa.Sangria).Sum(m => m.Valor);

        decimal calculado = sessao.ValorAbertura + totalVendas + suprimentos - sangrias;

        sessao.Status = StatusSessaoCaixa.Fechada;
        sessao.DataFechamento = DateTime.UtcNow;
        sessao.ValorFechamentoCalculado = calculado;
        sessao.ValorFechamentoInformado = request.ValorFechamentoInformado;
        sessao.DiferencaQuebra = request.ValorFechamentoInformado - calculado;
        sessao.ObservacoesFechamento = request.Observacoes?.Trim();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(CriarPreVendaPdvCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        if (dto.Itens.Count == 0)
            throw new InvalidOperationException("Adicione ao menos um item na pré-venda.");

        int ano = DateTime.UtcNow.Year;
        int totalAno = await _context.VendasPdv
            .CountAsync(v => v.EmpresaId == empresaId && v.DataVenda.Year == ano, cancellationToken);
        string numero = $"PV-{ano}-{(totalAno + 1):D4}";

        var (bruto, desconto, total) = CalcularTotais(dto);

        var venda = new VendaPdv
        {
            EmpresaId = empresaId,
            Numero = numero,
            TerminalPdvId = dto.TerminalPdvId,
            ClienteId = dto.ClienteId,
            ConsumidorNome = dto.ConsumidorNome?.Trim(),
            ConsumidorCpf = dto.ConsumidorCpf?.Trim(),
            VendedorId = dto.VendedorId,
            VendedorNome = dto.VendedorNome?.Trim(),
            Status = StatusVendaPdv.PreVenda,
            DataVenda = DateTime.UtcNow,
            ValorBruto = bruto,
            ValorDesconto = desconto,
            ValorTotal = total,
            Observacoes = dto.Observacoes?.Trim()
        };

        foreach (var item in dto.Itens)
        {
            decimal itemTotal = Math.Round((item.Quantidade * item.ValorUnitario) - item.ValorDesconto, 2);
            venda.Itens.Add(new VendaPdvItem
            {
                EmpresaId = empresaId,
                ProdutoId = item.ProdutoId,
                CodigoProduto = item.CodigoProduto,
                NomeProduto = item.NomeProduto,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario,
                ValorDesconto = item.ValorDesconto,
                ValorTotal = itemTotal
            });
        }

        _context.VendasPdv.Add(venda);
        await _context.SaveChangesAsync(cancellationToken);
        return venda.Id;
    }

    public async Task<int> Handle(FinalizarVendaPdvCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var dto = request.Dto;

        if (dto.Itens.Count == 0)
            throw new InvalidOperationException("Adicione ao menos um item para finalizar a venda.");

        if (!dto.SessaoCaixaPdvId.HasValue)
            throw new InvalidOperationException("Abra o caixa do terminal antes de finalizar vendas.");

        var terminal = await _context.TerminaisPdv
            .FirstOrDefaultAsync(t => t.Id == dto.TerminalPdvId && t.EmpresaId == empresaId, cancellationToken)
            ?? throw new KeyNotFoundException("Terminal PDV não encontrado.");

        var (bruto, desconto, total) = CalcularTotais(dto);
        decimal totalPago = dto.Pagamentos.Sum(p => p.Valor);
        if (totalPago < total)
            throw new InvalidOperationException($"Valor pago ({totalPago:C2}) é menor que o total da venda ({total:C2}).");

        decimal troco = Math.Round(totalPago - total, 2);

        VendaPdv venda;
        if (dto.PreVendaId.HasValue)
        {
            venda = await _context.VendasPdv
                .Include(v => v.Itens)
                .Include(v => v.Pagamentos)
                .FirstOrDefaultAsync(v => v.Id == dto.PreVendaId.Value && v.EmpresaId == empresaId && v.Status == StatusVendaPdv.PreVenda, cancellationToken)
                ?? throw new KeyNotFoundException("Pré-venda não encontrada ou já finalizada.");

            _context.VendaPdvItens.RemoveRange(venda.Itens);
            venda.Itens.Clear();
        }
        else
        {
            int ano = DateTime.UtcNow.Year;
            int totalAno = await _context.VendasPdv
                .CountAsync(v => v.EmpresaId == empresaId && v.DataVenda.Year == ano, cancellationToken);

            venda = new VendaPdv
            {
                EmpresaId = empresaId,
                Numero = $"PDV-{ano}-{(totalAno + 1):D4}"
            };
            _context.VendasPdv.Add(venda);
        }

        venda.TerminalPdvId = dto.TerminalPdvId;
        venda.SessaoCaixaPdvId = dto.SessaoCaixaPdvId;
        venda.ClienteId = dto.ClienteId;
        venda.ConsumidorNome = dto.ConsumidorNome?.Trim();
        venda.ConsumidorCpf = dto.ConsumidorCpf?.Trim();
        venda.VendedorId = dto.VendedorId;
        venda.VendedorNome = dto.VendedorNome?.Trim();
        venda.Status = StatusVendaPdv.Finalizada;
        venda.DataVenda = DateTime.UtcNow;
        venda.ValorBruto = bruto;
        venda.ValorDesconto = desconto;
        venda.ValorTotal = total;
        venda.Troco = troco;
        venda.Observacoes = dto.Observacoes?.Trim();

        foreach (var item in dto.Itens)
        {
            decimal itemTotal = Math.Round((item.Quantidade * item.ValorUnitario) - item.ValorDesconto, 2);
            venda.Itens.Add(new VendaPdvItem
            {
                EmpresaId = empresaId,
                ProdutoId = item.ProdutoId,
                CodigoProduto = item.CodigoProduto,
                NomeProduto = item.NomeProduto,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario,
                ValorDesconto = item.ValorDesconto,
                ValorTotal = itemTotal
            });
        }

        foreach (var pag in dto.Pagamentos.Where(p => p.Valor > 0))
        {
            venda.Pagamentos.Add(new VendaPdvPagamento
            {
                EmpresaId = empresaId,
                FormaPagamentoId = pag.FormaPagamentoId,
                DescricaoForma = pag.DescricaoForma,
                Valor = pag.Valor
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Baixa de estoque no depósito do terminal (ou primeiro depósito ativo da empresa)
        int? depositoId = terminal.DepositoId ?? await _context.Depositos
            .Where(d => d.EmpresaId == empresaId && d.Ativo)
            .Select(d => (int?)d.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (depositoId.HasValue)
        {
            foreach (var item in venda.Itens)
            {
                var estoque = await _context.EstoqueProdutos
                    .FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.ProdutoId == item.ProdutoId && e.DepositoId == depositoId.Value, cancellationToken);

                decimal custoUnit = estoque?.CustoMedio ?? 0m;
                if (estoque != null)
                {
                    estoque.Quantidade -= item.Quantidade;
                }

                _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    EmpresaId = empresaId,
                    ProdutoId = item.ProdutoId,
                    DepositoOrigemId = depositoId.Value,
                    Tipo = TipoMovimentacaoEstoque.Saida,
                    Origem = OrigemMovimentacaoEstoque.Pdv,
                    Quantidade = item.Quantidade,
                    CustoUnitario = custoUnit,
                    CustoTotal = Math.Round(item.Quantidade * custoUnit, 2),
                    DataMovimentacao = DateTime.UtcNow,
                    DocumentoOrigem = venda.Numero,
                    DocumentoOrigemId = venda.Id,
                    Observacao = $"Venda PDV {venda.Numero} ({terminal.Nome})"
                });
            }
        }

        // Lançamento financeiro na conta/caixa vinculada ao terminal
        if (terminal.ContaBancariaId.HasValue)
        {
            _context.MovimentacoesFinanceiras.Add(new MovimentacaoFinanceira
            {
                EmpresaId = empresaId,
                ContaBancariaId = terminal.ContaBancariaId.Value,
                Tipo = TipoOperacaoFinanceira.Entrada,
                DataMovimentacao = DateTime.UtcNow,
                Valor = venda.ValorTotal,
                Descricao = $"Venda PDV {venda.Numero} - {terminal.Nome}",
                DocumentoReferencia = venda.Numero
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return venda.Id;
    }

    public async Task<bool> Handle(CancelarVendaPdvCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;
        var venda = await _context.VendasPdv
            .Include(v => v.Itens)
            .Include(v => v.TerminalPdv)
            .FirstOrDefaultAsync(v => v.Id == request.VendaPdvId && v.EmpresaId == empresaId, cancellationToken);

        if (venda == null || venda.Status == StatusVendaPdv.Cancelada) return false;

        if (venda.Status == StatusVendaPdv.Finalizada)
        {
            int? depositoId = venda.TerminalPdv.DepositoId ?? await _context.Depositos
                .Where(d => d.EmpresaId == empresaId && d.Ativo)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (depositoId.HasValue)
            {
                foreach (var item in venda.Itens)
                {
                    var estoque = await _context.EstoqueProdutos
                        .FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.ProdutoId == item.ProdutoId && e.DepositoId == depositoId.Value, cancellationToken);
                    if (estoque != null)
                        estoque.Quantidade += item.Quantidade;

                    _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                    {
                        EmpresaId = empresaId,
                        ProdutoId = item.ProdutoId,
                        DepositoDestinoId = depositoId.Value,
                        Tipo = TipoMovimentacaoEstoque.Entrada,
                        Origem = OrigemMovimentacaoEstoque.CancelamentoVenda,
                        Quantidade = item.Quantidade,
                        CustoUnitario = estoque?.CustoMedio ?? 0m,
                        CustoTotal = Math.Round(item.Quantidade * (estoque?.CustoMedio ?? 0m), 2),
                        DataMovimentacao = DateTime.UtcNow,
                        DocumentoOrigem = venda.Numero,
                        DocumentoOrigemId = venda.Id,
                        Observacao = $"Estorno cancelamento PDV {venda.Numero}"
                    });
                }
            }
        }

        venda.Status = StatusVendaPdv.Cancelada;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static (decimal Bruto, decimal Desconto, decimal Total) CalcularTotais(SalvarVendaPdvDto dto)
    {
        decimal bruto = dto.Itens.Sum(i => Math.Round((i.Quantidade * i.ValorUnitario) - i.ValorDesconto, 2));
        decimal desconto = Math.Max(0, dto.ValorDesconto);
        decimal total = Math.Max(0, Math.Round(bruto - desconto, 2));
        return (bruto, desconto, total);
    }
}
