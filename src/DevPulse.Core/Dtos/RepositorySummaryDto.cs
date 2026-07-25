using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Repository summary for user profile.
/// </summary>
public class RepositorySummaryDto
{
    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository name</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>User's role in repository</summary>
    [JsonPropertyName("role")]
    public ContributorRole Role { get; set; }

    /// <summary>User's commit count in this repo</summary>
    [JsonPropertyName("commit_count")]
    public int CommitCount { get; set; }
}
