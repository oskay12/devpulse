namespace DevPulse.Core.Messages;

/// <summary>
/// Webhook event for code review submissions.
/// Triggers review indexing and notification workflows.
/// </summary>
public class ReviewWebhookEvent : WebhookEvent
{
    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("pull_request_id")]
    [Required]
    public Guid PullRequestId { get; set; }

    /// <summary>PR number</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>Reviewer UUID</summary>
    [JsonPropertyName("reviewer_id")]
    [Required]
    public Guid ReviewerId { get; set; }

    /// <summary>Review state (e.g., "approved", "changes_requested")</summary>
    [JsonPropertyName("state")]
    [Required]
    public string State { get; set; } = string.Empty;

    /// <summary>Review comment (nullable)</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}
