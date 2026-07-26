using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// A review submitted against a pull request.
/// </summary>
public class ReviewDto
{
    /// <summary>Review UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Pull request UUID</summary>
    [JsonPropertyName("pull_request_id")]
    public Guid PullRequestId { get; set; }

    /// <summary>Reviewer user UUID</summary>
    [JsonPropertyName("reviewer_id")]
    public Guid ReviewerId { get; set; }

    /// <summary>Reviewer username, resolved for display</summary>
    [JsonPropertyName("reviewer_username")]
    public string? ReviewerUsername { get; set; }

    /// <summary>Review outcome</summary>
    [JsonPropertyName("state")]
    public ReviewState State { get; set; }

    /// <summary>Summary comment left with the review</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>Submission timestamp (UTC)</summary>
    [JsonPropertyName("submitted_at")]
    public DateTime SubmittedAt { get; set; }
}
