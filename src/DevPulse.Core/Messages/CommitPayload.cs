namespace DevPulse.Core.Messages;

/// <summary>
/// Individual commit data from webhook payload.
/// Mapped to Commit entity during processing.
/// </summary>
public class CommitPayload
{
    /// <summary>Git commit SHA-1 hash</summary>
    [JsonPropertyName("sha")]
    [Required]
    public string Sha { get; set; } = string.Empty;

    /// <summary>Commit message</summary>
    [JsonPropertyName("message")]
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>Author name from Git</summary>
    [JsonPropertyName("author_name")]
    [Required]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Author email from Git</summary>
    [JsonPropertyName("author_email")]
    [Required]
    public string AuthorEmail { get; set; } = string.Empty;

    /// <summary>Commit timestamp</summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>Newly created files</summary>
    [JsonPropertyName("added_files")]
    public List<string> AddedFiles { get; set; } = new();

    /// <summary>Modified existing files</summary>
    [JsonPropertyName("modified_files")]
    public List<string> ModifiedFiles { get; set; } = new();

    /// <summary>Deleted files</summary>
    [JsonPropertyName("removed_files")]
    public List<string> RemovedFiles { get; set; } = new();
}
