namespace DevPulse.Core.Dtos;

/// <summary>
/// Response returned when a webhook token is issued.
/// </summary>
/// <remarks>
/// <see cref="Token"/> is shown exactly once. Only a hash is persisted, so the
/// plaintext value cannot be recovered afterwards — the caller must store it now.
/// </remarks>
public class CreateProjectTokenResponse
{
    /// <summary>Token metadata</summary>
    [JsonPropertyName("token_info")]
    public ProjectTokenDto TokenInfo { get; set; } = new();

    /// <summary>
    /// The plaintext token. Use it as the webhook secret when configuring the
    /// provider. Not retrievable later.
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
