using DevPulse.Core.Dtos;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Read-only full-text search over the OpenSearch indices. Used by the API.
/// </summary>
public interface ISearchService
{
    /// <summary>Searches commit messages, file paths and diff snippets.</summary>
    /// <exception cref="Exceptions.DependencyUnavailableException">OpenSearch unreachable.</exception>
    Task<SearchResultDto<CommitSearchResultDto>> SearchCommitsAsync(
        string query,
        PagedQuery paging,
        Guid? repositoryId = null,
        string? author = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>Searches pull request titles, descriptions and review comments.</summary>
    /// <exception cref="Exceptions.DependencyUnavailableException">OpenSearch unreachable.</exception>
    Task<SearchResultDto<PullRequestSearchResultDto>> SearchPullRequestsAsync(
        string query,
        PagedQuery paging,
        Guid? repositoryId = null,
        string? state = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches review comment bodies.
    /// </summary>
    /// <remarks>
    /// Filtered by repository <em>name</em> rather than id: the review document
    /// shape only carries <c>repository_name</c>.
    /// </remarks>
    /// <exception cref="Exceptions.DependencyUnavailableException">OpenSearch unreachable.</exception>
    Task<SearchResultDto<ReviewSearchResultDto>> SearchReviewsAsync(
        string query,
        PagedQuery paging,
        string? repositoryName = null,
        string? author = null,
        CancellationToken cancellationToken = default);
}
