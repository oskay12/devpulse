using System.Linq.Expressions;
using DevPulse.Core.Dtos;
using DevPulse.Core.Entities;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services;

/// <inheritdoc cref="ICommitService"/>
internal sealed class CommitService : ICommitService
{
    private readonly ApplicationDbContext _dbContext;

    public CommitService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<CommitDetailDto>> ListByRepositoryAsync(
        Guid repositoryId,
        PagedQuery paging,
        string? branch = null,
        CancellationToken cancellationToken = default)
    {
        var repositoryExists = await _dbContext.Repositories
            .AsNoTracking()
            .AnyAsync(r => r.Id == repositoryId, cancellationToken);

        if (!repositoryExists)
        {
            throw new NotFoundException("Repository", repositoryId);
        }

        var query = _dbContext.Commits
            .AsNoTracking()
            .Where(c => c.RepositoryId == repositoryId);

        if (!string.IsNullOrWhiteSpace(branch))
        {
            query = query.Where(c => c.Branch == branch);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CommittedAt)
            .ThenBy(c => c.Id)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(Projection)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CommitDetailDto>
        {
            TotalCount = totalCount,
            Page = paging.Page,
            PageSize = paging.PageSize,
            Items = items
        };
    }

    public async Task<CommitDetailDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var commit = await _dbContext.Commits
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(Projection)
            .FirstOrDefaultAsync(cancellationToken);

        return commit ?? throw new NotFoundException("Commit", id);
    }

    public async Task<List<CommitFileDto>> GetFilesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Commits
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException("Commit", id);
        }

        return await _dbContext.CommitFiles
            .AsNoTracking()
            .Where(cf => cf.CommitId == id)
            .OrderBy(cf => cf.FilePath)
            .Select(cf => new CommitFileDto
            {
                Id = cf.Id,
                CommitId = cf.CommitId,
                FilePath = cf.FilePath,
                ChangeType = cf.ChangeType,
                Additions = cf.Additions,
                Deletions = cf.Deletions,
                DiffSnippet = cf.DiffSnippet
            })
            .ToListAsync(cancellationToken);
    }

    private Expression<Func<Commit, CommitDetailDto>> Projection =>
        c => new CommitDetailDto
        {
            Id = c.Id,
            RepositoryId = c.RepositoryId,
            RepositoryName = _dbContext.Repositories
                .Where(r => r.Id == c.RepositoryId)
                .Select(r => r.FullName)
                .FirstOrDefault(),
            Sha = c.Sha,
            AuthorId = c.AuthorId,
            AuthorName = c.AuthorName,
            AuthorEmail = c.AuthorEmail,
            Message = c.Message,
            Branch = c.Branch,
            CommittedAt = c.CommittedAt,
            IndexedAt = c.IndexedAt,
            FilesChanged = c.FilesChanged,
            Additions = c.Additions,
            Deletions = c.Deletions,
            ParentSha = c.ParentSha
        };
}
