using System.ComponentModel.DataAnnotations;
using DevPulse.Core.Dtos;
using DevPulse.Core.Enums;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

/// <summary>
/// Full-text search across commits, pull requests and review discussions.
/// </summary>
/// <remarks>
/// Backed by Amazon OpenSearch. Documents are written asynchronously by the Worker,
/// so a freshly ingested commit becomes searchable a moment after it is stored.
/// </remarks>
[ApiController]
[Route("api/search")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ISearchIndexService _searchIndexService;

    /// <summary>Initialises the controller.</summary>
    public SearchController(ISearchService searchService, ISearchIndexService searchIndexService)
    {
        _searchService = searchService;
        _searchIndexService = searchIndexService;
    }

    /// <summary>Searches commit messages, changed file paths and diff snippets.</summary>
    /// <param name="q">Search terms. Required.</param>
    /// <param name="paging">Pagination parameters.</param>
    /// <param name="repositoryId">Restrict to a repository.</param>
    /// <param name="author">Restrict to a commit author name (exact match).</param>
    /// <param name="from">Only commits at or after this instant (UTC).</param>
    /// <param name="to">Only commits at or before this instant (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching commits with highlight snippets.</returns>
    [HttpGet("commits", Name = "SearchCommits")]
    [ProducesResponseType<SearchResultDto<CommitSearchResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResultDto<CommitSearchResultDto>>> SearchCommits(
        [FromQuery] [Required] string q,
        [FromQuery] PagedQuery paging,
        [FromQuery] Guid? repositoryId,
        [FromQuery] string? author,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        EnsureQueryPresent(q);

        return Ok(await _searchService.SearchCommitsAsync(
            q, paging, repositoryId, author, from, to, cancellationToken));
    }

    /// <summary>Searches pull request titles, descriptions and review comments.</summary>
    /// <param name="q">Search terms. Required.</param>
    /// <param name="paging">Pagination parameters.</param>
    /// <param name="repositoryId">Restrict to a repository.</param>
    /// <param name="state">Restrict to a pull request state, e.g. <c>Merged</c>.</param>
    /// <param name="from">Only pull requests created at or after this instant (UTC).</param>
    /// <param name="to">Only pull requests created at or before this instant (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching pull requests with highlight snippets.</returns>
    [HttpGet("pull-requests", Name = "SearchPullRequests")]
    [ProducesResponseType<SearchResultDto<PullRequestSearchResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResultDto<PullRequestSearchResultDto>>> SearchPullRequests(
        [FromQuery] [Required] string q,
        [FromQuery] PagedQuery paging,
        [FromQuery] Guid? repositoryId,
        [FromQuery] string? state,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        EnsureQueryPresent(q);

        return Ok(await _searchService.SearchPullRequestsAsync(
            q, paging, repositoryId, state, from, to, cancellationToken));
    }

    /// <summary>Searches review comment bodies.</summary>
    /// <param name="q">Search terms. Required.</param>
    /// <param name="paging">Pagination parameters.</param>
    /// <param name="repositoryName">
    /// Restrict to a repository by full name. The review documents carry the
    /// repository name rather than its id.
    /// </param>
    /// <param name="author">Restrict to a comment author (exact match).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching review comments with highlight snippets.</returns>
    [HttpGet("reviews", Name = "SearchReviews")]
    [ProducesResponseType<SearchResultDto<ReviewSearchResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResultDto<ReviewSearchResultDto>>> SearchReviews(
        [FromQuery] [Required] string q,
        [FromQuery] PagedQuery paging,
        [FromQuery] string? repositoryName,
        [FromQuery] string? author,
        CancellationToken cancellationToken)
    {
        EnsureQueryPresent(q);

        return Ok(await _searchService.SearchReviewsAsync(
            q, paging, repositoryName, author, cancellationToken));
    }

    /// <summary>Queues a rebuild of one of the indices.</summary>
    /// <remarks>
    /// Returns as soon as the jobs are queued; the Worker performs the indexing.
    /// Indexing is idempotent, so running this against an already-populated index
    /// overwrites documents rather than duplicating them.
    /// </remarks>
    /// <param name="contentType">Which index to rebuild.</param>
    /// <param name="repositoryId">Restrict to one repository, or all when omitted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of jobs queued.</returns>
    [HttpPost("reindex", Name = "Reindex")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reindex(
        [FromQuery] IndexContentType contentType,
        [FromQuery] Guid? repositoryId,
        CancellationToken cancellationToken)
    {
        var queued = await _searchIndexService.EnqueueReindexAsync(
            contentType, repositoryId, cancellationToken);

        return Accepted(new { content_type = contentType.ToString(), queued_jobs = queued });
    }

    /// <summary>
    /// [Required] alone accepts a whitespace-only string, which OpenSearch would
    /// happily run as a query that matches nothing.
    /// </summary>
    private static void EnsureQueryPresent(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            throw new DomainValidationException(
                "A search query is required.",
                new Dictionary<string, string[]>
                {
                    ["q"] = ["Provide at least one non-whitespace search term."]
                });
        }
    }
}
