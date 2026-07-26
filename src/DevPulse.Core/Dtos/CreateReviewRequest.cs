using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Payload for submitting a review on a pull request.
/// </summary>
public class CreateReviewRequest
{
    /// <summary>Reviewer user UUID. Must reference an existing user.</summary>
    [JsonPropertyName("reviewer_id")]
    [Required]
    public Guid ReviewerId { get; set; }

    /// <summary>Review outcome</summary>
    [JsonPropertyName("state")]
    [EnumDataType(typeof(ReviewState))]
    public ReviewState State { get; set; }

    /// <summary>Summary comment left with the review</summary>
    [JsonPropertyName("comment")]
    [StringLength(5000)]
    public string? Comment { get; set; }
}
