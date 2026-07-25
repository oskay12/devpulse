namespace DevPulse.Core.Dtos;

/// <summary>
/// Generic paginated search results wrapper.
/// </summary>
public class SearchResultDto<T>
{
    /// <summary>Total number of hits</summary>
    [JsonPropertyName("total_hits")]
    public int TotalHits { get; set; }

    /// <summary>Current page number (1-indexed)</summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>Results per page</summary>
    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    /// <summary>Result items</summary>
    [JsonPropertyName("results")]
    public List<T> Results { get; set; } = new();

    /// <summary>Search execution time in milliseconds</summary>
    [JsonPropertyName("search_time_ms")]
    public double SearchTimeMs { get; set; }
}
