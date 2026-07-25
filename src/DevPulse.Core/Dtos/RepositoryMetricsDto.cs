namespace DevPulse.Core.Dtos;

/// <summary>
/// Repository analytics response DTO.
/// Aggregates key metrics and trends for dashboard display.
/// </summary>
public class RepositoryMetricsDto
{
    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository full name</summary>
    [JsonPropertyName("repository_name")]
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>Total commit count</summary>
    [JsonPropertyName("total_commits")]
    public int TotalCommits { get; set; }

    /// <summary>Total pull request count</summary>
    [JsonPropertyName("total_pull_requests")]
    public int TotalPullRequests { get; set; }

    /// <summary>Number of active contributors</summary>
    [JsonPropertyName("active_contributors")]
    public int ActiveContributors { get; set; }

    /// <summary>Overall code health score (0-100)</summary>
    [JsonPropertyName("code_health_score")]
    public decimal CodeHealthScore { get; set; }

    /// <summary>Top contributors list</summary>
    [JsonPropertyName("top_contributors")]
    public List<TopContributorDto> TopContributors { get; set; } = new();

    /// <summary>Commit trend over time</summary>
    [JsonPropertyName("commit_trend")]
    public List<MetricDataPointDto> CommitTrend { get; set; } = new();
}
