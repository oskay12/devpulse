namespace DevPulse.Core.Dtos;

/// <summary>
/// Time-series data point for charts.
/// </summary>
public class MetricDataPointDto
{
    /// <summary>Data point timestamp (UTC)</summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    /// <summary>Metric value</summary>
    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}
