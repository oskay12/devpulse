namespace DevPulse.Core.Enums;

/// <summary>
/// Pull request lifecycle states
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PullRequestState
{
    /// <summary>Currently open and awaiting review</summary>
    [JsonPropertyName("open")]
    Open = 0,

    /// <summary>Closed without merging</summary>
    [JsonPropertyName("closed")]
    Closed = 1,

    /// <summary>Successfully merged</summary>
    [JsonPropertyName("merged")]
    Merged = 2,

    /// <summary>Work in progress (draft)</summary>
    [JsonPropertyName("draft")]
    Draft = 3
}
