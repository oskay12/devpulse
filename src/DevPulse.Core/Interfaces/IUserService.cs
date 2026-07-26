using DevPulse.Core.Dtos;
using DevPulse.Core.Enums;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// User accounts and developer profile queries.
/// </summary>
public interface IUserService
{
    /// <summary>Lists users, optionally filtered.</summary>
    Task<PagedResultDto<UserDetailDto>> ListAsync(
        PagedQuery paging,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a user by id.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such user.</exception>
    Task<UserDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates a user, hashing the supplied password.</summary>
    /// <exception cref="Exceptions.ConflictException">Username or email already taken.</exception>
    Task<UserDetailDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a user's profile.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such user.</exception>
    /// <exception cref="Exceptions.ConflictException">Email already taken.</exception>
    Task<UserDetailDto> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a user. Soft delete, because commits and reviews reference the
    /// user and a hard delete would either fail or null out attribution.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">No such user.</exception>
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Builds the developer profile view: identity, metrics and repositories.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such user.</exception>
    Task<DeveloperProfileDto> GetProfileAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Aggregates a developer's metrics over the requested window.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such user.</exception>
    Task<DeveloperMetricsDto> GetMetricsAsync(
        Guid id,
        MetricPeriodType? periodType = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
}
