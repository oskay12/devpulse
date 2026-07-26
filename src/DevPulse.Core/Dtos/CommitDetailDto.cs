namespace DevPulse.Core.Dtos;

/// <summary>
/// Commit representation returned by the commits endpoints.
/// </summary>
public class CommitDetailDto
{
    /// <summary>Commit UUID</summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>Owning repository UUID</summary>
    [JsonPropertyName("repository_id")]
    public Guid RepositoryId { get; set; }

    /// <summary>Repository full name, resolved for display</summary>
    [JsonPropertyName("repository_name")]
    public string? RepositoryName { get; set; }

    /// <summary>Commit SHA-1</summary>
    [JsonPropertyName("sha")]
    public string Sha { get; set; } = string.Empty;

    /// <summary>Matched DevPulse user UUID, when the commit author is known</summary>
    [JsonPropertyName("author_id")]
    public Guid? AuthorId { get; set; }

    /// <summary>Commit author name as recorded in Git</summary>
    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Commit author email as recorded in Git</summary>
    [JsonPropertyName("author_email")]
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>Full commit message</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Branch the commit was observed on</summary>
    [JsonPropertyName("branch")]
    public string Branch { get; set; } = string.Empty;

    /// <summary>Commit timestamp (UTC)</summary>
    [JsonPropertyName("committed_at")]
    public DateTime CommittedAt { get; set; }

    /// <summary>Timestamp this commit was ingested (UTC)</summary>
    [JsonPropertyName("indexed_at")]
    public DateTime IndexedAt { get; set; }

    /// <summary>Number of files changed</summary>
    [JsonPropertyName("files_changed")]
    public int FilesChanged { get; set; }

    /// <summary>Lines added</summary>
    [JsonPropertyName("additions")]
    public int Additions { get; set; }

    /// <summary>Lines deleted</summary>
    [JsonPropertyName("deletions")]
    public int Deletions { get; set; }

    /// <summary>Parent commit SHA</summary>
    [JsonPropertyName("parent_sha")]
    public string? ParentSha { get; set; }
}
