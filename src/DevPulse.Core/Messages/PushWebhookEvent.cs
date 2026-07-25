namespace DevPulse.Core.Messages;

/// <summary>
/// Webhook event for Git push containing new commits.
/// Triggers commit indexing and metrics calculation.
/// </summary>
public class PushWebhookEvent : WebhookEvent
{
    /// <summary>Branch that was pushed to</summary>
    [JsonPropertyName("branch")]
    [Required]
    public string Branch { get; set; } = string.Empty;

    /// <summary>List of commits in push</summary>
    [JsonPropertyName("commits")]
    [Required]
    public List<CommitPayload> Commits { get; set; } = new();

    /// <summary>User who performed the push</summary>
    [JsonPropertyName("pushed_by_id")]
    [Required]
    public Guid PushedById { get; set; }
}
