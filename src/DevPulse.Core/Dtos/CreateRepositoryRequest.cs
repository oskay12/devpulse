using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Payload for registering a repository.
/// </summary>
/// <remarks>
/// Length limits mirror the entity constraints so oversized input is rejected as
/// a 400 by model binding instead of failing at the database as a 500.
/// </remarks>
public class CreateRepositoryRequest
{
    /// <summary>Repository name (e.g., "devpulse")</summary>
    [JsonPropertyName("name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Full repository path (e.g., "organization/repo-name"). Must be unique.</summary>
    [JsonPropertyName("full_name")]
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Repository description</summary>
    [JsonPropertyName("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>Git clone URL (HTTPS)</summary>
    [JsonPropertyName("clone_url")]
    [Required]
    [Url]
    public string CloneUrl { get; set; } = string.Empty;

    /// <summary>Default branch name</summary>
    [JsonPropertyName("default_branch")]
    [Required]
    [StringLength(100)]
    public string DefaultBranch { get; set; } = "main";

    /// <summary>Source provider</summary>
    [JsonPropertyName("provider")]
    [EnumDataType(typeof(RepositoryProvider))]
    public RepositoryProvider Provider { get; set; }

    /// <summary>
    /// External repository ID from the provider API. Unique together with
    /// <see cref="Provider"/>.
    /// </summary>
    [JsonPropertyName("external_id")]
    [Required]
    [StringLength(100)]
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Owner user UUID. Must reference an existing user.</summary>
    [JsonPropertyName("owner_id")]
    [Required]
    public Guid OwnerId { get; set; }

    /// <summary>Repository visibility flag</summary>
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>Star count (cached from the provider)</summary>
    [JsonPropertyName("star_count")]
    [Range(0, int.MaxValue)]
    public int StarCount { get; set; }

    /// <summary>Fork count (cached from the provider)</summary>
    [JsonPropertyName("fork_count")]
    [Range(0, int.MaxValue)]
    public int ForkCount { get; set; }
}
