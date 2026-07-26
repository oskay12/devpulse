using System.ComponentModel.DataAnnotations;
using DevPulse.Core.Dtos;
using DevPulse.Core.Enums;
using DevPulse.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

/// <summary>
/// Repository registration and analytics.
/// </summary>
[ApiController]
[Route("api/repositories")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class RepositoriesController : ControllerBase
{
    private readonly IRepositoryService _repositoryService;
    private readonly ICommitService _commitService;
    private readonly IPullRequestService _pullRequestService;

    /// <summary>Initialises the controller.</summary>
    public RepositoriesController(
        IRepositoryService repositoryService,
        ICommitService commitService,
        IPullRequestService pullRequestService)
    {
        _repositoryService = repositoryService;
        _commitService = commitService;
        _pullRequestService = pullRequestService;
    }

    /// <summary>Lists registered repositories.</summary>
    /// <param name="paging">Pagination parameters.</param>
    /// <param name="provider">Filter by source provider.</param>
    /// <param name="isActive">Filter by monitoring status.</param>
    /// <param name="q">Case-insensitive substring match on name or full name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of repositories.</returns>
    [HttpGet(Name = "ListRepositories")]
    [ProducesResponseType<PagedResultDto<RepositoryDetailDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<RepositoryDetailDto>>> List(
        [FromQuery] PagedQuery paging,
        [FromQuery] RepositoryProvider? provider,
        [FromQuery] bool? isActive,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
        => Ok(await _repositoryService.ListAsync(paging, provider, isActive, q, cancellationToken));

    /// <summary>Gets a repository by id.</summary>
    /// <param name="id">Repository UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The repository.</returns>
    [HttpGet("{id:guid}", Name = "GetRepository")]
    [ProducesResponseType<RepositoryDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepositoryDetailDto>> Get(Guid id, CancellationToken cancellationToken)
        => Ok(await _repositoryService.GetAsync(id, cancellationToken));

    /// <summary>Registers a repository.</summary>
    /// <param name="request">Repository details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created repository.</returns>
    [HttpPost(Name = "CreateRepository")]
    [ProducesResponseType<RepositoryDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RepositoryDetailDto>> Create(
        [FromBody] CreateRepositoryRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _repositoryService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute("GetRepository", new { id = created.Id }, created);
    }

    /// <summary>Updates a repository's mutable metadata.</summary>
    /// <param name="id">Repository UUID.</param>
    /// <param name="request">Updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated repository.</returns>
    [HttpPut("{id:guid}", Name = "UpdateRepository")]
    [ProducesResponseType<RepositoryDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepositoryDetailDto>> Update(
        Guid id,
        [FromBody] UpdateRepositoryRequest request,
        CancellationToken cancellationToken)
        => Ok(await _repositoryService.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Deactivates a repository. This is a soft delete — commits, pull requests and
    /// metrics are retained.
    /// </summary>
    /// <param name="id">Repository UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:guid}", Name = "DeactivateRepository")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _repositoryService.DeactivateAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>Lists a repository's commits, newest first.</summary>
    /// <param name="id">Repository UUID.</param>
    /// <param name="paging">Pagination parameters.</param>
    /// <param name="branch">Filter by branch name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of commits.</returns>
    [HttpGet("{id:guid}/commits", Name = "ListRepositoryCommits")]
    [ProducesResponseType<PagedResultDto<CommitDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResultDto<CommitDetailDto>>> ListCommits(
        Guid id,
        [FromQuery] PagedQuery paging,
        [FromQuery] string? branch,
        CancellationToken cancellationToken)
        => Ok(await _commitService.ListByRepositoryAsync(id, paging, branch, cancellationToken));

    /// <summary>Lists a repository's pull requests, newest first.</summary>
    /// <param name="id">Repository UUID.</param>
    /// <param name="paging">Pagination parameters.</param>
    /// <param name="state">Filter by pull request state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of pull requests.</returns>
    [HttpGet("{id:guid}/pull-requests", Name = "ListRepositoryPullRequests")]
    [ProducesResponseType<PagedResultDto<PullRequestDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResultDto<PullRequestDetailDto>>> ListPullRequests(
        Guid id,
        [FromQuery] PagedQuery paging,
        [FromQuery] PullRequestState? state,
        CancellationToken cancellationToken)
        => Ok(await _pullRequestService.ListByRepositoryAsync(id, paging, state, cancellationToken));

    /// <summary>Lists a repository's contributors, ordered by commit count.</summary>
    /// <param name="id">Repository UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The contributor leaderboard.</returns>
    [HttpGet("{id:guid}/contributors", Name = "ListRepositoryContributors")]
    [ProducesResponseType<List<TopContributorDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TopContributorDto>>> ListContributors(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await _repositoryService.GetContributorsAsync(id, cancellationToken));

    /// <summary>Returns aggregate analytics for a repository.</summary>
    /// <param name="id">Repository UUID.</param>
    /// <param name="trendDays">Length of the commit trend window, in days.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Repository metrics.</returns>
    [HttpGet("{id:guid}/metrics", Name = "GetRepositoryMetrics")]
    [ProducesResponseType<RepositoryMetricsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RepositoryMetricsDto>> GetMetrics(
        Guid id,
        [FromQuery] [Range(1, 365)] int trendDays = 30,
        CancellationToken cancellationToken = default)
        => Ok(await _repositoryService.GetMetricsAsync(id, trendDays, cancellationToken));

    /// <summary>Lists code health snapshots for a repository, newest first.</summary>
    /// <param name="id">Repository UUID.</param>
    /// <param name="latestOnly">Return only the most recent snapshot.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Code health snapshots.</returns>
    [HttpGet("{id:guid}/health-scores", Name = "ListRepositoryHealthScores")]
    [ProducesResponseType<List<CodeHealthScoreDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CodeHealthScoreDto>>> ListHealthScores(
        Guid id,
        [FromQuery] bool latestOnly,
        CancellationToken cancellationToken)
        => Ok(await _repositoryService.GetHealthScoresAsync(id, latestOnly, cancellationToken));
}
