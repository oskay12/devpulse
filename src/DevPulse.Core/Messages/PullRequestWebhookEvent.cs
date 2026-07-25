namespace DevPulse.Core.Messages;

/// <summary>
/// Webhook event for pull request lifecycle changes.
/// Triggers PR entity updates and review workflows.
/// </summary>
public class PullRequestWebhookEvent : WebhookEvent
{
    /// <summary>Action type (e.g., "opened", "closed", "merged", "reopened")</summary>
    [JsonPropertyName("action")]
    [Required]
    public string Action { get; set; } = string.Empty;

    /// <summary>PR number</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>PR title</summary>
    [JsonPropertyName("title")]
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>PR description</summary>
    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>PR author UUID</summary>
    [JsonPropertyName("author_id")]
    [Required]
    public Guid AuthorId { get; set; }

    /// <summary>Source branch</summary>
    [JsonPropertyName("source_branch")]
    [Required]
    public string SourceBranch { get; set; } = string.Empty;

    /// <summary>Target branch</summary>
    [JsonPropertyName("target_branch")]
    [Required]
    public string TargetBranch { get; set; } = string.Empty;
}
