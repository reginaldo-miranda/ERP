using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Financeiro.Cadastros.DTOs;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.Cadastros.Commands;

// Conta Bancária
public record CreateContaBancariaCommand(
    string Descricao,
    TipoContaBancaria Tipo,
    int? BancoId,
    string? Agencia,
    string? AgenciaDigito,
    string? Conta,
    string? ContaDigito,
    decimal SaldoInicial,
    string? Observacoes
) : IRequest<Result<ContaBancariaDto>>;

public class CreateContaBancariaCommandValidator : AbstractValidator<CreateContaBancariaCommand>
{
    public CreateContaBancariaCommandValidator()
    {
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(150).WithMessage("Descrição é obrigatória.");
    }
}

public class CreateContaBancariaCommandHandler : IRequestHandler<CreateContaBancariaCommand, Result<ContaBancariaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateContaBancariaCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<ContaBancariaDto>> Handle(CreateContaBancariaCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        var entity = new ContaBancaria
        {
            EmpresaId = empresaId,
            Descricao = request.Descricao.Trim(),
            Tipo = request.Tipo,
            BancoId = request.BancoId,
            Agencia = request.Agencia?.Trim(),
            AgenciaDigito = request.AgenciaDigito?.Trim(),
            Conta = request.Conta?.Trim(),
            ContaDigito = request.ContaDigito?.Trim(),
            SaldoInicial = request.SaldoInicial,
            SaldoAtual = request.SaldoInicial,
            Observacoes = request.Observacoes?.Trim(),
            Ativa = true
        };

        _context.ContasBancarias.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ContaBancariaDto>.Ok(new ContaBancariaDto(
            entity.Id, entity.Descricao, entity.Tipo, entity.BancoId, null,
            entity.Agencia, entity.AgenciaDigito, entity.Conta, entity.ContaDigito,
            entity.SaldoInicial, entity.SaldoAtual, entity.Ativa, entity.Observacoes
        ));
    }
}

// Forma de Pagamento
public record CreateFormaPagamentoCommand(
    string Nome,
    TipoFormaPagamento Tipo,
    int DiasCompensacao,
    decimal TaxaPercentual
) : IRequest<Result<FormaPagamentoDto>>;

public class CreateFormaPagamentoCommandHandler : IRequestHandler<CreateFormaPagamentoCommand, Result<FormaPagamentoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateFormaPagamentoCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<FormaPagamentoDto>> Handle(CreateFormaPagamentoCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        var entity = new FormaPagamento
        {
            EmpresaId = empresaId,
            Nome = request.Nome.Trim(),
            Tipo = request.Tipo,
            DiasCompensacao = request.DiasCompensacao,
            TaxaPercentual = request.TaxaPercentual,
            Ativa = true
        };

        _context.FormasPagamento.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<FormaPagamentoDto>.Ok(new FormaPagamentoDto(
            entity.Id, entity.Nome, entity.Tipo, entity.DiasCompensacao, entity.TaxaPercentual, entity.Ativa
        ));
    }
}

// Plano de Contas
public record CreatePlanoContaCommand(
    string Codigo,
    string Descricao,
    TipoPlanoConta Tipo,
    bool Sintetica,
    int? PlanoContaPaiId
) : IRequest<Result<PlanoContaDto>>;

public class CreatePlanoContaCommandHandler : IRequestHandler<CreatePlanoContaCommand, Result<PlanoContaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreatePlanoContaCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<PlanoContaDto>> Handle(CreatePlanoContaCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        var codigo = request.Codigo.Trim();

        var jaExiste = await _context.PlanosContas.AnyAsync(p => p.Codigo == codigo, cancellationToken);
        if (jaExiste) return Result<PlanoContaDto>.Falha("Já existe uma conta contábil com este código.");

        var entity = new PlanoConta
        {
            EmpresaId = empresaId,
            Codigo = codigo,
            Descricao = request.Descricao.Trim(),
            Tipo = request.Tipo,
            Sintetica = request.Sintetica,
            PlanoContaPaiId = request.PlanoContaPaiId,
            Ativo = true
        };

        _context.PlanosContas.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<PlanoContaDto>.Ok(new PlanoContaDto(
            entity.Id, entity.Codigo, entity.Descricao, entity.Tipo, entity.Sintetica, entity.PlanoContaPaiId, null, entity.Ativo
        ));
    }
}

// Centro de Custo
public record CreateCentroCustoCommand(string Codigo, string Descricao) : IRequest<Result<CentroCustoDto>>;

public class CreateCentroCustoCommandHandler : IRequestHandler<CreateCentroCustoCommand, Result<CentroCustoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentEmpresaService _empresaService;

    public CreateCentroCustoCommandHandler(IApplicationDbContext context, ICurrentEmpresaService empresaService)
    {
        _context = context;
        _empresaService = empresaService;
    }

    public async Task<Result<CentroCustoDto>> Handle(CreateCentroCustoCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        var codigo = request.Codigo.Trim();

        var jaExiste = await _context.CentrosCusto.AnyAsync(c => c.Codigo == codigo, cancellationToken);
        if (jaExiste) return Result<CentroCustoDto>.Falha("Já existe um Centro de Custo com este código.");

        var entity = new CentroCusto
        {
            EmpresaId = empresaId,
            Codigo = codigo,
            Descricao = request.Descricao.Trim(),
            Ativo = true
        };

        _context.CentrosCusto.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CentroCustoDto>.Ok(new CentroCustoDto(entity.Id, entity.Codigo, entity.Descricao, entity.Ativo));
    }
}
