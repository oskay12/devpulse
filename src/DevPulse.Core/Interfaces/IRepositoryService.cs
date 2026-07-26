using DevPulse.Core.Dtos;
using DevPulse.Core.Enums;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Repository registration and analytics queries.
/// </summary>
public interface IRepositoryService
{
    /// <summary>Lists repositories, optionally filtered.</summary>
    Task<PagedResultDto<RepositoryDetailDto>> ListAsync(
        PagedQuery paging,
        RepositoryProvider? provider = null,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a repository by id.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<RepositoryDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Registers a repository.</summary>
    /// <exception cref="Exceptions.ConflictException">Full name or provider/external id already registered.</exception>
    /// <exception cref="Exceptions.DomainValidationException">Owner does not exist.</exception>
    Task<RepositoryDetailDto> CreateAsync(
        CreateRepositoryRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a repository's mutable metadata.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<RepositoryDetailDto> UpdateAsync(
        Guid id,
        UpdateRepositoryRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a repository. This is a soft delete: a hard delete would cascade
    /// away every commit, pull request and metric attached to it.
    /// </summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists a repository's contributors, ordered by commit count.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<List<TopContributorDto>> GetContributorsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>Builds the aggregate analytics view for a repository.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<RepositoryMetricsDto> GetMetricsAsync(
        Guid id,
        int trendDays = 30,
        CancellationToken cancellationToken = default);

    /// <summary>Lists code health snapshots, newest first.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<List<CodeHealthScoreDto>> GetHealthScoresAsync(
        Guid id,
        bool latestOnly = false,
        CancellationToken cancellationToken = default);
}
