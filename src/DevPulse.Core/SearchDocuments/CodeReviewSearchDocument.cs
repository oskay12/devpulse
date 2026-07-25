namespace DevPulse.Core.SearchDocuments;

/// <summary>
/// OpenSearch document for code review comments search.
/// Enables searching across all review discussions.
/// </summary>
public class CodeReviewSearchDocument
{
    /// <summary>Document ID (review comment UUID as string)</summary>
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("pull_request_id")]
    [Required]
    public Guid PullRequestId { get; set; }

    /// <summary>PR number (for display)</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>Repository full name</summary>
    [JsonPropertyName("repository_name")]
    [Required]
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>Comment author username</summary>
    [JsonPropertyName("author_name")]
    [Required]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Comment body (indexed)</summary>
    [JsonPropertyName("comment_body")]
    [Required]
    public string CommentBody { get; set; } = string.Empty;

    /// <summary>File path for inline comments (nullable)</summary>
    [JsonPropertyName("file_path")]
    public string? FilePath { get; set; }

    /// <summary>Line number for inline comments (nullable)</summary>
    [JsonPropertyName("line_number")]
    public int? LineNumber { get; set; }

    /// <summary>Comment creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}
