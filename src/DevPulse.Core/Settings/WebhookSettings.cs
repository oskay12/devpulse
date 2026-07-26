namespace DevPulse.Core.Settings;

/// <summary>
/// Inbound webhook verification settings.
/// </summary>
public class WebhookSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Webhooks";

    /// <summary>
    /// Shared secret GitHub signs payloads with (<c>X-Hub-Signature-256</c>).
    /// </summary>
    /// <remarks>
    /// One secret for the whole installation rather than one per repository:
    /// HMAC verification needs the secret in plaintext, and storing a recoverable
    /// per-repository secret would mean managing encryption keys. GitLab does not
    /// have this constraint — it sends the token itself, so those are per-repository
    /// and stored only as hashes.
    ///
    /// Left unset, GitHub webhooks are rejected rather than accepted unverified.
    /// </remarks>
    [JsonPropertyName("github_secret")]
    public string? GitHubSecret { get; set; }
}
