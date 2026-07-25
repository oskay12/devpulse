using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// API access token for repository webhook integrations.
/// Used for authentication of GitHub/GitLab webhook requests.
/// </summary>
public class ProjectToken
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>SHA-256 hashed token value</summary>
    [JsonPropertyName("token_hash")]
    [Required]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Human-readable token name/description</summary>
    [JsonPropertyName("name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Token creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Token expiration timestamp (UTC, nullable for no expiry)</summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Last usage timestamp for audit trail (UTC, nullable)</summary>
    [JsonPropertyName("last_used_at")]
    public DateTime? LastUsedAt { get; set; }

    /// <summary>Manual revocation flag</summary>
    [JsonPropertyName("is_revoked")]
    public bool IsRevoked { get; set; }

    /// <summary>Bitwise permission flags</summary>
    [JsonPropertyName("permissions")]
    public TokenPermission Permissions { get; set; }
}
