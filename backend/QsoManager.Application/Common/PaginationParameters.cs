namespace QsoManager.Application.Common;

/// <summary>
/// Paramètres de pagination pour les requêtes
/// </summary>
public class PaginationParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    /// <summary>
    /// Numéro de page (commence à 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Taille de la page (nombre d'éléments par page)
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }

    /// <summary>
    /// Nombre d'éléments à ignorer (skip)
    /// </summary>
    public int Skip => (PageNumber - 1) * PageSize;

    /// <summary>
    /// Valide les paramètres de pagination
    /// </summary>
    public bool IsValid => PageNumber > 0 && PageSize > 0;
}
