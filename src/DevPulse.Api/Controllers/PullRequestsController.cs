using DevPulse.Core.Dtos;
using DevPulse.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

/// <summary>
/// Pull requests, reviews and comments.
/// </summary>
/// <remarks>
/// Pull requests themselves are created by webhook ingestion. Reviews and comments
/// can also be posted directly, which is what a DevPulse-native review flow uses.
/// List pull requests for a repository via
/// <c>GET /api/repositories/{id}/pull-requests</c>.
/// </remarks>
[ApiController]
[Route("api/pull-requests")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class PullRequestsController : ControllerBase
{
    private readonly IPullRequestService _pullRequestService;

    /// <summary>Initialises the controller.</summary>
    public PullRequestsController(IPullRequestService pullRequestService)
    {
        _pullRequestService = pullRequestService;
    }

    /// <summary>Gets a pull request by id.</summary>
    /// <param name="id">Pull request UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pull request.</returns>
    [HttpGet("{id:guid}", Name = "GetPullRequest")]
    [ProducesResponseType<PullRequestDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PullRequestDetailDto>> Get(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await _pullRequestService.GetAsync(id, cancellationToken));

    /// <summary>Lists reviews on a pull request.</summary>
    /// <param name="id">Pull request UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reviews.</returns>
    [HttpGet("{id:guid}/reviews", Name = "ListPullRequestReviews")]
    [ProducesResponseType<List<ReviewDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReviewDto>>> ListReviews(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await _pullRequestService.GetReviewsAsync(id, cancellationToken));

    /// <summary>Submits a review on a pull request.</summary>
    /// <param name="id">Pull request UUID.</param>
    /// <param name="request">Review details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created review.</returns>
    [HttpPost("{id:guid}/reviews", Name = "CreatePullRequestReview")]
    [ProducesResponseType<ReviewDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> CreateReview(
        Guid id,
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _pullRequestService.AddReviewAsync(id, request, cancellationToken);

        return CreatedAtRoute("ListPullRequestReviews", new { id }, created);
    }

    /// <summary>Lists comments on a pull request.</summary>
    /// <param name="id">Pull request UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The comments.</returns>
    [HttpGet("{id:guid}/comments", Name = "ListPullRequestComments")]
    [ProducesResponseType<List<ReviewCommentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ReviewCommentDto>>> ListComments(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await _pullRequestService.GetCommentsAsync(id, cancellationToken));

    /// <summary>Adds a comment to a pull request.</summary>
    /// <param name="id">Pull request UUID.</param>
    /// <param name="request">Comment details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created comment.</returns>
    [HttpPost("{id:guid}/comments", Name = "CreatePullRequestComment")]
    [ProducesResponseType<ReviewCommentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewCommentDto>> CreateComment(
        Guid id,
        [FromBody] CreateReviewCommentRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _pullRequestService.AddCommentAsync(id, request, cancellationToken);

        return CreatedAtRoute("ListPullRequestComments", new { id }, created);
    }
}
