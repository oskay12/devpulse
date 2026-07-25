using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// Represents a file uploaded to S3 bucket.
/// Tracks diagrams, screenshots, charts, and generated reports.
/// </summary>
public class MediaAsset
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository (nullable for user-uploaded assets)</summary>
    [JsonPropertyName("repository_id")]
    public Guid? RepositoryId { get; set; }

    /// <summary>Foreign key to PullRequest (nullable if not PR-related)</summary>
    [JsonPropertyName("pull_request_id")]
    public Guid? PullRequestId { get; set; }

    /// <summary>Foreign key to User (uploader)</summary>
    [JsonPropertyName("uploaded_by_id")]
    [Required]
    public Guid UploadedById { get; set; }

    /// <summary>Original filename</summary>
    [JsonPropertyName("file_name")]
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>S3 object key (path in bucket)</summary>
    [JsonPropertyName("s3_key")]
    [Required]
    public string S3Key { get; set; } = string.Empty;

    /// <summary>Public CloudFront/S3 URL</summary>
    [JsonPropertyName("s3_url")]
    [Required]
    [Url]
    public string S3Url { get; set; } = string.Empty;

    /// <summary>Asset type category</summary>
    [JsonPropertyName("type")]
    public MediaAssetType Type { get; set; }

    /// <summary>File size in bytes</summary>
    [JsonPropertyName("file_size_bytes")]
    public long FileSizeBytes { get; set; }

    /// <summary>MIME type (e.g., "image/png", "application/pdf")</summary>
    [JsonPropertyName("mime_type")]
    [Required]
    [StringLength(100)]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Upload timestamp (UTC)</summary>
    [JsonPropertyName("uploaded_at")]
    public DateTime UploadedAt { get; set; }

    /// <summary>Lambda optimization completion flag</summary>
    [JsonPropertyName("is_optimized")]
    public bool IsOptimized { get; set; }

    /// <summary>S3 key for optimized version (nullable)</summary>
    [JsonPropertyName("optimized_s3_key")]
    public string? OptimizedS3Key { get; set; }
}
