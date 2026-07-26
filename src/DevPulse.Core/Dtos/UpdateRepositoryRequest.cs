namespace DevPulse.Core.Dtos;

/// <summary>
/// Payload for updating a repository's mutable metadata.
/// </summary>
/// <remarks>
/// <c>FullName</c>, <c>Provider</c> and <c>ExternalId</c> are intentionally absent.
/// They form the repository's identity — both are backed by unique indexes and are
/// what webhooks resolve against — so changing them would silently orphan existing
/// commits and pull requests. Re-register the repository instead.
/// </remarks>
public class UpdateRepositoryRequest
{
    /// <summary>Repository name</summary>
    [JsonPropertyName("name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

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
    public string DefaultBranch { get; set; } = string.Empty;

    /// <summary>Repository visibility flag</summary>
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>Repository monitoring status</summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }

    /// <summary>Star count (cached from the provider)</summary>
    [JsonPropertyName("star_count")]
    [Range(0, int.MaxValue)]
    public int StarCount { get; set; }

    /// <summary>Fork count (cached from the provider)</summary>
    [JsonPropertyName("fork_count")]
    [Range(0, int.MaxValue)]
    public int ForkCount { get; set; }
}
