using DevPulse.Core.Dtos;
using DevPulse.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

/// <summary>
/// Commit lookups.
/// </summary>
/// <remarks>
/// Commits are created by webhook ingestion, not by clients, so this controller is
/// read-only. List commits for a repository via
/// <c>GET /api/repositories/{id}/commits</c>.
/// </remarks>
[ApiController]
[Route("api/commits")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class CommitsController : ControllerBase
{
    private readonly ICommitService _commitService;

    /// <summary>Initialises the controller.</summary>
    public CommitsController(ICommitService commitService)
    {
        _commitService = commitService;
    }

    /// <summary>Gets a commit by id.</summary>
    /// <param name="id">Commit UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The commit.</returns>
    [HttpGet("{id:guid}", Name = "GetCommit")]
    [ProducesResponseType<CommitDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommitDetailDto>> Get(Guid id, CancellationToken cancellationToken)
        => Ok(await _commitService.GetAsync(id, cancellationToken));

    /// <summary>Lists the files changed by a commit.</summary>
    /// <param name="id">Commit UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The changed files.</returns>
    [HttpGet("{id:guid}/files", Name = "ListCommitFiles")]
    [ProducesResponseType<List<CommitFileDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CommitFileDto>>> ListFiles(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await _commitService.GetFilesAsync(id, cancellationToken));
}
