using ERP.Application.Common.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ERP.Infrastructure.Services;

public class OfxParserService : IOfxParserService
{
    public OfxExtrato Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        // Detect if it's XML-based OFX 2.x or SGML-based OFX 1.x
        if (content.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
            content.TrimStart().StartsWith("<OFX", StringComparison.OrdinalIgnoreCase))
        {
            return ParseXml(content);
        }

        return ParseSgml(content);
    }

    private OfxExtrato ParseSgml(string content)
    {
        var bancoId = ExtractTagValue(content, "BANKID") ?? string.Empty;
        var contaNumero = ExtractTagValue(content, "ACCTID") ?? string.Empty;
        var dtStart = ParseOfxDate(ExtractTagValue(content, "DTSTART"));
        var dtEnd = ParseOfxDate(ExtractTagValue(content, "DTEND"));
        var balAmt = ParseDecimal(ExtractTagValue(content, "BALAMT"));

        var transacoes = new List<OfxTransacao>();

        // Split by STMTTRN blocks
        var trnPattern = @"<STMTTRN>(.*?)(?=<STMTTRN>|</STMTTRN>|</?BANKTRANLIST|$)";
        var matches = Regex.Matches(content, trnPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var block = match.Groups[1].Value;

            var trnType = ExtractTagValue(block, "TRNTYPE") ?? "OTHER";
            var dtPosted = ParseOfxDate(ExtractTagValue(block, "DTPOSTED"));
            var trnAmt = ParseDecimal(ExtractTagValue(block, "TRNAMT"));
            var fitId = ExtractTagValue(block, "FITID") ?? Guid.NewGuid().ToString();
            var name = ExtractTagValue(block, "NAME");
            var memo = ExtractTagValue(block, "MEMO");
            var descricao = !string.IsNullOrWhiteSpace(name) ? name : (memo ?? "Transação sem descrição");
            if (!string.IsNullOrWhiteSpace(memo) && !string.IsNullOrWhiteSpace(name) && name != memo)
                descricao = $"{name} - {memo}";

            transacoes.Add(new OfxTransacao(
                TransacaoId: fitId.Trim(),
                Data: dtPosted,
                Valor: trnAmt,
                Descricao: descricao.Trim(),
                TipoTransacao: trnType.Trim().ToUpperInvariant()
            ));
        }

        return new OfxExtrato(
            BancoId: bancoId.Trim(),
            ContaNumero: contaNumero.Trim(),
            DataInicio: dtStart,
            DataFim: dtEnd,
            SaldoFinal: balAmt,
            Transacoes: transacoes
        );
    }

    private OfxExtrato ParseXml(string content)
    {
        // XML OFX follows similar structure but with proper closing tags
        // Reuse SGML parser since regex handles both
        return ParseSgml(content);
    }

    private static string? ExtractTagValue(string content, string tagName)
    {
        // Pattern: <TAGNAME>value (value ends at newline or next tag)
        var pattern = $@"<{tagName}>\s*([^<\r\n]+)";
        var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static DateTime ParseOfxDate(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return DateTime.UtcNow;

        // OFX date: YYYYMMDDHHMMSS or YYYYMMDD or YYYYMMDDHHMMSS[-X:GMT]
        dateStr = dateStr.Trim();
        // Remove timezone info
        var bracketIndex = dateStr.IndexOf('[');
        if (bracketIndex > 0) dateStr = dateStr[..bracketIndex];

        string[] formats = {
            "yyyyMMddHHmmss",
            "yyyyMMddHHmm",
            "yyyyMMdd",
            "yyyyMMddHHmmss.fff"
        };

        if (DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
            return result.ToUniversalTime();

        return DateTime.UtcNow;
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        value = value.Trim().Replace(',', '.');
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return 0;
    }
}
