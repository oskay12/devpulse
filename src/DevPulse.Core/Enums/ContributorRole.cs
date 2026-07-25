namespace DevPulse.Core.Enums;

/// <summary>
/// Repository access roles
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContributorRole
{
    /// <summary>Read-only access</summary>
    [JsonPropertyName("viewer")]
    Viewer = 0,

    /// <summary>Can commit and create PRs</summary>
    [JsonPropertyName("contributor")]
    Contributor = 1,

    /// <summary>Can merge PRs and manage settings</summary>
    [JsonPropertyName("maintainer")]
    Maintainer = 2,

    /// <summary>Full administrative access</summary>
    [JsonPropertyName("owner")]
    Owner = 3
}
