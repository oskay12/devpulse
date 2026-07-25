using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// Represents a user account in the DevPulse platform.
/// Stores authentication credentials and profile information.
/// </summary>
public class User
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Unique username for login</summary>
    [JsonPropertyName("username")]
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>User email address (unique)</summary>
    [JsonPropertyName("email")]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt hashed password</summary>
    [JsonPropertyName("password_hash")]
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>S3 URL to user avatar image (nullable)</summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>Account creation timestamp (UTC)</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Last successful login timestamp (UTC, nullable)</summary>
    [JsonPropertyName("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>User role for authorization</summary>
    [JsonPropertyName("role")]
    public UserRole Role { get; set; }

    /// <summary>Account active status (soft delete flag)</summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}
