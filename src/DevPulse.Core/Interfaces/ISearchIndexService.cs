using DevPulse.Core.Enums;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Write side of the search integration: projects relational rows into OpenSearch
/// documents. Used by the Worker, never by the API.
/// </summary>
public interface ISearchIndexService
{
    /// <summary>
    /// Creates the commit, pull request and review indices with explicit mappings
    /// if they do not exist. Idempotent and safe to call on every startup.
    /// </summary>
    Task EnsureIndicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes one entity. Document ids are derived from the entity, so a message
    /// delivered twice overwrites rather than duplicating.
    /// </summary>
    /// <param name="contentType">Which index the entity belongs to.</param>
    /// <param name="entityId">Commit, pull request or review comment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// <see langword="false"/> if the entity no longer exists — it was deleted
    /// between enqueue and consumption, which is not an error worth retrying.
    /// </returns>
    Task<bool> IndexAsync(
        IndexContentType contentType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queues an indexing job for every matching entity, rebuilding an index without
    /// blocking the caller.
    /// </summary>
    /// <param name="contentType">Which entity type to reindex.</param>
    /// <param name="repositoryId">Restrict to one repository, or all when omitted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of jobs queued.</returns>
    Task<int> EnqueueReindexAsync(
        IndexContentType contentType,
        Guid? repositoryId = null,
        CancellationToken cancellationToken = default);
}
