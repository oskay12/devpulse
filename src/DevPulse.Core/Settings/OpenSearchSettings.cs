namespace DevPulse.Core.Settings;

/// <summary>
/// OpenSearch cluster configuration.
/// </summary>
public class OpenSearchSettings
{
    /// <summary>OpenSearch cluster endpoint URL</summary>
    [JsonPropertyName("endpoint")]
    [Required]
    [Url]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>Authentication username</summary>
    [JsonPropertyName("username")]
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>Authentication password</summary>
    [JsonPropertyName("password")]
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>Commits index name</summary>
    [JsonPropertyName("commits_index")]
    public string CommitsIndex { get; set; } = "devpulse-commits";

    /// <summary>Pull requests index name</summary>
    [JsonPropertyName("pull_requests_index")]
    public string PullRequestsIndex { get; set; } = "devpulse-pull-requests";

    /// <summary>Reviews index name</summary>
    [JsonPropertyName("reviews_index")]
    public string ReviewsIndex { get; set; } = "devpulse-reviews";
}
