namespace DevPulse.Core.Messages.Jobs;

/// <summary>
/// RabbitMQ message to trigger Lambda image optimization.
/// Alternative to S3 event trigger for manual optimization.
/// </summary>
public class OptimizeImageJob
{
    /// <summary>Unique job ID</summary>
    [JsonPropertyName("job_id")]
    [Required]
    public Guid JobId { get; set; }

    /// <summary>Media asset UUID</summary>
    [JsonPropertyName("media_asset_id")]
    [Required]
    public Guid MediaAssetId { get; set; }

    /// <summary>S3 object key</summary>
    [JsonPropertyName("s3_key")]
    [Required]
    public string S3Key { get; set; } = string.Empty;

    /// <summary>Queue timestamp (UTC)</summary>
    [JsonPropertyName("queued_at")]
    public DateTime QueuedAt { get; set; }
}
