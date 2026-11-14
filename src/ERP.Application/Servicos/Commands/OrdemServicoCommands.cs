using ERP.Application.Common.Interfaces;
using ERP.Application.Servicos.DTOs;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Entities.Servicos;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Servicos.Commands;

public record CriarOrdemServicoCommand(CriarOrdemServicoDto Dto, string? UsuarioNome = null) : IRequest<int>;
public record AtualizarOrdemServicoCommand(int Id, AtualizarOrdemServicoDto Dto, string? UsuarioNome = null) : IRequest<bool>;
public record MudarStatusOrdemServicoCommand(MudarStatusOrdemServicoDto Dto, string? UsuarioNome = null) : IRequest<bool>;
public record CancelarOrdemServicoCommand(int Id, string? Motivo, string? UsuarioNome = null) : IRequest<bool>;
public record FaturarOrdemServicoCommand(FaturarOrdemServicoDto Dto, string? UsuarioNome = null) : IRequest<bool>;

public class OrdemServicoCommandsHandler :
    IRequestHandler<CriarOrdemServicoCommand, int>,
    IRequestHandler<AtualizarOrdemServicoCommand, bool>,
    IRequestHandler<MudarStatusOrdemServicoCommand, bool>,
    IRequestHandler<CancelarOrdemServicoCommand, bool>,
    IRequestHandler<FaturarOrdemServicoCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public OrdemServicoCommandsHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<int> Handle(CriarOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == dto.ClienteId, cancellationToken);
        if (cliente == null)
            throw new KeyNotFoundException($"Cliente com ID {dto.ClienteId} não encontrado.");

        int ano = dto.DataAbertura.Year;
        int totalAno = await _context.OrdensServico
            .CountAsync(o => o.EmpresaId == empresaId && o.DataAbertura.Year == ano, cancellationToken);
        string numero = $"OS-{ano}-{(totalAno + 1):D4}";

        var os = new OrdemServico
        {
            EmpresaId = empresaId,
            Numero = numero,
            ClienteId = dto.ClienteId,
            Prioridade = dto.Prioridade,
            Status = StatusOrdemServico.Aberta,
            DataAbertura = dto.DataAbertura.ToUniversalTime(),
            PrevisaoConclusao = dto.PrevisaoConclusao?.ToUniversalTime(),
            TecnicoResponsavel = dto.TecnicoResponsavel?.Trim(),
            Equipamento = dto.Equipamento.Trim(),
            Marca = dto.Marca?.Trim(),
            Modelo = dto.Modelo?.Trim(),
            NumeroSerie = dto.NumeroSerie?.Trim(),
            Acessorios = dto.Acessorios?.Trim(),
            CondicoesGerais = dto.CondicoesGerais?.Trim(),
            DefeitoRelatado = dto.DefeitoRelatado.Trim(),
            LaudoTecnico = dto.LaudoTecnico?.Trim(),
            SolucaoTecnica = dto.SolucaoTecnica?.Trim(),
            ValorDesconto = dto.ValorDesconto,
            FormaPagamentoId = dto.FormaPagamentoId,
            NumeroParcelas = Math.Max(1, dto.NumeroParcelas),
            Observacoes = dto.Observacoes?.Trim(),
            TermoGarantia = dto.TermoGarantia?.Trim() ?? "Garantia de 90 dias para as peças aplicadas e serviços executados."
        };

        // Adiciona peças
        if (dto.Pecas != null && dto.Pecas.Any())
        {
            foreach (var p in dto.Pecas)
            {
                var peca = new OrdemServicoPeca
                {
                    EmpresaId = empresaId,
                    ProdutoId = p.ProdutoId,
                    Descricao = p.Descricao.Trim(),
                    Quantidade = p.Quantidade,
                    ValorUnitario = p.ValorUnitario,
                    ValorDesconto = p.ValorDesconto,
                    DepositoId = p.DepositoId,
                    BaixadoEstoque = false
                };
                peca.CalcularTotal();
                os.ItensPecas.Add(peca);
            }
        }

        // Adiciona serviços
        if (dto.Servicos != null && dto.Servicos.Any())
        {
            foreach (var s in dto.Servicos)
            {
                var serv = new OrdemServicoServico
                {
                    EmpresaId = empresaId,
                    ServicoId = s.ServicoId,
                    Descricao = s.Descricao.Trim(),
                    Quantidade = s.Quantidade,
                    ValorUnitario = s.ValorUnitario,
                    ValorDesconto = s.ValorDesconto,
                    Tecnico = s.Tecnico?.Trim() ?? os.TecnicoResponsavel
                };
                serv.CalcularTotal();
                os.ItensServicos.Add(serv);
            }
        }

        os.RecalcularTotais();

        // Histórico inicial
        os.HistoricoStatus.Add(new OrdemServicoHistorico
        {
            EmpresaId = empresaId,
            StatusAnterior = null,
            StatusNovo = StatusOrdemServico.Aberta,
            Data = DateTime.UtcNow,
            UsuarioNome = request.UsuarioNome ?? "Sistema",
            Observacao = "Abertura da Ordem de Serviço."
        });

        _context.OrdensServico.Add(os);
        await _context.SaveChangesAsync(cancellationToken);

        return os.Id;
    }

    public async Task<bool> Handle(AtualizarOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var os = await _context.OrdensServico
            .Include(o => o.ItensPecas)
            .Include(o => o.ItensServicos)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.EmpresaId == empresaId, cancellationToken);

        if (os == null)
            throw new KeyNotFoundException($"Ordem de Serviço com ID {request.Id} não encontrada.");

        if (os.Status == StatusOrdemServico.Faturada || os.Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException($"Não é possível editar uma OS com status '{os.Status}'.");

        os.ClienteId = dto.ClienteId;
        os.Prioridade = dto.Prioridade;
        os.PrevisaoConclusao = dto.PrevisaoConclusao?.ToUniversalTime();
        os.TecnicoResponsavel = dto.TecnicoResponsavel?.Trim();
        os.Equipamento = dto.Equipamento.Trim();
        os.Marca = dto.Marca?.Trim();
        os.Modelo = dto.Modelo?.Trim();
        os.NumeroSerie = dto.NumeroSerie?.Trim();
        os.Acessorios = dto.Acessorios?.Trim();
        os.CondicoesGerais = dto.CondicoesGerais?.Trim();
        os.DefeitoRelatado = dto.DefeitoRelatado.Trim();
        os.LaudoTecnico = dto.LaudoTecnico?.Trim();
        os.SolucaoTecnica = dto.SolucaoTecnica?.Trim();
        os.ValorDesconto = dto.ValorDesconto;
        os.FormaPagamentoId = dto.FormaPagamentoId;
        os.NumeroParcelas = Math.Max(1, dto.NumeroParcelas);
        os.Observacoes = dto.Observacoes?.Trim();
        if (!string.IsNullOrWhiteSpace(dto.TermoGarantia))
            os.TermoGarantia = dto.TermoGarantia.Trim();

        // Atualiza peças
        _context.OrdemServicoPecas.RemoveRange(os.ItensPecas);
        os.ItensPecas.Clear();
        if (dto.Pecas != null)
        {
            foreach (var p in dto.Pecas)
            {
                var peca = new OrdemServicoPeca
                {
                    EmpresaId = empresaId,
                    OrdemServicoId = os.Id,
                    ProdutoId = p.ProdutoId,
                    Descricao = p.Descricao.Trim(),
                    Quantidade = p.Quantidade,
                    ValorUnitario = p.ValorUnitario,
                    ValorDesconto = p.ValorDesconto,
                    DepositoId = p.DepositoId,
                    BaixadoEstoque = false
                };
                peca.CalcularTotal();
                os.ItensPecas.Add(peca);
            }
        }

        // Atualiza serviços
        _context.OrdemServicoServicos.RemoveRange(os.ItensServicos);
        os.ItensServicos.Clear();
        if (dto.Servicos != null)
        {
            foreach (var s in dto.Servicos)
            {
                var serv = new OrdemServicoServico
                {
                    EmpresaId = empresaId,
                    OrdemServicoId = os.Id,
                    ServicoId = s.ServicoId,
                    Descricao = s.Descricao.Trim(),
                    Quantidade = s.Quantidade,
                    ValorUnitario = s.ValorUnitario,
                    ValorDesconto = s.ValorDesconto,
                    Tecnico = s.Tecnico?.Trim() ?? os.TecnicoResponsavel
                };
                serv.CalcularTotal();
                os.ItensServicos.Add(serv);
            }
        }

        os.RecalcularTotais();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(MudarStatusOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var os = await _context.OrdensServico
            .Include(o => o.HistoricoStatus)
            .FirstOrDefaultAsync(o => o.Id == dto.Id && o.EmpresaId == empresaId, cancellationToken);

        if (os == null)
            throw new KeyNotFoundException($"Ordem de Serviço com ID {dto.Id} não encontrada.");

        if (os.Status == dto.NovoStatus)
            return true;

        if (os.Status == StatusOrdemServico.Faturada)
            throw new InvalidOperationException("Não é possível alterar o status de uma OS que já foi faturada.");

        if (os.Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Não é possível alterar o status de uma OS que foi cancelada.");

        var statusAnterior = os.Status;
        os.Status = dto.NovoStatus;

        if (dto.NovoStatus == StatusOrdemServico.Concluida && os.DataConclusao == null)
        {
            os.DataConclusao = DateTime.UtcNow;
        }

        os.HistoricoStatus.Add(new OrdemServicoHistorico
        {
            EmpresaId = empresaId,
            OrdemServicoId = os.Id,
            StatusAnterior = statusAnterior,
            StatusNovo = dto.NovoStatus,
            Data = DateTime.UtcNow,
            UsuarioNome = request.UsuarioNome ?? "Operador",
            Observacao = dto.Observacao ?? $"Status alterado para {dto.NovoStatus}."
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(CancelarOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var os = await _context.OrdensServico
            .Include(o => o.ItensPecas)
            .Include(o => o.HistoricoStatus)
            .FirstOrDefaultAsync(o => o.Id == request.Id && o.EmpresaId == empresaId, cancellationToken);

        if (os == null)
            throw new KeyNotFoundException($"Ordem de Serviço com ID {request.Id} não encontrada.");

        if (os.Status == StatusOrdemServico.Faturada)
        {
            // Se já foi faturada, verifica se as parcelas geradas foram pagas
            if (os.IdParcelamento.HasValue)
            {
                var contasReceber = await _context.ContasReceber
                    .Where(c => c.EmpresaId == empresaId && c.IdParcelamento == os.IdParcelamento.Value)
                    .ToListAsync(cancellationToken);

                bool temParcelaPaga = contasReceber.Any(c => c.Status == StatusContaFinanceira.Paga || c.ValorRecebido > 0);
                if (temParcelaPaga)
                {
                    throw new InvalidOperationException("Não é possível cancelar uma OS faturada com parcelas já recebidas no financeiro.");
                }

                foreach (var conta in contasReceber)
                {
                    conta.Status = StatusContaFinanceira.Cancelada;
                }
            }

            // Estorna estoque das peças baixadas
            foreach (var peca in os.ItensPecas.Where(p => p.BaixadoEstoque && p.ProdutoId.HasValue && p.DepositoId.HasValue))
            {
                var estoque = await _context.EstoqueProdutos
                    .FirstOrDefaultAsync(ep => ep.ProdutoId == peca.ProdutoId!.Value && ep.DepositoId == peca.DepositoId!.Value, cancellationToken);

                if (estoque != null)
                {
                    estoque.Quantidade += peca.Quantidade;
                }

                _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
                {
                    EmpresaId = empresaId,
                    ProdutoId = peca.ProdutoId!.Value,
                    DepositoOrigemId = peca.DepositoId!.Value,
                    Tipo = TipoMovimentacaoEstoque.Entrada,
                    Origem = OrigemMovimentacaoEstoque.OrdemServico,
                    Quantidade = peca.Quantidade,
                    CustoUnitario = peca.CustoUnitario,
                    CustoTotal = Math.Round(peca.Quantidade * peca.CustoUnitario, 2),
                    DataMovimentacao = DateTime.UtcNow,
                    DocumentoOrigem = os.Numero,
                    DocumentoOrigemId = os.Id,
                    Observacao = $"Estorno de estoque por cancelamento da OS {os.Numero}"
                });

                peca.BaixadoEstoque = false;
            }
        }

        var statusAnterior = os.Status;
        os.Status = StatusOrdemServico.Cancelada;
        os.DataCancelamento = DateTime.UtcNow;
        os.MotivoCancelamento = request.Motivo?.Trim();

        os.HistoricoStatus.Add(new OrdemServicoHistorico
        {
            EmpresaId = empresaId,
            OrdemServicoId = os.Id,
            StatusAnterior = statusAnterior,
            StatusNovo = StatusOrdemServico.Cancelada,
            Data = DateTime.UtcNow,
            UsuarioNome = request.UsuarioNome ?? "Operador",
            Observacao = $"Cancelamento: {request.Motivo ?? "Sem motivo informado"}"
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(FaturarOrdemServicoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var os = await _context.OrdensServico
            .Include(o => o.ItensPecas)
            .Include(o => o.ItensServicos)
            .Include(o => o.HistoricoStatus)
            .FirstOrDefaultAsync(o => o.Id == dto.Id && o.EmpresaId == empresaId, cancellationToken);

        if (os == null)
            throw new KeyNotFoundException($"Ordem de Serviço com ID {dto.Id} não encontrada.");

        if (os.Status == StatusOrdemServico.Faturada)
            throw new InvalidOperationException("Esta Ordem de Serviço já se encontra faturada.");

        if (os.Status == StatusOrdemServico.Cancelada)
            throw new InvalidOperationException("Não é possível faturar uma OS cancelada.");

        // Atualiza condições de pagamento
        os.FormaPagamentoId = dto.FormaPagamentoId;
        os.NumeroParcelas = Math.Max(1, dto.NumeroParcelas);

        // 1. Localiza depósito padrão se alguma peça não tiver depósito informado
        var depositoPadrao = await _context.Depositos
            .FirstOrDefaultAsync(d => d.EmpresaId == empresaId && d.Padrao && d.Ativo, cancellationToken);
        if (depositoPadrao == null)
            depositoPadrao = await _context.Depositos.FirstOrDefaultAsync(d => d.EmpresaId == empresaId && d.Ativo, cancellationToken);

        // 2. Dá baixa de estoque nas peças utilizadas
        foreach (var peca in os.ItensPecas.Where(p => p.ProdutoId.HasValue && !p.BaixadoEstoque))
        {
            int depositoId = peca.DepositoId ?? depositoPadrao?.Id ?? 1;
            peca.DepositoId = depositoId;

            var estoque = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(ep => ep.ProdutoId == peca.ProdutoId!.Value && ep.DepositoId == depositoId, cancellationToken);

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == peca.ProdutoId!.Value, cancellationToken);

            decimal custoUnitario = estoque?.CustoMedio > 0 ? estoque.CustoMedio : (produto?.PrecoCusto ?? 0m);
            peca.CustoUnitario = custoUnitario;

            if (estoque != null)
            {
                estoque.Quantidade -= peca.Quantidade;
            }

            _context.MovimentacoesEstoque.Add(new MovimentacaoEstoque
            {
                EmpresaId = empresaId,
                ProdutoId = peca.ProdutoId!.Value,
                DepositoOrigemId = depositoId,
                Tipo = TipoMovimentacaoEstoque.Saida,
                Origem = OrigemMovimentacaoEstoque.OrdemServico,
                Quantidade = peca.Quantidade,
                CustoUnitario = custoUnitario,
                CustoTotal = Math.Round(peca.Quantidade * custoUnitario, 2),
                DataMovimentacao = DateTime.UtcNow,
                DocumentoOrigem = os.Numero,
                DocumentoOrigemId = os.Id,
                Observacao = $"Baixa de estoque por faturamento da OS {os.Numero}"
            });

            peca.BaixadoEstoque = true;
        }

        // 3. Gera Contas a Receber
        var idParcelamento = Guid.NewGuid();
        os.IdParcelamento = idParcelamento;

        int parcelasQtd = Math.Max(1, dto.NumeroParcelas);
        decimal valorTotal = os.ValorTotal;
        decimal valorBase = Math.Round(valorTotal / parcelasQtd, 2);
        decimal diferencaCentavos = valorTotal - (valorBase * parcelasQtd);

        var hoje = DateTime.UtcNow.Date;
        var dataPrimeiroVencimento = hoje.AddDays(dto.DiasPrimeiroVencimento);

        for (int i = 1; i <= parcelasQtd; i++)
        {
            decimal valorParcela = valorBase;
            if (i == 1)
            {
                valorParcela += diferencaCentavos;
            }

            var dataVencimento = i == 1
                ? dataPrimeiroVencimento
                : dataPrimeiroVencimento.AddDays((i - 1) * Math.Max(1, dto.IntervaloDias));

            var contaReceber = new ContaReceber
            {
                EmpresaId = empresaId,
                ClienteId = os.ClienteId,
                Descricao = $"OS {os.Numero} - {os.Equipamento} (Parcela {i}/{parcelasQtd})",
                NumeroDocumento = $"{os.Numero}-{i}",
                ValorOriginal = valorParcela,
                ValorRecebido = 0,
                SaldoRestante = valorParcela,
                DataEmissao = hoje,
                DataVencimento = dataVencimento,
                DataCompetencia = hoje,
                Status = StatusContaFinanceira.Pendente,
                FormaPagamentoId = dto.FormaPagamentoId,
                NumeroParcela = i,
                TotalParcelas = parcelasQtd,
                IdParcelamento = idParcelamento,
                Observacoes = $"Gerado automaticamente pelo faturamento da Ordem de Serviço {os.Numero}."
            };

            _context.ContasReceber.Add(contaReceber);
        }

        // 4. Conclui e fatura a OS
        var statusAnterior = os.Status;
        os.Status = StatusOrdemServico.Faturada;
        if (os.DataConclusao == null) os.DataConclusao = DateTime.UtcNow;
        os.DataEntrega = DateTime.UtcNow;

        os.HistoricoStatus.Add(new OrdemServicoHistorico
        {
            EmpresaId = empresaId,
            OrdemServicoId = os.Id,
            StatusAnterior = statusAnterior,
            StatusNovo = StatusOrdemServico.Faturada,
            Data = DateTime.UtcNow,
            UsuarioNome = request.UsuarioNome ?? "Operador",
            Observacao = $"Ordem de Serviço faturada com sucesso em {parcelasQtd}x parcela(s). Baixas de estoque e Contas a Receber geradas."
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

