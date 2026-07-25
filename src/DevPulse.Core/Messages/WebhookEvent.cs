using DevPulse.Core.Enums;

namespace DevPulse.Core.Messages;

/// <summary>
/// Base class for all webhook events received from Git providers.
/// Contains common metadata for event processing.
/// </summary>
public abstract class WebhookEvent
{
    /// <summary>Unique event ID for deduplication</summary>
    [JsonPropertyName("event_id")]
    [Required]
    public Guid EventId { get; set; }

    /// <summary>Event type identifier (e.g., "push", "pull_request")</summary>
    [JsonPropertyName("event_type")]
    [Required]
    public string EventType { get; set; } = string.Empty;

    /// <summary>Target repository UUID</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Source provider</summary>
    [JsonPropertyName("provider")]
    public RepositoryProvider Provider { get; set; }

    /// <summary>When webhook was received (UTC)</summary>
    [JsonPropertyName("received_at")]
    public DateTime ReceivedAt { get; set; }

    /// <summary>Original JSON payload (for audit/debugging)</summary>
    [JsonPropertyName("raw_payload")]
    [Required]
    public string RawPayload { get; set; } = string.Empty;
}
