using DevPulse.Core.Dtos;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Commit queries.
/// </summary>
public interface ICommitService
{
    /// <summary>Lists commits in a repository, newest first.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<PagedResultDto<CommitDetailDto>> ListByRepositoryAsync(
        Guid repositoryId,
        PagedQuery paging,
        string? branch = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a commit by id.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such commit.</exception>
    Task<CommitDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists the files changed by a commit.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such commit.</exception>
    Task<List<CommitFileDto>> GetFilesAsync(Guid id, CancellationToken cancellationToken = default);
}
