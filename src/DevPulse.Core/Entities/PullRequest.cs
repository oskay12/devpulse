using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// Represents a pull request (or merge request in GitLab).
/// Tracks lifecycle, changes, and merge statistics.
/// </summary>
public class PullRequest
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Sequential PR number from provider (e.g., #123)</summary>
    [JsonPropertyName("pr_number")]
    [Required]
    public int PrNumber { get; set; }

    /// <summary>Pull request title</summary>
    [JsonPropertyName("title")]
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Full PR description (Markdown, nullable)</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Foreign key to User (PR author)</summary>
    [JsonPropertyName("author_id")]
    [Required]
    public Guid AuthorId { get; set; }

    /// <summary>Source/head branch name</summary>
    [JsonPropertyName("source_branch")]
    [Required]
    [StringLength(200)]
    public string SourceBranch { get; set; } = string.Empty;

    /// <summary>Target/base branch name</summary>
    [JsonPropertyName("target_branch")]
    [Required]
    [StringLength(200)]
    public string TargetBranch { get; set; } = string.Empty;

    /// <summary>Current PR state</summary>
    [JsonPropertyName("state")]
    public PullRequestState State { get; set; }

    /// <summary>PR creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last update timestamp (UTC, nullable)</summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Merge timestamp (UTC, nullable)</summary>
    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }

    /// <summary>Close timestamp (UTC, nullable)</summary>
    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }

    /// <summary>Foreign key to User who merged PR (nullable)</summary>
    [JsonPropertyName("merged_by_id")]
    public Guid? MergedById { get; set; }

    /// <summary>Number of commits in PR</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }

    /// <summary>Number of files changed</summary>
    [JsonPropertyName("files_changed")]
    public int FilesChanged { get; set; }

    /// <summary>Total lines added</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Total lines deleted</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Draft/WIP status flag</summary>
    [JsonPropertyName("is_draft")]
    public bool IsDraft { get; set; }
}
