using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Metadata about a webhook token. The token value itself is never returned here —
/// only <see cref="CreateProjectTokenResponse"/> exposes it, once, at creation.
/// </summary>
public class ProjectTokenDto
{
    /// <summary>Token UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Repository the token authorises</summary>
    [JsonPropertyName("repository_id")]
    public Guid RepositoryId { get; set; }

    /// <summary>Human-readable label</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Expiry timestamp (UTC), if the token expires</summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Last time the token authenticated a request (UTC)</summary>
    [JsonPropertyName("last_used_at")]
    public DateTime? LastUsedAt { get; set; }

    /// <summary>Whether the token has been revoked</summary>
    [JsonPropertyName("is_revoked")]
    public bool IsRevoked { get; set; }

    /// <summary>Granted permissions</summary>
    [JsonPropertyName("permissions")]
    public TokenPermission Permissions { get; set; }
}
