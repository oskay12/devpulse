using DevPulse.Core.Dtos;
using DevPulse.Core.Enums;
using DevPulse.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

/// <summary>
/// User accounts and developer profiles.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    /// <summary>Initialises the controller.</summary>
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Lists users.</summary>
    /// <param name="paging">Pagination parameters.</param>
    /// <param name="isActive">Filter by account status.</param>
    /// <param name="q">Case-insensitive substring match on username or email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of users.</returns>
    [HttpGet(Name = "ListUsers")]
    [ProducesResponseType<PagedResultDto<UserDetailDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<UserDetailDto>>> List(
        [FromQuery] PagedQuery paging,
        [FromQuery] bool? isActive,
        [FromQuery] string? q,
        CancellationToken cancellationToken)
        => Ok(await _userService.ListAsync(paging, isActive, q, cancellationToken));

    /// <summary>Gets a user by id.</summary>
    /// <param name="id">User UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user.</returns>
    [HttpGet("{id:guid}", Name = "GetUser")]
    [ProducesResponseType<UserDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> Get(Guid id, CancellationToken cancellationToken)
        => Ok(await _userService.GetAsync(id, cancellationToken));

    /// <summary>Creates a user.</summary>
    /// <param name="request">User details, including the plaintext password to hash.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user.</returns>
    [HttpPost(Name = "CreateUser")]
    [ProducesResponseType<UserDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailDto>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _userService.CreateAsync(request, cancellationToken);

        return CreatedAtRoute("GetUser", new { id = created.Id }, created);
    }

    /// <summary>Updates a user's profile.</summary>
    /// <param name="id">User UUID.</param>
    /// <param name="request">Updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("{id:guid}", Name = "UpdateUser")]
    [ProducesResponseType<UserDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailDto>> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
        => Ok(await _userService.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Deactivates a user. Soft delete — commits and reviews keep their attribution.
    /// </summary>
    /// <param name="id">User UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpDelete("{id:guid}", Name = "DeactivateUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _userService.DeactivateAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>Returns a developer's profile: identity, metrics and repositories.</summary>
    /// <param name="id">User UUID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The developer profile.</returns>
    [HttpGet("{id:guid}/profile", Name = "GetUserProfile")]
    [ProducesResponseType<DeveloperProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeveloperProfileDto>> GetProfile(
        Guid id,
        CancellationToken cancellationToken)
        => Ok(await _userService.GetProfileAsync(id, cancellationToken));

    /// <summary>Returns a developer's aggregated metrics.</summary>
    /// <param name="id">User UUID.</param>
    /// <param name="periodType">Aggregation period.</param>
    /// <param name="from">Inclusive lower bound (UTC).</param>
    /// <param name="to">Inclusive upper bound (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Developer metrics.</returns>
    [HttpGet("{id:guid}/metrics", Name = "GetUserMetrics")]
    [ProducesResponseType<DeveloperMetricsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeveloperMetricsDto>> GetMetrics(
        Guid id,
        [FromQuery] MetricPeriodType? periodType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
        => Ok(await _userService.GetMetricsAsync(id, periodType, from, to, cancellationToken));
}
