using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Pull request representation returned by the pull request endpoints.
/// </summary>
public class PullRequestDetailDto
{
    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Owning repository UUID</summary>
    [JsonPropertyName("repository_id")]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository full name, resolved for display</summary>
    [JsonPropertyName("repository_name")]
    public string? RepositoryName { get; set; }

    /// <summary>Provider-assigned PR number</summary>
    [JsonPropertyName("pr_number")]
    public int PrNumber { get; set; }

    /// <summary>PR title</summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>PR description</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Author user UUID</summary>
    [JsonPropertyName("author_id")]
    public Guid AuthorId { get; set; }

    /// <summary>Author username, resolved for display</summary>
    [JsonPropertyName("author_username")]
    public string? AuthorUsername { get; set; }

    /// <summary>Source branch</summary>
    [JsonPropertyName("source_branch")]
    public string SourceBranch { get; set; } = string.Empty;

    /// <summary>Target branch</summary>
    [JsonPropertyName("target_branch")]
    public string TargetBranch { get; set; } = string.Empty;

    /// <summary>Current PR state</summary>
    [JsonPropertyName("state")]
    public PullRequestState State { get; set; }

    /// <summary>Creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last update timestamp (UTC)</summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Merge timestamp (UTC)</summary>
    [JsonPropertyName("merged_at")]
    public DateTime? MergedAt { get; set; }

    /// <summary>Close timestamp (UTC)</summary>
    [JsonPropertyName("closed_at")]
    public DateTime? ClosedAt { get; set; }

    /// <summary>User who merged the PR</summary>
    [JsonPropertyName("merged_by_id")]
    public Guid? MergedById { get; set; }

    /// <summary>Number of commits in the PR</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }

    /// <summary>Number of files changed</summary>
    [JsonPropertyName("files_changed")]
    public int FilesChanged { get; set; }

    /// <summary>Lines added</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Lines deleted</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Whether the PR is a draft</summary>
    [JsonPropertyName("is_draft")]
    public bool IsDraft { get; set; }
}
