using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Models;
using ERP.Application.Financeiro.Conciliacao.DTOs;
using ERP.Domain.Core.Entities.Financeiro;
using ERP.Domain.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Financeiro.Conciliacao.Commands;

public record ImportarExtratoOfxCommand(
    int ContaBancariaId,
    Stream Arquivo,
    string NomeArquivo
) : IRequest<Result<ImportacaoOfxResultDto>>;

public class ImportarExtratoOfxCommandHandler : IRequestHandler<ImportarExtratoOfxCommand, Result<ImportacaoOfxResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IOfxParserService _ofxParser;
    private readonly ICurrentEmpresaService _empresaService;

    public ImportarExtratoOfxCommandHandler(
        IApplicationDbContext context,
        IOfxParserService ofxParser,
        ICurrentEmpresaService empresaService)
    {
        _context = context;
        _ofxParser = ofxParser;
        _empresaService = empresaService;
    }

    public async Task<Result<ImportacaoOfxResultDto>> Handle(ImportarExtratoOfxCommand request, CancellationToken cancellationToken)
    {
        var empresaId = _empresaService.EmpresaId ?? 0;
        if (empresaId == 0)
            return Result<ImportacaoOfxResultDto>.Falha("Nenhuma empresa ativa selecionada.");

        var contaBancaria = await _context.ContasBancarias
            .FirstOrDefaultAsync(c => c.Id == request.ContaBancariaId && c.EmpresaId == empresaId, cancellationToken);

        if (contaBancaria == null)
            return Result<ImportacaoOfxResultDto>.Falha("Conta bancária não encontrada.");

        OfxExtrato ofxExtrato;
        try
        {
            ofxExtrato = _ofxParser.Parse(request.Arquivo);
        }
        catch (Exception ex)
        {
            return Result<ImportacaoOfxResultDto>.Falha($"Erro ao processar o arquivo OFX: {ex.Message}");
        }

        if (ofxExtrato.Transacoes == null || !ofxExtrato.Transacoes.Any())
            return Result<ImportacaoOfxResultDto>.Falha("Nenhuma transação encontrada no arquivo OFX informado.");

        // Obter FITIDs já existentes para esta conta bancária a fim de evitar duplicidades
        var fitIdsList = await _context.ExtratosImportadosItens
            .AsNoTracking()
            .Where(i => i.EmpresaId == empresaId && i.ExtratoImportado.ContaBancariaId == request.ContaBancariaId && !string.IsNullOrEmpty(i.TransacaoId))
            .Select(i => i.TransacaoId)
            .ToListAsync(cancellationToken);
        var fitIdsExistentes = fitIdsList.ToHashSet();

        var extrato = new ExtratoImportado
        {
            EmpresaId = empresaId,
            ContaBancariaId = request.ContaBancariaId,
            NomeArquivo = Path.GetFileName(request.NomeArquivo),
            DataImportacao = DateTime.UtcNow,
            DataInicioExtrato = ofxExtrato.DataInicio,
            DataFimExtrato = ofxExtrato.DataFim
        };

        int totalTransacoes = ofxExtrato.Transacoes.Count;
        int duplicadas = 0;
        int novas = 0;
        decimal creditos = 0;
        decimal debitos = 0;

        foreach (var trn in ofxExtrato.Transacoes)
        {
            if (!string.IsNullOrEmpty(trn.TransacaoId) && fitIdsExistentes.Contains(trn.TransacaoId))
            {
                duplicadas++;
                continue;
            }

            novas++;
            if (trn.Valor > 0)
                creditos += trn.Valor;
            else
                debitos += Math.Abs(trn.Valor);

            var item = new ExtratoImportadoItem
            {
                EmpresaId = empresaId,
                TransacaoId = trn.TransacaoId,
                Data = trn.Data.ToUniversalTime(),
                Valor = trn.Valor,
                Descricao = trn.Descricao,
                TipoTransacao = trn.TipoTransacao,
                StatusConciliacao = StatusConciliacao.Pendente
            };

            extrato.Itens.Add(item);
        }

        extrato.TotalRegistros = extrato.Itens.Count;
        extrato.TotalCreditos = creditos;
        extrato.TotalDebitos = debitos;

        _context.ExtratosImportados.Add(extrato);
        await _context.SaveChangesAsync(cancellationToken);

        var resultDto = new ImportacaoOfxResultDto(
            ExtratoImportadoId: extrato.Id,
            TotalTransacoes: totalTransacoes,
            TransacoesNovas: novas,
            TransacoesDuplicadas: duplicadas,
            TotalCreditos: creditos,
            TotalDebitos: debitos,
            DataInicio: ofxExtrato.DataInicio,
            DataFim: ofxExtrato.DataFim
        );

        return Result<ImportacaoOfxResultDto>.Ok(resultDto);
    }
}
