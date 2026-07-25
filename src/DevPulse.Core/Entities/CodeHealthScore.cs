namespace DevPulse.Core.Entities;

/// <summary>
/// Repository code health score calculated by static analysis.
/// Inspired by SonarQube quality gates.
/// </summary>
public class CodeHealthScore
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Repository</summary>
    [JsonPropertyName("repository_id")]
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>Calculation timestamp (UTC)</summary>
    [JsonPropertyName("calculated_at")]
    public DateTime CalculatedAt { get; set; }

    /// <summary>Composite score (0-100, higher is better)</summary>
    [JsonPropertyName("overall_score")]
    [Range(0, 100)]
    public decimal OverallScore { get; set; }

    /// <summary>Maintainability index (0-100)</summary>
    [JsonPropertyName("maintainability_score")]
    [Range(0, 100)]
    public decimal MaintainabilityScore { get; set; }

    /// <summary>Test coverage percentage (0-100)</summary>
    [JsonPropertyName("test_coverage_score")]
    [Range(0, 100)]
    public decimal TestCoverageScore { get; set; }

    /// <summary>Documentation completeness (0-100)</summary>
    [JsonPropertyName("documentation_score")]
    [Range(0, 100)]
    public decimal DocumentationScore { get; set; }

    /// <summary>Estimated technical debt in minutes</summary>
    [JsonPropertyName("technical_debt_minutes")]
    public int TechnicalDebtMinutes { get; set; }

    /// <summary>Number of code smells detected</summary>
    [JsonPropertyName("code_smell_count")]
    public int CodeSmellCount { get; set; }

    /// <summary>Code duplication percentage</summary>
    [JsonPropertyName("duplication_percentage")]
    [Range(0, 100)]
    public int DuplicationPercentage { get; set; }

    /// <summary>Cyclomatic complexity score</summary>
    [JsonPropertyName("complexity_score")]
    public int ComplexityScore { get; set; }
}
