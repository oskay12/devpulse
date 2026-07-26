namespace DevPulse.Core.Dtos;

/// <summary>
/// A comment on a pull request, either general or anchored to a file line.
/// </summary>
public class ReviewCommentDto
{
    /// <summary>Comment UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("pull_request_id")]
    public Guid PullRequestId { get; set; }

    /// <summary>Parent review UUID, when the comment belongs to a formal review</summary>
    [JsonPropertyName("review_id")]
    public Guid? ReviewId { get; set; }

    /// <summary>Comment author user UUID</summary>
    [JsonPropertyName("author_id")]
    public Guid AuthorId { get; set; }

    /// <summary>Author username, resolved for display</summary>
    [JsonPropertyName("author_username")]
    public string? AuthorUsername { get; set; }

    /// <summary>Comment body</summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>File path for inline comments</summary>
    [JsonPropertyName("file_path")]
    public string? FilePath { get; set; }

    /// <summary>Line number for inline comments</summary>
    [JsonPropertyName("line_number")]
    public int? LineNumber { get; set; }

    /// <summary>Creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last edit timestamp (UTC)</summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
