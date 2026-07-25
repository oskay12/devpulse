namespace DevPulse.Core.Entities;

/// <summary>
/// Represents an inline code review comment.
/// Can be associated with specific file/line or general PR comment.
/// </summary>
public class ReviewComment
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to PullRequest</summary>
    [JsonPropertyName("pull_request_id")]
    [Required]
    public Guid PullRequestId { get; set; }

    /// <summary>Foreign key to PullRequestReview (nullable for standalone comments)</summary>
    [JsonPropertyName("review_id")]
    public Guid? ReviewId { get; set; }

    /// <summary>Foreign key to User (comment author)</summary>
    [JsonPropertyName("author_id")]
    [Required]
    public Guid AuthorId { get; set; }

    /// <summary>Comment body (Markdown)</summary>
    [JsonPropertyName("body")]
    [Required]
    public string Body { get; set; } = string.Empty;

    /// <summary>File path for inline comments (nullable for general comments)</summary>
    [JsonPropertyName("file_path")]
    public string? FilePath { get; set; }

    /// <summary>Line number for inline comments (nullable)</summary>
    [JsonPropertyName("line_number")]
    public int? LineNumber { get; set; }

    /// <summary>Comment creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last edit timestamp (UTC, nullable)</summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
