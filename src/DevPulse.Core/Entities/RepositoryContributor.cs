using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// Many-to-many join table for repository contributors.
/// Tracks user access and contribution statistics.
/// </summary>
public class RepositoryContributor
{
    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Foreign key to User</summary>
    [JsonPropertyName("user_id")]
    [Required]
    public Guid UserId { get; set; }

    /// <summary>Access role in repository</summary>
    [JsonPropertyName("role")]
    public ContributorRole Role { get; set; }

    /// <summary>When user was added to repository (UTC)</summary>
    [JsonPropertyName("joined_at")]
    public DateTime JoinedAt { get; set; }

    /// <summary>Cached commit count for this user in this repo</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }
}
