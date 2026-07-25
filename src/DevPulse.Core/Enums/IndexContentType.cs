namespace DevPulse.Core.Enums;

/// <summary>
/// Content types for OpenSearch indexing
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndexContentType
{
    /// <summary>Index a commit document</summary>
    [JsonPropertyName("commit")]
    Commit = 0,

    /// <summary>Index a pull request document</summary>
    [JsonPropertyName("pull_request")]
    PullRequest = 1,

    /// <summary>Index a review comment document</summary>
    [JsonPropertyName("review_comment")]
    ReviewComment = 2
}
