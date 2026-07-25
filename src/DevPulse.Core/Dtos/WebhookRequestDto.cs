namespace DevPulse.Core.Dtos;

/// <summary>
/// Generic DTO for receiving webhook requests from Git providers.
/// Validates signature and deserializes provider-specific payloads.
/// </summary>
public class WebhookRequestDto
{
    /// <summary>Event type header (e.g., "push", "pull_request")</summary>
    [JsonPropertyName("event")]
    [Required]
    public string Event { get; set; } = string.Empty;

    /// <summary>HMAC signature for validation</summary>
    [JsonPropertyName("signature")]
    [Required]
    public string Signature { get; set; } = string.Empty;

    /// <summary>Raw JSON payload (provider-specific schema)</summary>
    [JsonPropertyName("payload")]
    [Required]
    public object Payload { get; set; } = new();
}
