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

public class EstoqueCommandsHandler :
    IRequestHandler<CriarDepositoCommand, int>,
    IRequestHandler<AtualizarDepositoCommand, bool>,
    IRequestHandler<RegistrarMovimentacaoEstoqueCommand, int>
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
            DataMovimentacao = DateTime.UtcNow
        };

        _context.MovimentacoesEstoque.Add(movimentacao);
        await _context.SaveChangesAsync(cancellationToken);

        return movimentacao.Id;
    }
}
