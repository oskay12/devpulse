using DevPulse.Core.Dtos;
using DevPulse.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

/// <summary>
/// Webhook tokens scoped to a repository.
/// </summary>
/// <remarks>
/// These tokens are what inbound GitLab webhooks authenticate with. GitHub uses
/// HMAC signing against the configured webhook secret instead.
/// </remarks>
[ApiController]
[Route("api/repositories/{repositoryId:guid}/tokens")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class ProjectTokensController : ControllerBase
{
    private readonly IProjectTokenService _projectTokenService;

    /// <summary>Initialises the controller.</summary>
    public ProjectTokensController(IProjectTokenService projectTokenService)
    {
        _projectTokenService = projectTokenService;
    }

    /// <summary>Lists a repository's tokens. Token values are never returned.</summary>
    /// <param name="repositoryId">Repository UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token metadata.</returns>
    [HttpGet(Name = "ListProjectTokens")]
    [ProducesResponseType<List<ProjectTokenDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ProjectTokenDto>>> List(
        Guid repositoryId,
        CancellationToken cancellationToken)
        => Ok(await _projectTokenService.ListAsync(repositoryId, cancellationToken));

    /// <summary>
    /// Issues a token. The plaintext value is returned once in this response and
    /// cannot be retrieved afterwards.
    /// </summary>
    /// <param name="repositoryId">Repository UUID.</param>
    /// <param name="request">Token details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The token metadata and its one-time plaintext value.</returns>
    [HttpPost(Name = "CreateProjectToken")]
    [ProducesResponseType<CreateProjectTokenResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreateProjectTokenResponse>> Create(
        Guid repositoryId,
        [FromBody] CreateProjectTokenRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _projectTokenService.CreateAsync(repositoryId, request, cancellationToken);

        return CreatedAtRoute("ListProjectTokens", new { repositoryId }, created);
    }

    /// <summary>Revokes a token.</summary>
    /// <param name="repositoryId">Repository UUID.</param>
    /// <param name="tokenId">Token UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{tokenId:guid}", Name = "RevokeProjectToken")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(
        Guid repositoryId,
        Guid tokenId,
        CancellationToken cancellationToken)
    {
        await _projectTokenService.RevokeAsync(repositoryId, tokenId, cancellationToken);

        return NoContent();
    }
}
