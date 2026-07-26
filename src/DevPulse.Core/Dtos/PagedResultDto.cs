namespace DevPulse.Core.Dtos;

/// <summary>
/// Generic paginated collection wrapper for CRUD list endpoints.
/// </summary>
/// <remarks>
/// Distinct from <see cref="SearchResultDto{T}"/>, which carries search-specific
/// members such as the OpenSearch execution time.
/// </remarks>
/// <typeparam name="T">Item type.</typeparam>
public class PagedResultDto<T>
{
    /// <summary>Total number of records matching the query, ignoring pagination</summary>
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    /// <summary>Current page number (1-indexed)</summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>Items per page</summary>
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    /// <summary>Total number of pages available</summary>
    [JsonPropertyName("total_pages")]
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize)
        : 0;

    /// <summary>Items on the current page</summary>
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();
}
