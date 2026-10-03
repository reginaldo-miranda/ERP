namespace ERP.Application.Common.Interfaces;

public record OfxTransacao(
    string TransacaoId,
    DateTime Data,
    decimal Valor,
    string Descricao,
    string TipoTransacao
);

public record OfxExtrato(
    string BancoId,
    string ContaNumero,
    DateTime DataInicio,
    DateTime DataFim,
    decimal SaldoFinal,
    List<OfxTransacao> Transacoes
);

public interface IOfxParserService
{
    OfxExtrato Parse(Stream stream);
}
