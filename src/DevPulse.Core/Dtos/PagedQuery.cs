namespace DevPulse.Core.Dtos;

/// <summary>
/// Common pagination parameters for list endpoints.
/// </summary>
public class PagedQuery
{
    /// <summary>Largest page size a caller may request.</summary>
    public const int MaxPageSize = 100;

    private const int DefaultPageSize = 20;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>Page number (1-indexed). Values below 1 are clamped to 1.</summary>
    [JsonPropertyName("page")]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Items per page. Clamped to <see cref="MaxPageSize"/> rather than rejected,
    /// so an over-eager client gets data instead of a 400 — while the database
    /// never sees an unbounded <c>Take</c>.
    /// </summary>
    [JsonPropertyName("page_size")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>Number of records to skip for the current page.</summary>
    [JsonIgnore]
    public int Skip => (Page - 1) * PageSize;
}
