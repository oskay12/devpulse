using DevPulse.Core.Enums;

namespace DevPulse.Core.Dtos;

/// <summary>
/// Per-file change record within a commit.
/// </summary>
public class CommitFileDto
{
    /// <summary>Commit file UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Owning commit UUID</summary>
    [JsonPropertyName("commit_id")]
    public Guid CommitId { get; set; }

    /// <summary>Path of the changed file</summary>
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>How the file changed</summary>
    [JsonPropertyName("change_type")]
    public FileChangeType ChangeType { get; set; }

    /// <summary>Lines added</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Lines deleted</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Truncated diff snippet, when captured</summary>
    [JsonPropertyName("diff_snippet")]
    public string? DiffSnippet { get; set; }
}
