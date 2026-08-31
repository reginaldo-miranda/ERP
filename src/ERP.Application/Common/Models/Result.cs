namespace ERP.Application.Common.Models;

/// <summary>
/// Classe genérica de resultado para padronizar retornos de operações.
/// </summary>
public class Result
{
    public bool Sucesso { get; }
    public string[] Erros { get; }

    protected Result(bool sucesso, IEnumerable<string> erros)
    {
        Sucesso = sucesso;
        Erros = erros.ToArray();
    }

    public static Result Ok() => new(true, Array.Empty<string>());
    public static Result Falha(IEnumerable<string> erros) => new(false, erros);
    public static Result Falha(string erro) => new(false, new[] { erro });
}

/// <summary>
/// Resultado com dados de retorno.
/// </summary>
public class Result<T> : Result
{
    public T? Dados { get; }

    private Result(bool sucesso, T? dados, IEnumerable<string> erros) : base(sucesso, erros)
    {
        Dados = dados;
    }

    public static Result<T> Ok(T dados) => new(true, dados, Array.Empty<string>());
    public static new Result<T> Falha(IEnumerable<string> erros) => new(false, default, erros);
    public static new Result<T> Falha(string erro) => new(false, default, new[] { erro });
}
