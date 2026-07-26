using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Payload for issuing a webhook token for a repository.
/// </summary>
public class CreateProjectTokenRequest : IValidatableObject
{
    /// <summary>Human-readable label</summary>
    [JsonPropertyName("name")]
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Expiry timestamp (UTC). Omit for a non-expiring token.</summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Granted permissions</summary>
    [JsonPropertyName("permissions")]
    public TokenPermission Permissions { get; set; } = TokenPermission.WriteWebhooks;

    /// <summary>
    /// Rejects an expiry in the past — a token that is dead on arrival is almost
    /// always a client bug (e.g. a local timestamp sent as UTC) and silently
    /// accepting it produces confusing 401s later.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow)
        {
            yield return new ValidationResult(
                $"{nameof(ExpiresAt)} must be in the future.",
                [nameof(ExpiresAt)]);
        }

        if (Permissions == 0)
        {
            yield return new ValidationResult(
                $"{nameof(Permissions)} must grant at least one permission.",
                [nameof(Permissions)]);
        }
    }
}
