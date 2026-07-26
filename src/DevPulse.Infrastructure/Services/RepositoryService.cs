using System.Linq.Expressions;
using DevPulse.Core.Dtos;
using DevPulse.Core.Entities;
using DevPulse.Core.Enums;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services;

/// <inheritdoc cref="IRepositoryService"/>
internal sealed class RepositoryService : IRepositoryService
{
    private const int TopContributorCount = 5;

    private readonly ApplicationDbContext _dbContext;

    public RepositoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<RepositoryDetailDto>> ListAsync(
        PagedQuery paging,
        RepositoryProvider? provider = null,
        bool? isActive = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Repositories.AsNoTracking();

        if (provider.HasValue)
        {
            query = query.Where(r => r.Provider == provider.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(r =>
                EF.Functions.ILike(r.FullName, pattern) || EF.Functions.ILike(r.Name, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            // Id breaks ties so paging stays stable when timestamps collide.
            .OrderByDescending(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(Projection)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<RepositoryDetailDto>
        {
            TotalCount = totalCount,
            Page = paging.Page,
            PageSize = paging.PageSize,
            Items = items
        };
    }

    public async Task<RepositoryDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var repository = await _dbContext.Repositories
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return repository ?? throw new NotFoundException("Repository", id);
    }

    public async Task<RepositoryDetailDto> CreateAsync(
        CreateRepositoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var ownerExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.OwnerId, cancellationToken);

        if (!ownerExists)
        {
            throw new DomainValidationException(
                "The specified owner does not exist.",
                new Dictionary<string, string[]>
                {
                    [nameof(request.OwnerId)] = ["No user exists with this id."]
                });
        }

        // Pre-checked purely to return a useful message. The unique indexes are the
        // real guard, and a concurrent insert still surfaces as a 409 via the
        // constraint translation in ApplicationDbContext.
        var duplicate = await _dbContext.Repositories
            .AsNoTracking()
            .AnyAsync(
                r => r.FullName == request.FullName
                     || (r.Provider == request.Provider && r.ExternalId == request.ExternalId),
                cancellationToken);

        if (duplicate)
        {
            throw new ConflictException(
                $"A repository with full name '{request.FullName}' or the same provider/external id is already registered.");
        }

        var entity = new Repository
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            FullName = request.FullName,
            Description = request.Description,
            CloneUrl = request.CloneUrl,
            DefaultBranch = request.DefaultBranch,
            Provider = request.Provider,
            ExternalId = request.ExternalId,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow,
            IsPrivate = request.IsPrivate,
            IsActive = true,
            StarCount = request.StarCount,
            ForkCount = request.ForkCount
        };

        _dbContext.Repositories.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetAsync(entity.Id, cancellationToken);
    }

    public async Task<RepositoryDetailDto> UpdateAsync(
        Guid id,
        UpdateRepositoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Repositories
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Repository", id);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.CloneUrl = request.CloneUrl;
        entity.DefaultBranch = request.DefaultBranch;
        entity.IsPrivate = request.IsPrivate;
        entity.IsActive = request.IsActive;
        entity.StarCount = request.StarCount;
        entity.ForkCount = request.ForkCount;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetAsync(id, cancellationToken);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Repositories
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("Repository", id);

        entity.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TopContributorDto>> GetContributorsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(id, cancellationToken);

        return await _dbContext.RepositoryContributors
            .AsNoTracking()
            .Where(rc => rc.RepositoryId == id)
            .OrderByDescending(rc => rc.CommitCount)
            .ThenBy(rc => rc.UserId)
            .Select(rc => new TopContributorDto
            {
                UserId = rc.UserId,
                Username = _dbContext.Users
                    .Where(u => u.Id == rc.UserId)
                    .Select(u => u.Username)
                    .FirstOrDefault() ?? string.Empty,
                CommitCount = rc.CommitCount,
                PullRequestCount = _dbContext.PullRequests
                    .Count(pr => pr.RepositoryId == id && pr.AuthorId == rc.UserId)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<RepositoryMetricsDto> GetMetricsAsync(
        Guid id,
        int trendDays = 30,
        CancellationToken cancellationToken = default)
    {
        var repository = await _dbContext.Repositories
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new { r.Id, r.FullName })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Repository", id);

        var totalCommits = await _dbContext.Commits
            .AsNoTracking()
            .CountAsync(c => c.RepositoryId == id, cancellationToken);

        var totalPullRequests = await _dbContext.PullRequests
            .AsNoTracking()
            .CountAsync(pr => pr.RepositoryId == id, cancellationToken);

        var activeContributors = await _dbContext.RepositoryContributors
            .AsNoTracking()
            .CountAsync(rc => rc.RepositoryId == id, cancellationToken);

        var latestHealthScore = await _dbContext.CodeHealthScores
            .AsNoTracking()
            .Where(s => s.RepositoryId == id)
            .OrderByDescending(s => s.CalculatedAt)
            .Select(s => (decimal?)s.OverallScore)
            .FirstOrDefaultAsync(cancellationToken);

        var since = DateTime.UtcNow.Date.AddDays(-Math.Max(trendDays, 1) + 1);

        var commitTrend = await _dbContext.Commits
            .AsNoTracking()
            .Where(c => c.RepositoryId == id && c.CommittedAt >= since)
            .GroupBy(c => c.CommittedAt.Date)
            .Select(g => new MetricDataPointDto
            {
                Date = g.Key,
                Value = g.Count()
            })
            .OrderBy(p => p.Date)
            .ToListAsync(cancellationToken);

        var topContributors = await GetContributorsAsync(id, cancellationToken);

        return new RepositoryMetricsDto
        {
            RepositoryId = repository.Id,
            RepositoryName = repository.FullName,
            TotalCommits = totalCommits,
            TotalPullRequests = totalPullRequests,
            ActiveContributors = activeContributors,
            CodeHealthScore = latestHealthScore ?? 0m,
            TopContributors = topContributors.Take(TopContributorCount).ToList(),
            CommitTrend = commitTrend
        };
    }

    public async Task<List<CodeHealthScoreDto>> GetHealthScoresAsync(
        Guid id,
        bool latestOnly = false,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(id, cancellationToken);

        var query = _dbContext.CodeHealthScores
            .AsNoTracking()
            .Where(s => s.RepositoryId == id)
            .OrderByDescending(s => s.CalculatedAt)
            .Select(s => new CodeHealthScoreDto
            {
                Id = s.Id,
                RepositoryId = s.RepositoryId,
                CalculatedAt = s.CalculatedAt,
                OverallScore = s.OverallScore,
                MaintainabilityScore = s.MaintainabilityScore,
                TestCoverageScore = s.TestCoverageScore,
                DocumentationScore = s.DocumentationScore,
                TechnicalDebtMinutes = s.TechnicalDebtMinutes,
                CodeSmellCount = s.CodeSmellCount,
                DuplicationPercentage = s.DuplicationPercentage,
                ComplexityScore = s.ComplexityScore
            });

        if (latestOnly)
        {
            query = query.Take(1);
        }

        return await query.ToListAsync(cancellationToken);
    }

    private async Task EnsureExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Repositories
            .AsNoTracking()
            .AnyAsync(r => r.Id == id, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException("Repository", id);
        }
    }

    /// <summary>
    /// Shared projection, exposed as an expression so EF Core can translate it —
    /// a method call inside <c>Select</c> would fail at query time instead.
    /// </summary>
    /// <remarks>
    /// The owner username is a correlated subquery rather than a second round trip:
    /// the entities carry no navigation properties, so there is nothing to
    /// <c>Include</c>.
    /// </remarks>
    private Expression<Func<Repository, RepositoryDetailDto>> Projection =>
        r => new RepositoryDetailDto
        {
            Id = r.Id,
            Name = r.Name,
            FullName = r.FullName,
            Description = r.Description,
            CloneUrl = r.CloneUrl,
            DefaultBranch = r.DefaultBranch,
            Provider = r.Provider,
            ExternalId = r.ExternalId,
            OwnerId = r.OwnerId,
            OwnerUsername = _dbContext.Users
                .Where(u => u.Id == r.OwnerId)
                .Select(u => u.Username)
                .FirstOrDefault(),
            CreatedAt = r.CreatedAt,
            LastSyncedAt = r.LastSyncedAt,
            IsPrivate = r.IsPrivate,
            IsActive = r.IsActive,
            StarCount = r.StarCount,
            ForkCount = r.ForkCount
        };
}
