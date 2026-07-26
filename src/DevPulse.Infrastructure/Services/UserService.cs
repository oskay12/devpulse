using System.Linq.Expressions;
using DevPulse.Core.Dtos;
using DevPulse.Core.Entities;
using DevPulse.Core.Enums;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services;

/// <inheritdoc cref="IUserService"/>
internal sealed class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<UserDetailDto>> ListAsync(
        PagedQuery paging,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users.AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.Username, pattern) || EF.Functions.ILike(u.Email, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.Username)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(Projection)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<UserDetailDto>
        {
            TotalCount = totalCount,
            Page = paging.Page,
            PageSize = paging.PageSize,
            Items = items
        };
    }

    public async Task<UserDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return user ?? throw new NotFoundException("User", id);
    }

    public async Task<UserDetailDto> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalisedEmail = request.Email.Trim();

        var duplicate = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                u => u.Username == request.Username || u.Email == normalisedEmail,
                cancellationToken);

        if (duplicate)
        {
            throw new ConflictException("That username or email address is already registered.");
        }

        var entity = new User
        {
            Id = Guid.CreateVersion7(),
            Username = request.Username,
            Email = normalisedEmail,
            // The plaintext password never leaves this line.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            AvatarUrl = request.AvatarUrl,
            CreatedAt = DateTime.UtcNow,
            Role = request.Role,
            IsActive = true
        };

        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetAsync(entity.Id, cancellationToken);
    }

    public async Task<UserDetailDto> UpdateAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("User", id);

        var normalisedEmail = request.Email.Trim();

        var emailTaken = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id != id && u.Email == normalisedEmail, cancellationToken);

        if (emailTaken)
        {
            throw new ConflictException("That email address is already registered.");
        }

        entity.Email = normalisedEmail;
        entity.AvatarUrl = request.AvatarUrl;
        entity.Role = request.Role;
        entity.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetAsync(id, cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new NotFoundException("User", id);

        entity.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DeveloperProfileDto> GetProfileAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new { u.Id, u.Username, u.Email, u.AvatarUrl })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User", id);

        var repositories = await _dbContext.RepositoryContributors
            .AsNoTracking()
            .Where(rc => rc.UserId == id)
            .OrderByDescending(rc => rc.CommitCount)
            .Select(rc => new RepositorySummaryDto
            {
                RepositoryId = rc.RepositoryId,
                Name = _dbContext.Repositories
                    .Where(r => r.Id == rc.RepositoryId)
                    .Select(r => r.FullName)
                    .FirstOrDefault() ?? string.Empty,
                Role = rc.Role,
                CommitCount = rc.CommitCount
            })
            .ToListAsync(cancellationToken);

        var metrics = await GetMetricsAsync(id, cancellationToken: cancellationToken);

        return new DeveloperProfileDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            Metrics = metrics,
            Repositories = repositories
        };
    }

    public async Task<DeveloperMetricsDto> GetMetricsAsync(
        Guid id,
        MetricPeriodType? periodType = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(id, cancellationToken);

        // Computed from the source tables rather than the DeveloperMetric
        // aggregates. Those are written asynchronously by the Worker, so reading
        // them here would report zeros until the first recalculation lands.
        var commits = _dbContext.Commits.AsNoTracking().Where(c => c.AuthorId == id);
        var pullRequests = _dbContext.PullRequests.AsNoTracking().Where(pr => pr.AuthorId == id);
        var reviews = _dbContext.PullRequestReviews.AsNoTracking().Where(rv => rv.ReviewerId == id);

        if (from.HasValue)
        {
            var fromUtc = ToUtc(from.Value);
            commits = commits.Where(c => c.CommittedAt >= fromUtc);
            pullRequests = pullRequests.Where(pr => pr.CreatedAt >= fromUtc);
            reviews = reviews.Where(rv => rv.SubmittedAt >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = ToUtc(to.Value);
            commits = commits.Where(c => c.CommittedAt <= toUtc);
            pullRequests = pullRequests.Where(pr => pr.CreatedAt <= toUtc);
            reviews = reviews.Where(rv => rv.SubmittedAt <= toUtc);
        }

        var totalCommits = await commits.CountAsync(cancellationToken);
        var totalPullRequests = await pullRequests.CountAsync(cancellationToken);
        var codeReviews = await reviews.CountAsync(cancellationToken);

        var averageReviewHours = await reviews
            .Join(
                _dbContext.PullRequests.AsNoTracking(),
                rv => rv.PullRequestId,
                pr => pr.Id,
                (rv, pr) => (double?)(rv.SubmittedAt - pr.CreatedAt).TotalHours)
            .AverageAsync(cancellationToken);

        return new DeveloperMetricsDto
        {
            TotalCommits = totalCommits,
            TotalPullRequests = totalPullRequests,
            CodeReviews = codeReviews,
            AverageReviewTimeHours = Math.Round((decimal)(averageReviewHours ?? 0d), 2),
            ProductivityScore = CalculateProductivityScore(totalCommits, totalPullRequests, codeReviews)
        };
    }

    /// <summary>
    /// Weighted activity score. Reviews and merged work are weighted above raw
    /// commit count so the metric is not trivially gamed by committing more often.
    /// </summary>
    private static decimal CalculateProductivityScore(int commits, int pullRequests, int reviews)
    {
        const decimal commitWeight = 1.0m;
        const decimal pullRequestWeight = 3.0m;
        const decimal reviewWeight = 2.0m;

        return Math.Round(
            (commits * commitWeight) + (pullRequests * pullRequestWeight) + (reviews * reviewWeight),
            2);
    }

    /// <summary>
    /// Query-string dates arrive with an unspecified kind; the columns are
    /// <c>timestamp with time zone</c>, so Npgsql rejects anything but UTC.
    /// </summary>
    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private async Task EnsureExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == id, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException("User", id);
        }
    }

    private static Expression<Func<User, UserDetailDto>> Projection =>
        u => new UserDetailDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            AvatarUrl = u.AvatarUrl,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            Role = u.Role,
            IsActive = u.IsActive
        };
}
