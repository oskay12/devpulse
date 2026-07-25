using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// Represents a high-level code review submission.
/// Can contain multiple inline comments.
/// </summary>
public class PullRequestReview
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to PullRequest</summary>
    [JsonPropertyName("pull_request_id")]
    [Required]
    public Guid PullRequestId { get; set; }

    /// <summary>Foreign key to User (reviewer)</summary>
    [JsonPropertyName("reviewer_id")]
    [Required]
    public Guid ReviewerId { get; set; }

    /// <summary>Review decision/state</summary>
    [JsonPropertyName("state")]
    public ReviewState State { get; set; }

    /// <summary>Overall review comment (nullable)</summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>Review submission timestamp (UTC)</summary>
    [JsonPropertyName("submitted_at")]
    public DateTime SubmittedAt { get; set; }
}
