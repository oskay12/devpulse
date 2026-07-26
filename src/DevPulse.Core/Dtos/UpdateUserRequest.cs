using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Payload for updating a user's profile.
/// </summary>
/// <remarks>
/// <c>Username</c> is immutable: it is the handle webhooks and metrics attribute
/// activity to, and renaming it would detach historical records. Password changes
/// belong to a dedicated endpoint, not a general profile update.
/// </remarks>
public class UpdateUserRequest
{
    /// <summary>Email address. Must be unique.</summary>
    [JsonPropertyName("email")]
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Avatar image URL</summary>
    [JsonPropertyName("avatar_url")]
    [Url]
    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>Authorization role</summary>
    [JsonPropertyName("role")]
    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; }

    /// <summary>Account active status</summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}
