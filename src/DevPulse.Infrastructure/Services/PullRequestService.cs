using System.Linq.Expressions;
using DevPulse.Core.Dtos;
using DevPulse.Core.Entities;
using DevPulse.Core.Enums;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services;

/// <inheritdoc cref="IPullRequestService"/>
internal sealed class PullRequestService : IPullRequestService
{
    private readonly ApplicationDbContext _dbContext;

    public PullRequestService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<PullRequestDetailDto>> ListByRepositoryAsync(
        Guid repositoryId,
        PagedQuery paging,
        PullRequestState? state = null,
        CancellationToken cancellationToken = default)
    {
        var repositoryExists = await _dbContext.Repositories
            .AsNoTracking()
            .AnyAsync(r => r.Id == repositoryId, cancellationToken);

        if (!repositoryExists)
        {
            throw new NotFoundException("Repository", repositoryId);
        }

        var query = _dbContext.PullRequests
            .AsNoTracking()
            .Where(pr => pr.RepositoryId == repositoryId);

        if (state.HasValue)
        {
            query = query.Where(pr => pr.State == state.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(pr => pr.CreatedAt)
            .ThenBy(pr => pr.Id)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(Projection)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<PullRequestDetailDto>
        {
            TotalCount = totalCount,
            Page = paging.Page,
            PageSize = paging.PageSize,
            Items = items
        };
    }

    public async Task<PullRequestDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var pullRequest = await _dbContext.PullRequests
            .AsNoTracking()
            .Where(pr => pr.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return pullRequest ?? throw new NotFoundException("PullRequest", id);
    }

    public async Task<List<ReviewDto>> GetReviewsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(id, cancellationToken);

        return await _dbContext.PullRequestReviews
            .AsNoTracking()
            .Where(rv => rv.PullRequestId == id)
            .OrderBy(rv => rv.SubmittedAt)
            .Select(rv => new ReviewDto
            {
                Id = rv.Id,
                PullRequestId = rv.PullRequestId,
                ReviewerId = rv.ReviewerId,
                ReviewerUsername = _dbContext.Users
                    .Where(u => u.Id == rv.ReviewerId)
                    .Select(u => u.Username)
                    .FirstOrDefault(),
                State = rv.State,
                Comment = rv.Comment,
                SubmittedAt = rv.SubmittedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ReviewCommentDto>> GetCommentsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(id, cancellationToken);

        return await _dbContext.ReviewComments
            .AsNoTracking()
            .Where(rc => rc.PullRequestId == id)
            .OrderBy(rc => rc.CreatedAt)
            .Select(rc => new ReviewCommentDto
            {
                Id = rc.Id,
                PullRequestId = rc.PullRequestId,
                ReviewId = rc.ReviewId,
                AuthorId = rc.AuthorId,
                AuthorUsername = _dbContext.Users
                    .Where(u => u.Id == rc.AuthorId)
                    .Select(u => u.Username)
                    .FirstOrDefault(),
                Body = rc.Body,
                FilePath = rc.FilePath,
                LineNumber = rc.LineNumber,
                CreatedAt = rc.CreatedAt,
                UpdatedAt = rc.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ReviewDto> AddReviewAsync(
        Guid id,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(id, cancellationToken);
        await EnsureUserExistsAsync(request.ReviewerId, nameof(request.ReviewerId), cancellationToken);

        var entity = new PullRequestReview
        {
            Id = Guid.CreateVersion7(),
            PullRequestId = id,
            ReviewerId = request.ReviewerId,
            State = request.State,
            Comment = request.Comment,
            SubmittedAt = DateTime.UtcNow
        };

        _dbContext.PullRequestReviews.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var reviewerUsername = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == entity.ReviewerId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(cancellationToken);

        return new ReviewDto
        {
            Id = entity.Id,
            PullRequestId = entity.PullRequestId,
            ReviewerId = entity.ReviewerId,
            ReviewerUsername = reviewerUsername,
            State = entity.State,
            Comment = entity.Comment,
            SubmittedAt = entity.SubmittedAt
        };
    }

    public async Task<ReviewCommentDto> AddCommentAsync(
        Guid id,
        CreateReviewCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(id, cancellationToken);
        await EnsureUserExistsAsync(request.AuthorId, nameof(request.AuthorId), cancellationToken);

        if (request.ReviewId.HasValue)
        {
            // The review must belong to this same pull request, otherwise the
            // comment would appear threaded under an unrelated discussion.
            var reviewBelongs = await _dbContext.PullRequestReviews
                .AsNoTracking()
                .AnyAsync(
                    rv => rv.Id == request.ReviewId.Value && rv.PullRequestId == id,
                    cancellationToken);

            if (!reviewBelongs)
            {
                throw new DomainValidationException(
                    "The specified review does not belong to this pull request.",
                    new Dictionary<string, string[]>
                    {
                        [nameof(request.ReviewId)] = ["No such review on this pull request."]
                    });
            }
        }

        var entity = new ReviewComment
        {
            Id = Guid.CreateVersion7(),
            PullRequestId = id,
            ReviewId = request.ReviewId,
            AuthorId = request.AuthorId,
            Body = request.Body,
            FilePath = request.FilePath,
            LineNumber = request.LineNumber,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ReviewComments.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var authorUsername = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == entity.AuthorId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync(cancellationToken);

        return new ReviewCommentDto
        {
            Id = entity.Id,
            PullRequestId = entity.PullRequestId,
            ReviewId = entity.ReviewId,
            AuthorId = entity.AuthorId,
            AuthorUsername = authorUsername,
            Body = entity.Body,
            FilePath = entity.FilePath,
            LineNumber = entity.LineNumber,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private async Task EnsureExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.PullRequests
            .AsNoTracking()
            .AnyAsync(pr => pr.Id == id, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException("PullRequest", id);
        }
    }

    private async Task EnsureUserExistsAsync(
        Guid userId,
        string fieldName,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId, cancellationToken);

        if (!exists)
        {
            throw new DomainValidationException(
                "The specified user does not exist.",
                new Dictionary<string, string[]>
                {
                    [fieldName] = ["No user exists with this id."]
                });
        }
    }

    private Expression<Func<PullRequest, PullRequestDetailDto>> Projection =>
        pr => new PullRequestDetailDto
        {
            Id = pr.Id,
            RepositoryId = pr.RepositoryId,
            RepositoryName = _dbContext.Repositories
                .Where(r => r.Id == pr.RepositoryId)
                .Select(r => r.FullName)
                .FirstOrDefault(),
            PrNumber = pr.PrNumber,
            Title = pr.Title,
            Description = pr.Description,
            AuthorId = pr.AuthorId,
            AuthorUsername = _dbContext.Users
                .Where(u => u.Id == pr.AuthorId)
                .Select(u => u.Username)
                .FirstOrDefault(),
            SourceBranch = pr.SourceBranch,
            TargetBranch = pr.TargetBranch,
            State = pr.State,
            CreatedAt = pr.CreatedAt,
            UpdatedAt = pr.UpdatedAt,
            MergedAt = pr.MergedAt,
            ClosedAt = pr.ClosedAt,
            MergedById = pr.MergedById,
            CommitCount = pr.CommitCount,
            FilesChanged = pr.FilesChanged,
            Additions = pr.Additions,
            Deletions = pr.Deletions,
            IsDraft = pr.IsDraft
        };
}
