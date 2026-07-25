namespace DevPulse.Core.Dtos;

/// <summary>
/// Top contributor summary for leaderboard display.
/// </summary>
public class TopContributorDto
{
    /// <summary>User UUID</summary>
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    /// <summary>Username</summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>Total commits</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }

    /// <summary>Total pull requests</summary>
    [JsonPropertyName("pull_request_count")]
    public int PullRequestCount { get; set; }
}
