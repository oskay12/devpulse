using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Full repository representation returned by the repositories endpoints.
/// </summary>
public class RepositoryDetailDto
{
    /// <summary>Repository UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Repository name (e.g., "devpulse")</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Full repository path (e.g., "organization/repo-name")</summary>
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Repository description</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Git clone URL (HTTPS)</summary>
    [JsonPropertyName("clone_url")]
    public string CloneUrl { get; set; } = string.Empty;

    /// <summary>Default branch name</summary>
    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; } = string.Empty;

    /// <summary>Source provider</summary>
    [JsonPropertyName("provider")]
    public RepositoryProvider Provider { get; set; }

    /// <summary>External repository ID from the provider API</summary>
    [JsonPropertyName("external_id")]
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Owner user UUID</summary>
    [JsonPropertyName("owner_id")]
    public Guid OwnerId { get; set; }

    /// <summary>Owner username, resolved for display</summary>
    [JsonPropertyName("owner_username")]
    public string? OwnerUsername { get; set; }

    /// <summary>Repository creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last webhook sync timestamp (UTC)</summary>
    [JsonPropertyName("last_synced_at")]
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>Repository visibility flag</summary>
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>Repository monitoring status</summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    /// <summary>Star count (cached from the provider)</summary>
    [JsonPropertyName("star_count")]
    public int StarCount { get; set; }

    /// <summary>Fork count (cached from the provider)</summary>
    [JsonPropertyName("fork_count")]
    public int ForkCount { get; set; }
}
