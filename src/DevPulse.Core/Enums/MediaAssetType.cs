namespace DevPulse.Core.Enums;

/// <summary>
/// Media asset classification
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaAssetType
{
    /// <summary>System architecture diagram (SVG, PNG, etc.)</summary>
    [JsonPropertyName("architecture_diagram")]
    ArchitectureDiagram = 0,

    /// <summary>Test result screenshot</summary>
    [JsonPropertyName("screenshot")]
    Screenshot = 1,

    /// <summary>Analytics chart/graph</summary>
    [JsonPropertyName("chart")]
    Chart = 2,

    /// <summary>Generated PDF report</summary>
    [JsonPropertyName("report")]
    Report = 3,

    /// <summary>Uncategorized media</summary>
    [JsonPropertyName("other")]
    Other = 4
}
