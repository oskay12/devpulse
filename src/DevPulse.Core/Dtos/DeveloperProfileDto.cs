namespace DevPulse.Core.Dtos;

/// <summary>
/// Developer profile response DTO.
/// Contains user info, metrics, and repository list.
/// </summary>
public class DeveloperProfileDto
{
    /// <summary>User UUID</summary>
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    /// <summary>Username</summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>Email address</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Avatar URL (nullable)</summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>Aggregated developer metrics</summary>
    [JsonPropertyName("metrics")]
    public DeveloperMetricsDto Metrics { get; set; } = new();

    /// <summary>List of repositories user contributes to</summary>
    [JsonPropertyName("repositories")]
    public List<RepositorySummaryDto> Repositories { get; set; } = new();
}
