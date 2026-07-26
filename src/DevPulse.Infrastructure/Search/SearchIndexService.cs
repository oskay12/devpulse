using DevPulse.Core.Enums;
using DevPulse.Core.Interfaces;
using DevPulse.Core.Messages.Jobs;
using DevPulse.Core.SearchDocuments;
using DevPulse.Core.Settings;
using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;

namespace DevPulse.Infrastructure.Search;

/// <inheritdoc cref="ISearchIndexService"/>
internal sealed class SearchIndexService : ISearchIndexService
{
    private const int MaxDiffSnippets = 20;

    private readonly IOpenSearchClient _client;
    private readonly ApplicationDbContext _dbContext;
    private readonly OpenSearchIndexInitializer _initializer;
    private readonly OpenSearchSettings _settings;
    private readonly IMessagePublisher _publisher;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly ILogger<SearchIndexService> _logger;

    public SearchIndexService(
        IOpenSearchClient client,
        ApplicationDbContext dbContext,
        OpenSearchIndexInitializer initializer,
        IOptions<OpenSearchSettings> settings,
        IMessagePublisher publisher,
        IOptions<RabbitMqSettings> rabbitMqSettings,
        ILogger<SearchIndexService> logger)
    {
        _client = client;
        _dbContext = dbContext;
        _initializer = initializer;
        _settings = settings.Value;
        _publisher = publisher;
        _rabbitMqSettings = rabbitMqSettings.Value;
        _logger = logger;
    }

    public Task EnsureIndicesAsync(CancellationToken cancellationToken = default)
        => _initializer.EnsureIndicesAsync(cancellationToken);

    public async Task<int> EnqueueReindexAsync(
        IndexContentType contentType,
        Guid? repositoryId = null,
        CancellationToken cancellationToken = default)
    {
        var ids = contentType switch
        {
            IndexContentType.Commit => await _dbContext.Commits
                .AsNoTracking()
                .Where(c => repositoryId == null || c.RepositoryId == repositoryId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken),

            IndexContentType.PullRequest => await _dbContext.PullRequests
                .AsNoTracking()
                .Where(pr => repositoryId == null || pr.RepositoryId == repositoryId)
                .Select(pr => pr.Id)
                .ToListAsync(cancellationToken),

            IndexContentType.ReviewComment => await _dbContext.ReviewComments
                .AsNoTracking()
                .Where(rc => repositoryId == null
                             || _dbContext.PullRequests
                                 .Any(pr => pr.Id == rc.PullRequestId && pr.RepositoryId == repositoryId))
                .Select(rc => rc.Id)
                .ToListAsync(cancellationToken),

            _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, "Unsupported content type.")
        };

        foreach (var id in ids)
        {
            await _publisher.PublishAsync(
                _rabbitMqSettings.Queues.SearchIndexing,
                new IndexContentJob
                {
                    JobId = Guid.CreateVersion7(),
                    ContentType = contentType,
                    EntityId = id,
                    QueuedAt = DateTime.UtcNow
                },
                cancellationToken);
        }

        _logger.LogInformation(
            "Queued {Count} {ContentType} reindex job(s).", ids.Count, contentType);

        return ids.Count;
    }

    public async Task<bool> IndexAsync(
        IndexContentType contentType,
        Guid entityId,
        CancellationToken cancellationToken = default)
        => contentType switch
        {
            IndexContentType.Commit => await IndexCommitAsync(entityId, cancellationToken),
            IndexContentType.PullRequest => await IndexPullRequestAsync(entityId, cancellationToken),
            IndexContentType.ReviewComment => await IndexReviewCommentAsync(entityId, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, "Unsupported content type.")
        };

    private async Task<bool> IndexCommitAsync(Guid commitId, CancellationToken cancellationToken)
    {
        var commit = await _dbContext.Commits
            .AsNoTracking()
            .Where(c => c.Id == commitId)
            .Select(c => new
            {
                c.Id,
                c.RepositoryId,
                c.Sha,
                c.AuthorName,
                c.AuthorEmail,
                c.Message,
                c.Branch,
                c.CommittedAt,
                c.Additions,
                c.Deletions,
                RepositoryName = _dbContext.Repositories
                    .Where(r => r.Id == c.RepositoryId)
                    .Select(r => r.FullName)
                    .FirstOrDefault() ?? string.Empty
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (commit is null)
        {
            return false;
        }

        var files = await _dbContext.CommitFiles
            .AsNoTracking()
            .Where(cf => cf.CommitId == commitId)
            .Select(cf => new { cf.FilePath, cf.DiffSnippet })
            .ToListAsync(cancellationToken);

        var document = new CommitSearchDocument
        {
            Id = commit.Sha,
            RepositoryId = commit.RepositoryId,
            RepositoryName = commit.RepositoryName,
            AuthorName = commit.AuthorName,
            AuthorEmail = commit.AuthorEmail,
            Message = commit.Message,
            Branch = commit.Branch,
            FilePaths = files.Select(f => f.FilePath).ToList(),
            DiffSnippets = files
                .Where(f => !string.IsNullOrWhiteSpace(f.DiffSnippet))
                .Select(f => f.DiffSnippet!)
                // Capped: a large commit could otherwise push a multi-megabyte
                // document into the index for little search benefit.
                .Take(MaxDiffSnippets)
                .ToList(),
            CommittedAt = commit.CommittedAt,
            Additions = commit.Additions,
            Deletions = commit.Deletions
        };

        return await IndexDocumentAsync(document, _settings.CommitsIndex, commit.Id, cancellationToken);
    }

    private async Task<bool> IndexPullRequestAsync(Guid pullRequestId, CancellationToken cancellationToken)
    {
        var pullRequest = await _dbContext.PullRequests
            .AsNoTracking()
            .Where(pr => pr.Id == pullRequestId)
            .Select(pr => new
            {
                pr.Id,
                pr.RepositoryId,
                pr.PrNumber,
                pr.Title,
                pr.Description,
                pr.State,
                pr.CreatedAt,
                pr.MergedAt,
                RepositoryName = _dbContext.Repositories
                    .Where(r => r.Id == pr.RepositoryId)
                    .Select(r => r.FullName)
                    .FirstOrDefault() ?? string.Empty,
                AuthorName = _dbContext.Users
                    .Where(u => u.Id == pr.AuthorId)
                    .Select(u => u.Username)
                    .FirstOrDefault() ?? string.Empty
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (pullRequest is null)
        {
            return false;
        }

        var comments = await _dbContext.ReviewComments
            .AsNoTracking()
            .Where(rc => rc.PullRequestId == pullRequestId)
            .Select(rc => rc.Body)
            .ToListAsync(cancellationToken);

        var reviewers = await _dbContext.PullRequestReviews
            .AsNoTracking()
            .Where(rv => rv.PullRequestId == pullRequestId)
            .Select(rv => _dbContext.Users
                .Where(u => u.Id == rv.ReviewerId)
                .Select(u => u.Username)
                .FirstOrDefault() ?? string.Empty)
            .Distinct()
            .ToListAsync(cancellationToken);

        var document = new PullRequestSearchDocument
        {
            Id = pullRequest.Id.ToString(),
            RepositoryId = pullRequest.RepositoryId,
            RepositoryName = pullRequest.RepositoryName,
            PrNumber = pullRequest.PrNumber,
            Title = pullRequest.Title,
            Description = pullRequest.Description ?? string.Empty,
            AuthorName = pullRequest.AuthorName,
            ReviewComments = comments,
            Reviewers = reviewers,
            State = pullRequest.State.ToString(),
            CreatedAt = pullRequest.CreatedAt,
            MergedAt = pullRequest.MergedAt
        };

        return await IndexDocumentAsync(
            document, _settings.PullRequestsIndex, pullRequest.Id, cancellationToken);
    }

    private async Task<bool> IndexReviewCommentAsync(Guid commentId, CancellationToken cancellationToken)
    {
        var comment = await _dbContext.ReviewComments
            .AsNoTracking()
            .Where(rc => rc.Id == commentId)
            .Select(rc => new
            {
                rc.Id,
                rc.PullRequestId,
                rc.Body,
                rc.FilePath,
                rc.LineNumber,
                rc.CreatedAt,
                AuthorName = _dbContext.Users
                    .Where(u => u.Id == rc.AuthorId)
                    .Select(u => u.Username)
                    .FirstOrDefault() ?? string.Empty,
                PrNumber = _dbContext.PullRequests
                    .Where(pr => pr.Id == rc.PullRequestId)
                    .Select(pr => pr.PrNumber)
                    .FirstOrDefault(),
                RepositoryName = _dbContext.PullRequests
                    .Where(pr => pr.Id == rc.PullRequestId)
                    .Select(pr => _dbContext.Repositories
                        .Where(r => r.Id == pr.RepositoryId)
                        .Select(r => r.FullName)
                        .FirstOrDefault() ?? string.Empty)
                    .FirstOrDefault() ?? string.Empty
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (comment is null)
        {
            return false;
        }

        var document = new CodeReviewSearchDocument
        {
            Id = comment.Id.ToString(),
            PullRequestId = comment.PullRequestId,
            PrNumber = comment.PrNumber,
            RepositoryName = comment.RepositoryName,
            AuthorName = comment.AuthorName,
            CommentBody = comment.Body,
            FilePath = comment.FilePath,
            LineNumber = comment.LineNumber,
            CreatedAt = comment.CreatedAt
        };

        return await IndexDocumentAsync(document, _settings.ReviewsIndex, comment.Id, cancellationToken);
    }

    /// <summary>
    /// Writes a document under a deterministic <c>_id</c>.
    /// </summary>
    /// <remarks>
    /// The id is the entity's primary key, so redelivery of the same job overwrites
    /// the existing document instead of creating a duplicate. That is what makes
    /// at-least-once queue delivery safe here.
    /// </remarks>
    private async Task<bool> IndexDocumentAsync<TDocument>(
        TDocument document,
        string indexName,
        Guid documentId,
        CancellationToken cancellationToken)
        where TDocument : class
    {
        var response = await _client.IndexAsync(
            document,
            descriptor => descriptor.Index(indexName).Id(documentId.ToString()),
            cancellationToken);

        if (!response.IsValid)
        {
            throw new InvalidOperationException(
                $"Failed to index document {documentId} into '{indexName}': {response.DebugInformation}");
        }

        _logger.LogDebug("Indexed document {DocumentId} into {Index}.", documentId, indexName);

        return true;
    }
}
