using DevPulse.Core.Enums;

namespace DevPulse.Core.Messages.Jobs;

/// <summary>
/// RabbitMQ message to trigger metrics calculation job.
/// Processed by DevPulse.Worker service.
/// </summary>
public class CalculateMetricsJob
{
    /// <summary>Unique job ID for tracking</summary>
    [JsonPropertyName("job_id")]
    [Required]
    public Guid JobId { get; set; }

    /// <summary>Target user (nullable for all users)</summary>
    [JsonPropertyName("user_id")]
    public Guid? UserId { get; set; }

    /// <summary>Target repository (nullable for all repos)</summary>
    [JsonPropertyName("repository_id")]
    public Guid? RepositoryId { get; set; }

    /// <summary>Aggregation period type</summary>
    [JsonPropertyName("period_type")]
    public MetricPeriodType PeriodType { get; set; }

    /// <summary>Period start date (UTC)</summary>
    [JsonPropertyName("period_start")]
    public DateTime PeriodStart { get; set; }

    /// <summary>Period end date (UTC)</summary>
    [JsonPropertyName("period_end")]
    public DateTime PeriodEnd { get; set; }

    /// <summary>When job was queued (UTC)</summary>
    [JsonPropertyName("queued_at")]
    public DateTime QueuedAt { get; set; }
}
