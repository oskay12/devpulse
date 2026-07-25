namespace DevPulse.Core.SearchDocuments;

/// <summary>
/// OpenSearch document for pull request and review search.
/// Indexed with title, description, and review comments.
/// </summary>
public class PullRequestSearchDocument
{
    /// <summary>Document ID (PR UUID as string)</summary>
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository full name</summary>
    [JsonPropertyName("repository_name")]
    [Required]
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>PR number (e.g., 123)</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>PR title (indexed)</summary>
    [JsonPropertyName("title")]
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>PR description (indexed)</summary>
    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>PR author username</summary>
    [JsonPropertyName("author_name")]
    [Required]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>All review comment bodies (indexed)</summary>
    [JsonPropertyName("review_comments")]
    public List<string> ReviewComments { get; set; } = new();

    /// <summary>Reviewer usernames</summary>
    [JsonPropertyName("reviewers")]
    public List<string> Reviewers { get; set; } = new();

    /// <summary>PR state (for filtering)</summary>
    [JsonPropertyName("state")]
    [Required]
    public string State { get; set; } = string.Empty;

    /// <summary>Creation timestamp (UTC, for sorting)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Merge timestamp (UTC, nullable)</summary>
    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }
}
