namespace DevPulse.Core.SearchDocuments;

/// <summary>
/// OpenSearch document for commit full-text search.
/// Indexed with commit messages, file paths, and diff snippets.
/// </summary>
public class CommitSearchDocument
{
    /// <summary>Document ID (commit SHA-1)</summary>
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository full name (for display)</summary>
    [JsonPropertyName("repository_name")]
    [Required]
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>Commit author name</summary>
    [JsonPropertyName("author_name")]
    [Required]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Commit author email</summary>
    [JsonPropertyName("author_email")]
    [Required]
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>Full commit message (indexed for search)</summary>
    [JsonPropertyName("message")]
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>Branch name</summary>
    [JsonPropertyName("branch")]
    [Required]
    public string Branch { get; set; } = string.Empty;

    /// <summary>List of changed file paths (indexed)</summary>
    [JsonPropertyName("file_paths")]
    public List<string> FilePaths { get; set; } = new();

    /// <summary>Code diff snippets for content search</summary>
    [JsonPropertyName("diff_snippets")]
    public List<string> DiffSnippets { get; set; } = new();

    /// <summary>Commit timestamp (UTC, for sorting)</summary>
    [JsonPropertyName("committed_at")]
    public DateTime CommittedAt { get; set; }

    /// <summary>Lines added (for filtering)</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Lines deleted (for filtering)</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }
}
