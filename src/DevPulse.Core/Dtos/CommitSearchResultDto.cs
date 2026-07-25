namespace DevPulse.Core.Dtos;

/// <summary>
/// Commit search result item with highlights.
/// </summary>
public class CommitSearchResultDto
{
    /// <summary>Commit SHA</summary>
    [JsonPropertyName("sha")]
    public string Sha { get; set; } = string.Empty;

    /// <summary>Repository name</summary>
    [JsonPropertyName("repository_name")]
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>Author name</summary>
    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Commit message</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Commit timestamp (UTC)</summary>
    [JsonPropertyName("committed_at")]
    public DateTime CommittedAt { get; set; }

    /// <summary>OpenSearch highlight snippets</summary>
    [JsonPropertyName("highlight_snippets")]
    public List<string> HighlightSnippets { get; set; } = new();
}
