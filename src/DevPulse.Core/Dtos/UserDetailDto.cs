using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// User representation returned by the users endpoints.
/// </summary>
/// <remarks>
/// <c>PasswordHash</c> is deliberately not exposed.
/// </remarks>
public class UserDetailDto
{
    /// <summary>User UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Unique username</summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>Email address</summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Avatar image URL</summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>Account creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last successful login timestamp (UTC)</summary>
    [JsonPropertyName("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Authorization role</summary>
    [JsonPropertyName("role")]
    public UserRole Role { get; set; }

    /// <summary>Account active status</summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}
