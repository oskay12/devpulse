namespace DevPulse.Core.Enums;

/// <summary>
/// Time period types for metric aggregation
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MetricPeriodType
{
    /// <summary>24-hour period</summary>
    [JsonPropertyName("daily")]
    Daily = 0,

    /// <summary>7-day period</summary>
    [JsonPropertyName("weekly")]
    Weekly = 1,

    /// <summary>30-day period</summary>
    [JsonPropertyName("monthly")]
    Monthly = 2,

    /// <summary>90-day period</summary>
    [JsonPropertyName("quarterly")]
    Quarterly = 3,

    /// <summary>365-day period</summary>
    [JsonPropertyName("yearly")]
    Yearly = 4
}
