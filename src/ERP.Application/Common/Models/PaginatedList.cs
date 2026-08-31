using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Common.Models;

/// <summary>
/// Lista paginada para resultados de queries.
/// Encapsula dados, total de itens, página atual e total de páginas.
/// </summary>
public class PaginatedList<T>
{
    public IReadOnlyList<T> Itens { get; }
    public int PaginaAtual { get; }
    public int TotalPaginas { get; }
    public int TotalItens { get; }
    public int TamanhoPagina { get; }

    public bool TemPaginaAnterior => PaginaAtual > 1;
    public bool TemProximaPagina => PaginaAtual < TotalPaginas;

    public PaginatedList(IReadOnlyList<T> itens, int totalItens, int paginaAtual, int tamanhoPagina)
    {
        Itens = itens;
        TotalItens = totalItens;
        PaginaAtual = paginaAtual;
        TamanhoPagina = tamanhoPagina;
        TotalPaginas = (int)Math.Ceiling(totalItens / (double)tamanhoPagina);
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int paginaAtual, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((paginaAtual - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, paginaAtual, tamanhoPagina);
    }
}
