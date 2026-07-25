namespace DevPulse.Core.Entities;

/// <summary>
/// Detected architectural patterns and component metrics.
/// Used for dependency analysis and architecture visualization.
/// </summary>
public class ArchitecturalPattern
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Pattern type (e.g., "MVC", "Microservices", "Layered")</summary>
    [JsonPropertyName("pattern_type")]
    [Required]
    [StringLength(50)]
    public string PatternType { get; set; } = string.Empty;

    /// <summary>Component/module name</summary>
    [JsonPropertyName("component_name")]
    [Required]
    [StringLength(200)]
    public string ComponentName { get; set; } = string.Empty;

    /// <summary>Primary file path for component</summary>
    [JsonPropertyName("file_path")]
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Total lines of code in component</summary>
    [JsonPropertyName("line_count")]
    public int LineCount { get; set; }

    /// <summary>Coupling score (0-100, lower is better)</summary>
    [JsonPropertyName("coupling_score")]
    [Range(0, 100)]
    public decimal CouplingScore { get; set; }

    /// <summary>Cohesion score (0-100, higher is better)</summary>
    [JsonPropertyName("cohesion_score")]
    [Range(0, 100)]
    public decimal CohesionScore { get; set; }

    /// <summary>Pattern detection timestamp (UTC)</summary>
    [JsonPropertyName("detected_at")]
    public DateTime DetectedAt { get; set; }
}
