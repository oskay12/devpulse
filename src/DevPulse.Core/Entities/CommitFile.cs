using DevPulse.Core.Enums;

namespace DevPulse.Core.Entities;

/// <summary>
/// Represents a single file changed in a commit.
/// Stores diff statistics for code churn analysis.
/// </summary>
public class CommitFile
{
    /// <summary>Primary key - UUID v4</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Foreign key to Commit</summary>
    [JsonPropertyName("commit_id")]
    [Required]
    public Guid CommitId { get; set; }

    /// <summary>Relative file path in repository</summary>
    [JsonPropertyName("file_path")]
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Type of file change</summary>
    [JsonPropertyName("change_type")]
    public FileChangeType ChangeType { get; set; }

    /// <summary>Lines added in this file</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Lines deleted in this file</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Truncated diff snippet for OpenSearch indexing (nullable)</summary>
    [JsonPropertyName("diff_snippet")]
    [StringLength(5000)]
    public string? DiffSnippet { get; set; }
}
