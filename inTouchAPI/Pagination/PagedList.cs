namespace inTouchAPI.Pagination;

/// <summary>
/// Klasa umożliwiająca utworzenie paginowanej listy z dowolnej kolekcji
/// </summary>
/// <typeparam name="T">Typ, który ma przyjmowana kolekcja</typeparam>
public class PagedList<T> : List<T>
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }

    /// <summary>
    /// Metoda asynchroniczna tworząca paginowaną listę z przyjętej kolekcji
    /// </summary>
    /// <param name="source">Kolekcja, która zostanie podzielona</param>
    /// <param name="pageNumber">Numer "strony" danego podziału</param>
    /// <param name="pageSize">Rozmiar mówiący o tym ile elementów będzie zawiewrała dzielona kolekcja</param>
    /// <returns></returns>
    public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
