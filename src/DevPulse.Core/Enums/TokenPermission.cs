namespace DevPulse.Core.Enums;

/// <summary>
/// Bitwise flags for granular token permissions.
/// Allows combining multiple permissions using | operator.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TokenPermission
{
    /// <summary>Read metrics and analytics data</summary>
    [JsonPropertyName("read_metrics")]
    ReadMetrics = 1,

    /// <summary>Receive and process webhook events</summary>
    [JsonPropertyName("write_webhooks")]
    WriteWebhooks = 2,

    /// <summary>Read repository metadata</summary>
    [JsonPropertyName("read_repository")]
    ReadRepository = 4,

    /// <summary>Modify repository settings</summary>
    [JsonPropertyName("write_repository")]
    WriteRepository = 8
}
