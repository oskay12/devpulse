using DevPulse.Core.Enums;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Fetches per-commit line-change statistics from the source provider's API.
/// </summary>
/// <remarks>
/// Push webhook payloads list only changed file paths, not line counts — providers
/// only expose those from their commit-detail endpoint. Implementations are
/// best-effort: a failure here must never block commit ingestion, so callers get
/// <see langword="null"/> rather than an exception on any error.
/// </remarks>
public interface ICommitStatsProvider
{
    /// <summary>Source provider this instance fetches stats from.</summary>
    RepositoryProvider Provider { get; }

    /// <summary>
    /// Fetches line-change stats for one commit.
    /// </summary>
    /// <param name="repositoryFullName">Provider-qualified path, e.g. "owner/repo".</param>
    /// <param name="sha">Full commit SHA.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stats if the call succeeded, otherwise <see langword="null"/>.</returns>
    Task<CommitStats?> GetCommitStatsAsync(
        string repositoryFullName,
        string sha,
        CancellationToken cancellationToken);
}

/// <summary>Line-change totals plus a per-file breakdown for one commit.</summary>
public sealed record CommitStats(
    int Additions,
    int Deletions,
    IReadOnlyList<CommitFileStats> Files);

/// <summary>Line-change counts for a single file within a commit.</summary>
public sealed record CommitFileStats(string Path, int Additions, int Deletions);
