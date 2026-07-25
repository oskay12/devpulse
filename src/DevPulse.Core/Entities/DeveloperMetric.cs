using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// Aggregated developer performance metrics for a time period.
/// Calculated asynchronously by worker services.
/// </summary>
public class DeveloperMetric
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to User</summary>
    [JsonPropertyName("user_id")]
    [Required]
    public Guid UserId { get; set; }

    /// <summary>Foreign key to Repository (nullable for global metrics)</summary>
    [JsonPropertyName("repository_id")]
    public Guid? RepositoryId { get; set; }

    /// <summary>Period start timestamp (UTC)</summary>
    [JsonPropertyName("period_start")]
    public DateTime PeriodStart { get; set; }

    /// <summary>Period end timestamp (UTC)</summary>
    [JsonPropertyName("period_end")]
    public DateTime PeriodEnd { get; set; }

    /// <summary>Aggregation period type</summary>
    [JsonPropertyName("period_type")]
    public MetricPeriodType PeriodType { get; set; }

    /// <summary>Total commits authored in period</summary>
    [JsonPropertyName("total_commits")]
    public int TotalCommits { get; set; }

    /// <summary>Total pull requests created</summary>
    [JsonPropertyName("total_pull_requests")]
    public int TotalPullRequests { get; set; }

    /// <summary>Number of PRs reviewed</summary>
    [JsonPropertyName("pull_requests_reviewed")]
    public int PullRequestsReviewed { get; set; }

    /// <summary>Total lines added</summary>
    [JsonPropertyName("lines_added")]
    public int LinesAdded { get; set; }

    /// <summary>Total lines deleted</summary>
    [JsonPropertyName("lines_deleted")]
    public int LinesDeleted { get; set; }

    /// <summary>Number of issues closed</summary>
    [JsonPropertyName("issues_closed")]
    public int IssuesClosed { get; set; }

    /// <summary>Average time to review PRs (in hours)</summary>
    [JsonPropertyName("average_review_time")]
    public decimal AverageReviewTime { get; set; }

    /// <summary>Average time for own PRs to be merged (in hours)</summary>
    [JsonPropertyName("average_pr_merge_time")]
    public decimal AveragePrMergeTime { get; set; }

    /// <summary>Code churn rate: (Added + Deleted) / Total LOC</summary>
    [JsonPropertyName("code_churn_rate")]
    public decimal CodeChurnRate { get; set; }

    /// <summary>When metrics were calculated (UTC)</summary>
    [JsonPropertyName("calculated_at")]
    public DateTime CalculatedAt { get; set; }
}
