namespace DevPulse.Core.Dtos;

/// <summary>
/// Review comment search result item with highlights.
/// </summary>
public class ReviewSearchResultDto
{
    /// <summary>Review comment UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("pull_request_id")]
    public Guid PullRequestId { get; set; }

    /// <summary>PR number</summary>
    [JsonPropertyName("pr_number")]
    public int PrNumber { get; set; }

    /// <summary>Repository name</summary>
    [JsonPropertyName("repository_name")]
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>Comment author name</summary>
    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Comment body</summary>
    [JsonPropertyName("comment_body")]
    public string CommentBody { get; set; } = string.Empty;

    /// <summary>File path for inline comments</summary>
    [JsonPropertyName("file_path")]
    public string? FilePath { get; set; }

    /// <summary>Line number for inline comments</summary>
    [JsonPropertyName("line_number")]
    public int? LineNumber { get; set; }

    /// <summary>Creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>OpenSearch highlight snippets</summary>
    [JsonPropertyName("highlight_snippets")]
    public List<string> HighlightSnippets { get; set; } = new();
}
