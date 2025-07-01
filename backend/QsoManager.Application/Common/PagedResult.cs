namespace QsoManager.Application.Common;

/// <summary>
/// Résultat paginé générique
/// </summary>
/// <typeparam name="T">Type des éléments de la page</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Éléments de la page courante
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Numéro de page courante (commence à 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Taille de la page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Nombre total d'éléments
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Nombre total de pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Indique s'il y a une page précédente
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indique s'il y a une page suivante
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Constructeur
    /// </summary>
    public PagedResult(IEnumerable<T> items, long totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Constructeur par défaut
    /// </summary>
    public PagedResult()
    {
    }
}
