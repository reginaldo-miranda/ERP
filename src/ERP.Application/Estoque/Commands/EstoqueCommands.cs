using ERP.Application.Common.Interfaces;
using ERP.Application.Estoque.DTOs;
using ERP.Domain.Core.Entities.Estoque;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Estoque.Commands;

public record CriarDepositoCommand(CriarDepositoDto Dto) : IRequest<int>;
public record AtualizarDepositoCommand(int Id, CriarDepositoDto Dto) : IRequest<bool>;
public record RegistrarMovimentacaoEstoqueCommand(RegistrarMovimentacaoDto Dto) : IRequest<int>;
public record TransferirEstoqueCommand(TransferirEstoqueDto Dto) : IRequest<bool>;

public record IniciarInventarioCommand(IniciarInventarioDto Dto) : IRequest<int>;
public record AtualizarContagemInventarioCommand(int InventarioId, List<AtualizarContagemItemDto> Itens) : IRequest<bool>;
public record FinalizarInventarioCommand(FinalizarInventarioDto Dto) : IRequest<bool>;
public record CancelarInventarioCommand(int InventarioId) : IRequest<bool>;

public record CriarLoteCommand(CriarLoteDto Dto) : IRequest<int>;
public record CadastrarNumeroSerieCommand(CadastrarNumeroSerieDto Dto) : IRequest<int>;

public class EstoqueCommandsHandler :
    IRequestHandler<CriarDepositoCommand, int>,
    IRequestHandler<AtualizarDepositoCommand, bool>,
    IRequestHandler<RegistrarMovimentacaoEstoqueCommand, int>,
    IRequestHandler<TransferirEstoqueCommand, bool>,
    IRequestHandler<IniciarInventarioCommand, int>,
    IRequestHandler<AtualizarContagemInventarioCommand, bool>,
    IRequestHandler<FinalizarInventarioCommand, bool>,
    IRequestHandler<CancelarInventarioCommand, bool>,
    IRequestHandler<CriarLoteCommand, int>,
    IRequestHandler<CadastrarNumeroSerieCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _currentEmpresaService;

    public EstoqueCommandsHandler(IApplicationDbContext context, ICurrentEmpresaService currentEmpresaService)
    {
        _context = context;
        _currentEmpresaService = currentEmpresaService;
    }

    public async Task<int> Handle(CriarDepositoCommand request, CancellationToken cancellationToken)
    {
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        if (request.Dto.Padrao)
        {
            var depositosAtuais = await _context.Depositos
                .Where(d => d.EmpresaId == empresaId && d.Padrao)
                .ToListAsync(cancellationToken);

            foreach (var d in depositosAtuais)
            {
                d.Padrao = false;
            }
        }

        var deposito = new Deposito
        {
            EmpresaId = empresaId,
            Nome = request.Dto.Nome.Trim(),
            Codigo = request.Dto.Codigo?.Trim(),
            Endereco = request.Dto.Endereco?.Trim(),
            Responsavel = request.Dto.Responsavel?.Trim(),
            Ativo = true,
            Padrao = request.Dto.Padrao
        };

        _context.Depositos.Add(deposito);
        await _context.SaveChangesAsync(cancellationToken);
        return deposito.Id;
    }

    public async Task<bool> Handle(AtualizarDepositoCommand request, CancellationToken cancellationToken)
    {
        var deposito = await _context.Depositos.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (deposito == null) return false;

        int empresaId = _currentEmpresaService.EmpresaId ?? deposito.EmpresaId;

        if (request.Dto.Padrao && !deposito.Padrao)
        {
            var depositosAtuais = await _context.Depositos
                .Where(d => d.EmpresaId == empresaId && d.Padrao)
                .ToListAsync(cancellationToken);

            foreach (var d in depositosAtuais)
            {
                d.Padrao = false;
            }
        }

        deposito.Nome = request.Dto.Nome.Trim();
        deposito.Codigo = request.Dto.Codigo?.Trim();
        deposito.Endereco = request.Dto.Endereco?.Trim();
        deposito.Responsavel = request.Dto.Responsavel?.Trim();
        deposito.Padrao = request.Dto.Padrao;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(RegistrarMovimentacaoEstoqueCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (dto.Quantidade <= 0)
            throw new InvalidOperationException("A quantidade movimentada deve ser maior que zero.");

        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == dto.ProdutoId, cancellationToken);
        if (produto == null)
            throw new KeyNotFoundException($"Produto com ID {dto.ProdutoId} não encontrado.");

        var deposito = await _context.Depositos.FirstOrDefaultAsync(d => d.Id == dto.DepositoId, cancellationToken);
        if (deposito == null)
            throw new KeyNotFoundException($"Depósito com ID {dto.DepositoId} não encontrado.");

        var estoqueProduto = await _context.EstoqueProdutos
            .FirstOrDefaultAsync(ep => ep.ProdutoId == dto.ProdutoId && ep.DepositoId == dto.DepositoId, cancellationToken);

        if (estoqueProduto == null)
        {
            estoqueProduto = new EstoqueProduto
            {
                EmpresaId = empresaId,
                ProdutoId = dto.ProdutoId,
                DepositoId = dto.DepositoId,
                Quantidade = 0,
                CustoMedio = dto.CustoUnitario > 0 ? dto.CustoUnitario : produto.PrecoCusto,
                CustoUltimaCompra = dto.CustoUnitario > 0 ? dto.CustoUnitario : produto.PrecoCusto,
                EstoqueMinimo = produto.EstoqueMinimo,
                EstoqueMaximo = 100
            };
            _context.EstoqueProdutos.Add(estoqueProduto);
        }

        decimal custoUnitarioEfetivo = dto.CustoUnitario;
        if (custoUnitarioEfetivo <= 0)
        {
            custoUnitarioEfetivo = estoqueProduto.CustoMedio > 0 ? estoqueProduto.CustoMedio : produto.PrecoCusto;
        }

        int? loteIdVinculado = dto.LoteId;

        // Trata cadastro automático de novo lote se fornecido na entrada
        if (!loteIdVinculado.HasValue && !string.IsNullOrWhiteSpace(dto.NovoCodigoLote) && dto.NovaDataValidade.HasValue)
        {
            var novoLote = new Lote
            {
                EmpresaId = empresaId,
                ProdutoId = dto.ProdutoId,
                DepositoId = dto.DepositoId,
                CodigoLote = dto.NovoCodigoLote.Trim().ToUpperInvariant(),
                DataFabricacao = dto.NovaDataFabricacao,
                DataValidade = dto.NovaDataValidade.Value,
                QuantidadeInicial = dto.Quantidade,
                QuantidadeAtual = dto.Quantidade,
                CustoUnitario = custoUnitarioEfetivo,
                Ativo = true,
                Observacoes = dto.Observacao
            };
            _context.Lotes.Add(novoLote);
            await _context.SaveChangesAsync(cancellationToken);
            loteIdVinculado = novoLote.Id;
        }
        else if (loteIdVinculado.HasValue)
        {
            var lote = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == loteIdVinculado.Value, cancellationToken);
            if (lote != null)
            {
                if (dto.Tipo == TipoMovimentacaoEstoque.Entrada || dto.Tipo == TipoMovimentacaoEstoque.AjustePositivo)
                {
                    lote.QuantidadeAtual += dto.Quantidade;
                }
                else if (dto.Tipo == TipoMovimentacaoEstoque.Saida || dto.Tipo == TipoMovimentacaoEstoque.AjusteNegativo)
                {
                    if (lote.QuantidadeAtual < dto.Quantidade)
                    {
                        throw new InvalidOperationException($"Saldo insuficiente no lote '{lote.CodigoLote}'. Saldo no lote: {lote.QuantidadeAtual:N2}, solicitado: {dto.Quantidade:N2}.");
                    }
                    lote.QuantidadeAtual -= dto.Quantidade;
                }
            }
        }

        // Trata cadastro ou baixa de número de série
        int? numeroSerieIdVinculado = dto.NumeroSerieId;
        if (!numeroSerieIdVinculado.HasValue && !string.IsNullOrWhiteSpace(dto.NovoNumeroSerie))
        {
            var novoSerial = new NumeroSerie
            {
                EmpresaId = empresaId,
                ProdutoId = dto.ProdutoId,
                DepositoId = dto.DepositoId,
                LoteId = loteIdVinculado,
                Numero = dto.NovoNumeroSerie.Trim().ToUpperInvariant(),
                Status = StatusNumeroSerie.Disponivel,
                DataEntrada = DateTime.UtcNow,
                Observacoes = dto.Observacao
            };
            _context.NumerosSerie.Add(novoSerial);
            await _context.SaveChangesAsync(cancellationToken);
            numeroSerieIdVinculado = novoSerial.Id;
        }
        else if (numeroSerieIdVinculado.HasValue)
        {
            var serial = await _context.NumerosSerie.FirstOrDefaultAsync(n => n.Id == numeroSerieIdVinculado.Value, cancellationToken);
            if (serial != null)
            {
                if (dto.Tipo == TipoMovimentacaoEstoque.Saida || dto.Tipo == TipoMovimentacaoEstoque.AjusteNegativo)
                {
                    serial.Status = StatusNumeroSerie.Baixado;
                    serial.DataSaida = DateTime.UtcNow;
                }
                else
                {
                    serial.Status = StatusNumeroSerie.Disponivel;
                }
            }
        }

        switch (dto.Tipo)
        {
            case TipoMovimentacaoEstoque.Entrada:
            case TipoMovimentacaoEstoque.AjustePositivo:
                decimal qtdAnterior = estoqueProduto.Quantidade;
                decimal custoMedioAnterior = estoqueProduto.CustoMedio;
                decimal novaQtd = qtdAnterior + dto.Quantidade;

                if (novaQtd > 0 && dto.Tipo == TipoMovimentacaoEstoque.Entrada)
                {
                    decimal valorEstoqueTotal = (qtdAnterior * custoMedioAnterior) + (dto.Quantidade * custoUnitarioEfetivo);
                    estoqueProduto.CustoMedio = Math.Round(valorEstoqueTotal / novaQtd, 4);
                    estoqueProduto.CustoUltimaCompra = custoUnitarioEfetivo;
                }
                estoqueProduto.Quantidade = novaQtd;
                break;

            case TipoMovimentacaoEstoque.Saida:
            case TipoMovimentacaoEstoque.AjusteNegativo:
                if (estoqueProduto.Quantidade < dto.Quantidade)
                {
                    throw new InvalidOperationException(
                        $"Saldo insuficiente no depósito '{deposito.Nome}'. Saldo atual: {estoqueProduto.Quantidade:N2}, Quantidade solicitada: {dto.Quantidade:N2}."
                    );
                }
                estoqueProduto.Quantidade -= dto.Quantidade;
                break;

            default:
                throw new NotSupportedException($"Tipo de movimentação {dto.Tipo} ainda não suportado nesta operação.");
        }

        var movimentacao = new MovimentacaoEstoque
        {
            EmpresaId = empresaId,
            ProdutoId = dto.ProdutoId,
            DepositoOrigemId = dto.DepositoId,
            Tipo = dto.Tipo,
            Origem = OrigemMovimentacaoEstoque.Manual,
            Quantidade = dto.Quantidade,
            CustoUnitario = custoUnitarioEfetivo,
            CustoTotal = Math.Round(dto.Quantidade * custoUnitarioEfetivo, 2),
            DocumentoOrigem = dto.DocumentoOrigem,
            Observacao = dto.Observacao,
            LoteId = loteIdVinculado,
            NumeroSerieId = numeroSerieIdVinculado,
            DataMovimentacao = DateTime.UtcNow
        };

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync(cancellationToken);

        return movimentacao.Id;
    }

    public async Task<bool> Handle(TransferirEstoqueCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (dto.DepositoOrigemId == dto.DepositoDestinoId)
            throw new InvalidOperationException("O depósito de destino deve ser diferente do depósito de origem.");

        if (dto.Quantidade <= 0)
            throw new InvalidOperationException("A quantidade a transferir deve ser maior que zero.");

        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == dto.ProdutoId, cancellationToken);
        if (produto == null)
            throw new KeyNotFoundException($"Produto com ID {dto.ProdutoId} não encontrado.");

        var depOrigem = await _context.Depositos.FirstOrDefaultAsync(d => d.Id == dto.DepositoOrigemId, cancellationToken);
        if (depOrigem == null)
            throw new KeyNotFoundException($"Depósito de origem ID {dto.DepositoOrigemId} não encontrado.");

        var depDestino = await _context.Depositos.FirstOrDefaultAsync(d => d.Id == dto.DepositoDestinoId, cancellationToken);
        if (depDestino == null)
            throw new KeyNotFoundException($"Depósito de destino ID {dto.DepositoDestinoId} não encontrado.");

        var estoqueOrigem = await _context.EstoqueProdutos
            .FirstOrDefaultAsync(ep => ep.ProdutoId == dto.ProdutoId && ep.DepositoId == dto.DepositoOrigemId, cancellationToken);

        if (estoqueOrigem == null || estoqueOrigem.Quantidade < dto.Quantidade)
        {
            decimal saldoOrigem = estoqueOrigem?.Quantidade ?? 0m;
            throw new InvalidOperationException(
                $"Saldo insuficiente no depósito de origem '{depOrigem.Nome}'. Saldo atual: {saldoOrigem:N2}, solicitado: {dto.Quantidade:N2}."
            );
        }

        decimal custoUnitario = estoqueOrigem.CustoMedio > 0 ? estoqueOrigem.CustoMedio : produto.PrecoCusto;

        // Trata Lote na transferência
        int? loteDestinoId = null;
        if (dto.LoteId.HasValue)
        {
            var loteOrigem = await _context.Lotes.FirstOrDefaultAsync(l => l.Id == dto.LoteId.Value, cancellationToken);
            if (loteOrigem == null)
                throw new KeyNotFoundException($"Lote de origem não encontrado.");

            if (loteOrigem.QuantidadeAtual < dto.Quantidade)
                throw new InvalidOperationException($"Saldo insuficiente no lote '{loteOrigem.CodigoLote}'. Saldo no lote: {loteOrigem.QuantidadeAtual:N2}, solicitado: {dto.Quantidade:N2}.");

            loteOrigem.QuantidadeAtual -= dto.Quantidade;

            // Busca ou replica o lote no destino
            var loteDestino = await _context.Lotes
                .FirstOrDefaultAsync(l => l.EmpresaId == empresaId
                    && l.ProdutoId == dto.ProdutoId
                    && l.DepositoId == dto.DepositoDestinoId
                    && l.CodigoLote == loteOrigem.CodigoLote, cancellationToken);

            if (loteDestino == null)
            {
                loteDestino = new Lote
                {
                    EmpresaId = empresaId,
                    ProdutoId = dto.ProdutoId,
                    DepositoId = dto.DepositoDestinoId,
                    CodigoLote = loteOrigem.CodigoLote,
                    DataFabricacao = loteOrigem.DataFabricacao,
                    DataValidade = loteOrigem.DataValidade,
                    QuantidadeInicial = dto.Quantidade,
                    QuantidadeAtual = dto.Quantidade,
                    CustoUnitario = loteOrigem.CustoUnitario > 0 ? loteOrigem.CustoUnitario : custoUnitario,
                    Ativo = true,
                    Observacoes = $"Transferido de {depOrigem.Nome}"
                };
                _context.Lotes.Add(loteDestino);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                loteDestino.QuantidadeAtual += dto.Quantidade;
            }

            loteDestinoId = loteDestino.Id;
        }

        // Trata Número de Série na transferência
        if (dto.NumeroSerieId.HasValue)
        {
            var serial = await _context.NumerosSerie.FirstOrDefaultAsync(n => n.Id == dto.NumeroSerieId.Value, cancellationToken);
            if (serial != null)
            {
                serial.DepositoId = dto.DepositoDestinoId;
                if (loteDestinoId.HasValue)
                    serial.LoteId = loteDestinoId;
            }
        }

        // 1. Deduz da Origem
        estoqueOrigem.Quantidade -= dto.Quantidade;

        // 2. Credita no Destino (recalculando custo médio ponderado)
        var estoqueDestino = await _context.EstoqueProdutos
            .FirstOrDefaultAsync(ep => ep.ProdutoId == dto.ProdutoId && ep.DepositoId == dto.DepositoDestinoId, cancellationToken);

        if (estoqueDestino == null)
        {
            estoqueDestino = new EstoqueProduto
            {
                EmpresaId = empresaId,
                ProdutoId = dto.ProdutoId,
                DepositoId = dto.DepositoDestinoId,
                Quantidade = dto.Quantidade,
                CustoMedio = custoUnitario,
                CustoUltimaCompra = custoUnitario,
                EstoqueMinimo = produto.EstoqueMinimo,
                EstoqueMaximo = 100
            };
            _context.EstoqueProdutos.Add(estoqueDestino);
        }
        else
        {
            decimal qtdDestinoAnterior = estoqueDestino.Quantidade;
            decimal custoDestinoAnterior = estoqueDestino.CustoMedio;
            decimal novaQtdDestino = qtdDestinoAnterior + dto.Quantidade;

            if (novaQtdDestino > 0)
            {
                decimal valorTotalDestino = (qtdDestinoAnterior * custoDestinoAnterior) + (dto.Quantidade * custoUnitario);
                estoqueDestino.CustoMedio = Math.Round(valorTotalDestino / novaQtdDestino, 4);
                estoqueDestino.CustoUltimaCompra = custoUnitario;
            }
            estoqueDestino.Quantidade = novaQtdDestino;
        }

        // 3. Registra movimentação de transferência
        var movimentacao = new MovimentacaoEstoque
        {
            EmpresaId = empresaId,
            ProdutoId = dto.ProdutoId,
            DepositoOrigemId = dto.DepositoOrigemId,
            DepositoDestinoId = dto.DepositoDestinoId,
            Tipo = TipoMovimentacaoEstoque.Transferencia,
            Origem = OrigemMovimentacaoEstoque.Transferencia,
            Quantidade = dto.Quantidade,
            CustoUnitario = custoUnitario,
            CustoTotal = Math.Round(dto.Quantidade * custoUnitario, 2),
            DocumentoOrigem = dto.DocumentoOrigem ?? "TRANSFERENCIA",
            Observacao = string.IsNullOrWhiteSpace(dto.Observacao)
                ? $"Transferência de '{depOrigem.Nome}' para '{depDestino.Nome}'"
                : dto.Observacao,
            LoteId = dto.LoteId,
            NumeroSerieId = dto.NumeroSerieId,
            DataMovimentacao = DateTime.UtcNow
        };

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> Handle(IniciarInventarioCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var deposito = await _context.Depositos.FirstOrDefaultAsync(d => d.Id == dto.DepositoId, cancellationToken);
        if (deposito == null)
            throw new KeyNotFoundException($"Depósito {dto.DepositoId} não encontrado.");

        int countAno = await _context.Inventarios
            .CountAsync(i => i.EmpresaId == empresaId && i.DataAbertura.Year == DateTime.UtcNow.Year, cancellationToken);

        string protocolo = $"INV-{DateTime.UtcNow.Year}-{(countAno + 1):D4}";

        var inventario = new Inventario
        {
            EmpresaId = empresaId,
            DepositoId = dto.DepositoId,
            CategoriaId = dto.CategoriaId,
            Protocolo = protocolo,
            DataAbertura = DateTime.UtcNow,
            Status = StatusInventario.Aberto,
            Responsavel = dto.Responsavel.Trim(),
            Observacao = dto.Observacao?.Trim()
        };

        _context.Inventarios.Add(inventario);
        await _context.SaveChangesAsync(cancellationToken);

        // Busca itens do depósito para carregar no inventário
        var querySaldos = _context.EstoqueProdutos
            .Include(ep => ep.Produto)
            .Where(ep => ep.EmpresaId == empresaId && ep.DepositoId == dto.DepositoId);

        if (dto.CategoriaId.HasValue && dto.CategoriaId.Value > 0)
        {
            querySaldos = querySaldos.Where(ep => ep.Produto.CategoriaId == dto.CategoriaId.Value);
        }

        var saldos = await querySaldos.ToListAsync(cancellationToken);

        foreach (var saldo in saldos)
        {
            decimal custo = saldo.CustoMedio > 0 ? saldo.CustoMedio : saldo.Produto.PrecoCusto;
            var item = new InventarioItem
            {
                InventarioId = inventario.Id,
                ProdutoId = saldo.ProdutoId,
                QuantidadeSistema = saldo.Quantidade,
                QuantidadeContada = saldo.Quantidade, // Inicializa com a do sistema para conferência assistida
                CustoUnitario = custo,
                AjusteAplicado = false
            };
            _context.InventarioItens.Add(item);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return inventario.Id;
    }

    public async Task<bool> Handle(AtualizarContagemInventarioCommand request, CancellationToken cancellationToken)
    {
        var inventario = await _context.Inventarios
            .Include(i => i.Itens)
            .FirstOrDefaultAsync(i => i.Id == request.InventarioId, cancellationToken);

        if (inventario == null)
            throw new KeyNotFoundException($"Inventário {request.InventarioId} não encontrado.");

        if (inventario.Status != StatusInventario.Aberto)
            throw new InvalidOperationException("Não é possível alterar contagem de um inventário fechado ou cancelado.");

        var dictItens = inventario.Itens.ToDictionary(it => it.Id);

        foreach (var atualizacao in request.Itens)
        {
            if (dictItens.TryGetValue(atualizacao.ItemId, out var item))
            {
                item.QuantidadeContada = Math.Max(0, atualizacao.QuantidadeContada);
                item.Observacao = atualizacao.Observacao?.Trim();
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(FinalizarInventarioCommand request, CancellationToken cancellationToken)
    {
        var inventario = await _context.Inventarios
            .Include(i => i.Itens)
                .ThenInclude(it => it.Produto)
            .Include(i => i.Deposito)
            .FirstOrDefaultAsync(i => i.Id == request.Dto.InventarioId, cancellationToken);

        if (inventario == null)
            throw new KeyNotFoundException($"Inventário {request.Dto.InventarioId} não encontrado.");

        if (inventario.Status != StatusInventario.Aberto)
            throw new InvalidOperationException("Este inventário não está aberto para finalização.");

        int empresaId = inventario.EmpresaId;

        foreach (var item in inventario.Itens)
        {
            decimal diferenca = item.Diferenca;
            if (diferenca == 0 || item.AjusteAplicado)
                continue;

            var estoque = await _context.EstoqueProdutos
                .FirstOrDefaultAsync(ep => ep.ProdutoId == item.ProdutoId && ep.DepositoId == inventario.DepositoId, cancellationToken);

            if (estoque == null)
            {
                estoque = new EstoqueProduto
                {
                    EmpresaId = empresaId,
                    ProdutoId = item.ProdutoId,
                    DepositoId = inventario.DepositoId,
                    Quantidade = 0,
                    CustoMedio = item.CustoUnitario > 0 ? item.CustoUnitario : item.Produto.PrecoCusto,
                    CustoUltimaCompra = item.CustoUnitario > 0 ? item.CustoUnitario : item.Produto.PrecoCusto,
                    EstoqueMinimo = item.Produto.EstoqueMinimo,
                    EstoqueMaximo = 100
                };
                _context.EstoqueProdutos.Add(estoque);
            }

            TipoMovimentacaoEstoque tipo;
            decimal qtdAjuste;

            if (diferenca > 0)
            {
                tipo = TipoMovimentacaoEstoque.AjustePositivo;
                qtdAjuste = diferenca;
                estoque.Quantidade += qtdAjuste;
            }
            else
            {
                tipo = TipoMovimentacaoEstoque.AjusteNegativo;
                qtdAjuste = Math.Abs(diferenca);
                estoque.Quantidade = Math.Max(0, estoque.Quantidade - qtdAjuste);
            }

            var mov = new MovimentacaoEstoque
            {
                EmpresaId = empresaId,
                ProdutoId = item.ProdutoId,
                DepositoOrigemId = inventario.DepositoId,
                Tipo = tipo,
                Origem = OrigemMovimentacaoEstoque.Inventario,
                Quantidade = qtdAjuste,
                CustoUnitario = item.CustoUnitario,
                CustoTotal = Math.Round(qtdAjuste * item.CustoUnitario, 2),
                DocumentoOrigem = inventario.Protocolo,
                DocumentoOrigemId = inventario.Id,
                Observacao = string.IsNullOrWhiteSpace(request.Dto.Justificativa)
                    ? $"Ajuste automático via inventário {inventario.Protocolo}"
                    : request.Dto.Justificativa,
                DataMovimentacao = DateTime.UtcNow
            };

            _context.MovimentacoesEstoque.Add(mov);
            item.AjusteAplicado = true;
        }

        inventario.Status = StatusInventario.Finalizado;
        inventario.DataFechamento = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(CancelarInventarioCommand request, CancellationToken cancellationToken)
    {
        var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.Id == request.InventarioId, cancellationToken);
        if (inventario == null) return false;

        if (inventario.Status == StatusInventario.Finalizado)
            throw new InvalidOperationException("Não é possível cancelar um inventário que já foi finalizado e teve ajustes aplicados.");

        inventario.Status = StatusInventario.Cancelado;
        inventario.DataFechamento = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Handle(CriarLoteCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == dto.ProdutoId, cancellationToken);
        if (produto == null)
            throw new KeyNotFoundException($"Produto {dto.ProdutoId} não encontrado.");

        var deposito = await _context.Depositos.FirstOrDefaultAsync(d => d.Id == dto.DepositoId, cancellationToken);
        if (deposito == null)
            throw new KeyNotFoundException($"Depósito {dto.DepositoId} não encontrado.");

        var lote = new Lote
        {
            EmpresaId = empresaId,
            ProdutoId = dto.ProdutoId,
            DepositoId = dto.DepositoId,
            CodigoLote = dto.CodigoLote.Trim().ToUpperInvariant(),
            DataFabricacao = dto.DataFabricacao,
            DataValidade = dto.DataValidade,
            QuantidadeInicial = dto.Quantidade,
            QuantidadeAtual = dto.Quantidade,
            CustoUnitario = dto.CustoUnitario > 0 ? dto.CustoUnitario : produto.PrecoCusto,
            Ativo = true,
            Observacoes = dto.Observacoes?.Trim()
        };

        _context.Lotes.Add(lote);
        await _context.SaveChangesAsync(cancellationToken);
        return lote.Id;
    }

    public async Task<int> Handle(CadastrarNumeroSerieCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        int empresaId = _currentEmpresaService.EmpresaId ?? 1;

        string numeroLimpo = dto.Numero.Trim().ToUpperInvariant();

        bool existe = await _context.NumerosSerie
            .AnyAsync(n => n.EmpresaId == empresaId && n.ProdutoId == dto.ProdutoId && n.Numero == numeroLimpo, cancellationToken);

        if (existe)
            throw new InvalidOperationException($"O número de série '{numeroLimpo}' já está cadastrado para este produto.");

        var serial = new NumeroSerie
        {
            EmpresaId = empresaId,
            ProdutoId = dto.ProdutoId,
            DepositoId = dto.DepositoId,
            LoteId = dto.LoteId,
            Numero = numeroLimpo,
            Status = StatusNumeroSerie.Disponivel,
            DataEntrada = DateTime.UtcNow,
            Observacoes = dto.Observacoes?.Trim()
        };

        _context.NumerosSerie.Add(serial);
        await _context.SaveChangesAsync(cancellationToken);
        return serial.Id;
    }
}
