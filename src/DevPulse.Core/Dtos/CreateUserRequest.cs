using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Payload for creating a user.
/// </summary>
public class CreateUserRequest
{
    /// <summary>Unique username</summary>
    [JsonPropertyName("username")]
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>Email address. Must be unique.</summary>
    [JsonPropertyName("email")]
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Plaintext password. Hashed with BCrypt before storage and never persisted,
    /// logged or returned as given.
    /// </summary>
    [JsonPropertyName("password")]
    [Required]
    [StringLength(128, MinimumLength = 12)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Avatar image URL</summary>
    [JsonPropertyName("avatar_url")]
    [Url]
    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    /// <summary>Authorization role</summary>
    [JsonPropertyName("role")]
    [EnumDataType(typeof(UserRole))]
    public UserRole Role { get; set; }
}
