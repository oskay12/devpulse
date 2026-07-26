namespace DevPulse.Core.Dtos;

/// <summary>
/// Payload for adding a comment to a pull request.
/// </summary>
public class CreateReviewCommentRequest : IValidatableObject
{
    /// <summary>Comment author user UUID. Must reference an existing user.</summary>
    [JsonPropertyName("author_id")]
    [Required]
    public Guid AuthorId { get; set; }

    /// <summary>Parent review UUID, when the comment belongs to a formal review</summary>
    [JsonPropertyName("review_id")]
    public Guid? ReviewId { get; set; }

    /// <summary>Comment body</summary>
    [JsonPropertyName("body")]
    [Required]
    [StringLength(5000, MinimumLength = 1)]
    public string Body { get; set; } = string.Empty;

    /// <summary>File path, for an inline comment</summary>
    [JsonPropertyName("file_path")]
    [StringLength(500)]
    public string? FilePath { get; set; }

    /// <summary>Line number, for an inline comment</summary>
    [JsonPropertyName("line_number")]
    [Range(1, int.MaxValue)]
    public int? LineNumber { get; set; }

    /// <summary>
    /// An inline comment needs both a path and a line to anchor to; a line number
    /// without a file path has nowhere to attach and would render as a general
    /// comment with a misleading position.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (LineNumber.HasValue && string.IsNullOrWhiteSpace(FilePath))
        {
            yield return new ValidationResult(
                $"{nameof(FilePath)} is required when {nameof(LineNumber)} is supplied.",
                [nameof(FilePath), nameof(LineNumber)]);
        }
    }
}
