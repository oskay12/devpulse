namespace DevPulse.Core.Enums;

/// <summary>
/// Code review approval states
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewState
{
    /// <summary>Review requested but not submitted</summary>
    [JsonPropertyName("pending")]
    Pending = 0,

    /// <summary>Changes approved</summary>
    [JsonPropertyName("approved")]
    Approved = 1,

    /// <summary>Changes must be made</summary>
    [JsonPropertyName("changes_requested")]
    ChangesRequested = 2,

    /// <summary>General feedback without approval</summary>
    [JsonPropertyName("commented")]
    Commented = 3,

    /// <summary>Review dismissed/invalidated</summary>
    [JsonPropertyName("dismissed")]
    Dismissed = 4
}
