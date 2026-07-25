namespace DevPulse.Core.Entities;

/// <summary>
/// Represents a single Git commit.
/// Stores metadata and statistics for analytical processing.
/// </summary>
public class Commit
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Git commit SHA-1 hash (40 characters)</summary>
    [JsonPropertyName("sha")]
    [Required]
    [StringLength(40, MinimumLength = 40)]
    public string Sha { get; set; } = string.Empty;

    /// <summary>Foreign key to User (commit author, nullable if not registered)</summary>
    [JsonPropertyName("author_id")]
    public Guid? AuthorId { get; set; }

    /// <summary>Git author name from commit metadata</summary>
    [JsonPropertyName("author_name")]
    [Required]
    [StringLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Git author email from commit metadata</summary>
    [JsonPropertyName("author_email")]
    [Required]
    [EmailAddress]
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>Commit message (full text)</summary>
    [JsonPropertyName("message")]
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>Branch name where commit was pushed</summary>
    [JsonPropertyName("branch")]
    [Required]
    [StringLength(200)]
    public string Branch { get; set; } = string.Empty;

    /// <summary>Git commit timestamp (UTC)</summary>
    [JsonPropertyName("committed_at")]
    public DateTime CommittedAt { get; set; }

    /// <summary>When commit was indexed in DevPulse (UTC)</summary>
    [JsonPropertyName("indexed_at")]
    public DateTime IndexedAt { get; set; }

    /// <summary>Number of files modified in commit</summary>
    [JsonPropertyName("files_changed")]
    public int FilesChanged { get; set; }

    /// <summary>Total lines added</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Total lines deleted</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Parent commit SHA (nullable for initial commit)</summary>
    [JsonPropertyName("parent_sha")]
    [StringLength(40)]
    public string? ParentSha { get; set; }
}
