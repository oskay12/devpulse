namespace DevPulse.Core.Dtos;

/// <summary>
/// A code health snapshot for a repository.
/// </summary>
public class CodeHealthScoreDto
{
    /// <summary>Score UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Repository UUID</summary>
    [JsonPropertyName("repository_id")]
    public Guid RepositoryId { get; set; }

    /// <summary>When the score was calculated (UTC)</summary>
    [JsonPropertyName("calculated_at")]
    public DateTime CalculatedAt { get; set; }

    /// <summary>Composite health score</summary>
    [JsonPropertyName("overall_score")]
    public decimal OverallScore { get; set; }

    /// <summary>Maintainability sub-score</summary>
    [JsonPropertyName("maintainability_score")]
    public decimal MaintainabilityScore { get; set; }

    /// <summary>Test coverage sub-score</summary>
    [JsonPropertyName("test_coverage_score")]
    public decimal TestCoverageScore { get; set; }

    /// <summary>Documentation sub-score</summary>
    [JsonPropertyName("documentation_score")]
    public decimal DocumentationScore { get; set; }

    /// <summary>Estimated remediation effort in minutes</summary>
    [JsonPropertyName("technical_debt_minutes")]
    public int TechnicalDebtMinutes { get; set; }

    /// <summary>Number of detected code smells</summary>
    [JsonPropertyName("code_smell_count")]
    public int CodeSmellCount { get; set; }

    /// <summary>Percentage of duplicated code</summary>
    [JsonPropertyName("duplication_percentage")]
    public int DuplicationPercentage { get; set; }

    /// <summary>Aggregate cyclomatic complexity score</summary>
    [JsonPropertyName("complexity_score")]
    public int ComplexityScore { get; set; }
}
