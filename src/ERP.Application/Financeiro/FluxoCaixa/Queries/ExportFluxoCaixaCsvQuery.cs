using ERP.Application.Financeiro.FluxoCaixa.DTOs;
using ERP.Domain.Core.Enums;
using MediatR;
using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Financeiro.FluxoCaixa.Queries;

public record ExportFluxoCaixaCsvQuery(
    DateTime DataInicio,
    DateTime DataFim,
    int? ContaBancariaId = null,
    TipoAgrupamentoFluxoCaixa Agrupamento = TipoAgrupamentoFluxoCaixa.Diario,
    bool IncluirProjetado = true
) : IRequest<byte[]>;

public class ExportFluxoCaixaCsvQueryHandler : IRequestHandler<ExportFluxoCaixaCsvQuery, byte[]>
{
    private readonly IMediator _mediator;

    public ExportFluxoCaixaCsvQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<byte[]> Handle(ExportFluxoCaixaCsvQuery request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFluxoCaixaQuery(
            request.DataInicio,
            request.DataFim,
            request.ContaBancariaId,
            request.Agrupamento,
            request.IncluirProjetado
        ), cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("Data;Categoria;Descrição;Tipo;Origem;Entradas;Saídas;Saldo Acumulado");

        var culture = new CultureInfo("pt-BR");

        foreach (var periodo in result.Periodos)
        {
            foreach (var item in periodo.Itens)
            {
                var data = item.Data.ToString("dd/MM/yyyy", culture);
                var categoria = item.Categoria ?? string.Empty;
                var descricao = item.Descricao ?? string.Empty;
                var tipo = item.Tipo ?? string.Empty;
                var origem = item.Origem ?? string.Empty;
                
                var entradas = item.Entrada.ToString("N2", culture);
                var saidas = item.Saida.ToString("N2", culture);
                var saldoAcumulado = item.SaldoAcumulado.ToString("N2", culture);

                csv.AppendLine($"{data};{categoria};{descricao};{tipo};{origem};{entradas};{saidas};{saldoAcumulado}");
            }
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var contentBytes = Encoding.UTF8.GetBytes(csv.ToString());
        var finalBytes = new byte[preamble.Length + contentBytes.Length];
        
        Buffer.BlockCopy(preamble, 0, finalBytes, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, finalBytes, preamble.Length, contentBytes.Length);

        return finalBytes;
    }
}
