namespace DevPulse.Core.Dtos;

/// <summary>
/// Pull request search result item with highlights.
/// </summary>
public class PullRequestSearchResultDto
{
    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Repository name</summary>
    [JsonPropertyName("repository_name")]
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>PR number</summary>
    [JsonPropertyName("pr_number")]
    public int PrNumber { get; set; }

    /// <summary>PR title</summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Author name</summary>
    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>PR state</summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    /// <summary>Creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Merge timestamp (UTC)</summary>
    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }

    /// <summary>OpenSearch highlight snippets</summary>
    [JsonPropertyName("highlight_snippets")]
    public List<string> HighlightSnippets { get; set; } = new();
}
