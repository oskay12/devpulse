using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// Represents a Git repository registered in DevPulse.
/// Tracks metadata from GitHub/GitLab and sync status.
/// </summary>
public class Repository
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Repository name (e.g., "devpulse")</summary>
    [JsonPropertyName("name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Full repository path (e.g., "organization/repo-name")</summary>
    [JsonPropertyName("full_name")]
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Repository description (nullable)</summary>
    [JsonPropertyName("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Git clone URL (HTTPS)</summary>
    [JsonPropertyName("clone_url")]
    [Required]
    [Url]
    public string CloneUrl { get; set; } = string.Empty;

    /// <summary>Default branch name (e.g., "main", "master")</summary>
    [JsonPropertyName("default_branch")]
    [Required]
    [StringLength(100)]
    public string DefaultBranch { get; set; } = string.Empty;

    /// <summary>Source provider (GitHub, GitLab, etc.)</summary>
    [JsonPropertyName("provider")]
    public RepositoryProvider Provider { get; set; }

    /// <summary>External repository ID from provider API</summary>
    [JsonPropertyName("external_id")]
    [Required]
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Foreign key to User (repository owner)</summary>
    [JsonPropertyName("owner_id")]
    [Required]
    public Guid OwnerId { get; set; }

    /// <summary>Repository creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last webhook sync timestamp (UTC, nullable)</summary>
    [JsonPropertyName("last_synced_at")]
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>Repository visibility flag</summary>
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>Repository monitoring status</summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    /// <summary>GitHub/GitLab star count (cached)</summary>
    [JsonPropertyName("star_count")]
    public int StarCount { get; set; }

    /// <summary>Fork count (cached)</summary>
    [JsonPropertyName("fork_count")]
    public int ForkCount { get; set; }
}
