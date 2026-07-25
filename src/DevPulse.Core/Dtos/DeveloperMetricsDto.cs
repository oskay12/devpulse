namespace DevPulse.Core.Dtos;

/// <summary>
/// Developer metrics summary.
/// </summary>
public class DeveloperMetricsDto
{
    /// <summary>Total commits across all repositories</summary>
    [JsonPropertyName("total_commits")]
    public int TotalCommits { get; set; }

    /// <summary>Total pull requests created</summary>
    [JsonPropertyName("total_pull_requests")]
    public int TotalPullRequests { get; set; }

    /// <summary>Number of code reviews performed</summary>
    [JsonPropertyName("code_reviews")]
    public int CodeReviews { get; set; }

    /// <summary>Average PR review time in hours</summary>
    [JsonPropertyName("average_review_time_hours")]
    public decimal AverageReviewTimeHours { get; set; }

    /// <summary>Productivity score (0-100)</summary>
    [JsonPropertyName("productivity_score")]
    public decimal ProductivityScore { get; set; }
}
