namespace DevPulse.Core.Enums;

/// <summary>
/// Supported Git hosting providers
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RepositoryProvider
{
    /// <summary>GitHub.com or GitHub Enterprise</summary>
    [JsonPropertyName("github")]
    GitHub = 0,

    /// <summary>GitLab.com or self-hosted GitLab</summary>
    [JsonPropertyName("gitlab")]
    GitLab = 1,

    /// <summary>Bitbucket Cloud or Server</summary>
    [JsonPropertyName("bitbucket")]
    Bitbucket = 2
}
