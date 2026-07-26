using DevPulse.Core.Dtos;
using DevPulse.Core.Enums;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Pull request, review and comment operations.
/// </summary>
public interface IPullRequestService
{
    /// <summary>Lists pull requests in a repository, newest first.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such repository.</exception>
    Task<PagedResultDto<PullRequestDetailDto>> ListByRepositoryAsync(
        Guid repositoryId,
        PagedQuery paging,
        PullRequestState? state = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a pull request by id.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such pull request.</exception>
    Task<PullRequestDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists reviews on a pull request.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such pull request.</exception>
    Task<List<ReviewDto>> GetReviewsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists comments on a pull request.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such pull request.</exception>
    Task<List<ReviewCommentDto>> GetCommentsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Submits a review on a pull request.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such pull request.</exception>
    /// <exception cref="Exceptions.DomainValidationException">Reviewer does not exist.</exception>
    Task<ReviewDto> AddReviewAsync(
        Guid id,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a comment to a pull request.</summary>
    /// <exception cref="Exceptions.NotFoundException">No such pull request.</exception>
    /// <exception cref="Exceptions.DomainValidationException">Author or parent review does not exist.</exception>
    Task<ReviewCommentDto> AddCommentAsync(
        Guid id,
        CreateReviewCommentRequest request,
        CancellationToken cancellationToken = default);
}
