using DevPulse.Core.Enums;

namespace DevPulse.Core.Messages.Jobs;

/// <summary>
/// RabbitMQ message to index content in OpenSearch.
/// Processed asynchronously to avoid blocking API responses.
/// </summary>
public class IndexContentJob
{
    /// <summary>Unique job ID</summary>
    [JsonPropertyName("job_id")]
    [Required]
    public Guid JobId { get; set; }

    /// <summary>Type of content to index</summary>
    [JsonPropertyName("content_type")]
    public IndexContentType ContentType { get; set; }

    /// <summary>Entity UUID (commit, PR, or comment)</summary>
    [JsonPropertyName("entity_id")]
    [Required]
    public Guid EntityId { get; set; }

    /// <summary>Queue timestamp (UTC)</summary>
    [JsonPropertyName("queued_at")]
    public DateTime QueuedAt { get; set; }
}
