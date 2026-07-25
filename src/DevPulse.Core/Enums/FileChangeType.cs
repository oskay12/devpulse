namespace DevPulse.Core.Enums;

/// <summary>
/// Git file change operation types
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileChangeType
{
    /// <summary>New file created</summary>
    [JsonPropertyName("added")]
    Added = 0,

    /// <summary>Existing file modified</summary>
    [JsonPropertyName("modified")]
    Modified = 1,

    /// <summary>File removed</summary>
    [JsonPropertyName("deleted")]
    Deleted = 2,

    /// <summary>File moved/renamed</summary>
    [JsonPropertyName("renamed")]
    Renamed = 3,

    /// <summary>File copied to new location</summary>
    [JsonPropertyName("copied")]
    Copied = 4
}
