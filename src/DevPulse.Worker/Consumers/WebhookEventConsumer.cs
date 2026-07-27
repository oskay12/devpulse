using DevPulse.Core.Entities;
using DevPulse.Core.Enums;
using DevPulse.Core.Interfaces;
using DevPulse.Core.Messages;
using DevPulse.Core.Messages.Jobs;
using DevPulse.Core.Settings;
using DevPulse.Infrastructure.Data;
using DevPulse.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DevPulse.Worker.Consumers;

/// <summary>
/// Turns verified push events into commit rows, then queues them for indexing.
/// </summary>
/// <remarks>
/// This is the ingestion half of the pipeline: the API verifies and queues, the
/// Worker writes. Splitting it this way keeps webhook responses fast even when a
/// push carries hundreds of commits.
/// </remarks>
internal sealed class WebhookEventConsumer : RabbitMqConsumerBase<PushWebhookEvent>
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<WebhookEventConsumer> _logger;

    public WebhookEventConsumer(
        RabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<WebhookEventConsumer> logger)
        : base(connectionProvider, scopeFactory, logger)
    {
        _settings = settings.Value;
        QueueName = _settings.Queues.WebhookEvents;
        _logger = logger;
    }

    protected override string QueueName { get; }

    protected override async Task HandleAsync(
        PushWebhookEvent message,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var publisher = services.GetRequiredService<IMessagePublisher>();
        var statsProvider = services.GetRequiredService<ICommitStatsProvider>();

        if (message.Commits.Count == 0)
        {
            _logger.LogInformation("Push event {EventId} carried no commits.", message.EventId);
            return;
        }

        var shas = message.Commits
            .Select(c => c.Sha)
            .Where(sha => !string.IsNullOrWhiteSpace(sha))
            .Distinct()
            .ToList();

        // One query for what already exists rather than a lookup per commit. The
        // (RepositoryId, Sha) unique index means redelivery must not insert twice.
        var existingShas = await dbContext.Commits
            .AsNoTracking()
            .Where(c => c.RepositoryId == message.RepositoryId && shas.Contains(c.Sha))
            .Select(c => c.Sha)
            .ToListAsync(cancellationToken);

        var known = existingShas.ToHashSet(StringComparer.Ordinal);

        // Only fetched when there is at least one new commit to enrich, and only
        // for GitHub — GitLab pushes carry no stats provider today (see
        // ServiceCollectionExtensions.AddDevPulseCommitStatsProviders).
        var repositoryFullName = message.Provider == RepositoryProvider.GitHub
            ? await dbContext.Repositories
                .AsNoTracking()
                .Where(r => r.Id == message.RepositoryId)
                .Select(r => r.FullName)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var emails = message.Commits
            .Select(c => c.AuthorEmail)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct()
            .ToList();

        // Commit authors are Git identities; match them to DevPulse users by email
        // so metrics can attribute the work. Unmatched authors stay anonymous.
        var usersByEmail = await dbContext.Users
            .AsNoTracking()
            .Where(u => emails.Contains(u.Email))
            .ToDictionaryAsync(u => u.Email, u => u.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var newCommitIds = new List<Guid>();

        foreach (var payload in message.Commits)
        {
            if (string.IsNullOrWhiteSpace(payload.Sha) || !known.Add(payload.Sha))
            {
                continue;
            }

            var commitId = Guid.CreateVersion7();
            var files = BuildFiles(commitId, payload);

            // Best-effort enrichment: a null result (no token configured, 404, rate
            // limited, network error) just leaves additions/deletions at zero rather
            // than failing ingestion of the commit itself.
            var stats = repositoryFullName is not null
                ? await statsProvider.GetCommitStatsAsync(repositoryFullName, payload.Sha, cancellationToken)
                : null;

            if (stats is not null)
            {
                ApplyFileStats(files, stats.Files);
            }

            usersByEmail.TryGetValue(payload.AuthorEmail ?? string.Empty, out var authorId);

            dbContext.Commits.Add(new Commit
            {
                Id = commitId,
                RepositoryId = message.RepositoryId,
                Sha = payload.Sha,
                AuthorId = authorId == Guid.Empty ? null : authorId,
                AuthorName = Truncate(payload.AuthorName, 200),
                AuthorEmail = payload.AuthorEmail ?? string.Empty,
                Message = payload.Message ?? string.Empty,
                Branch = Truncate(message.Branch, 200),
                CommittedAt = payload.Timestamp,
                IndexedAt = DateTime.UtcNow,
                FilesChanged = files.Count,
                Additions = stats?.Additions ?? 0,
                Deletions = stats?.Deletions ?? 0,
                ParentSha = null
            });

            dbContext.CommitFiles.AddRange(files);
            newCommitIds.Add(commitId);
        }

        if (newCommitIds.Count == 0)
        {
            _logger.LogInformation(
                "Push event {EventId}: all {Count} commit(s) already ingested.",
                message.EventId, message.Commits.Count);
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Push event {EventId}: ingested {Count} new commit(s) for repository {RepositoryId}.",
            message.EventId, newCommitIds.Count, message.RepositoryId);

        // Queued after the transaction commits, so the indexer can always find the
        // row it is asked to index.
        foreach (var commitId in newCommitIds)
        {
            await publisher.PublishAsync(
                _settings.Queues.SearchIndexing,
                new IndexContentJob
                {
                    JobId = Guid.CreateVersion7(),
                    ContentType = IndexContentType.Commit,
                    EntityId = commitId,
                    QueuedAt = DateTime.UtcNow
                },
                cancellationToken);
        }
    }

    private static List<CommitFile> BuildFiles(Guid commitId, CommitPayload payload)
    {
        var files = new List<CommitFile>();

        void Add(IEnumerable<string> paths, FileChangeType changeType)
        {
            foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                files.Add(new CommitFile
                {
                    Id = Guid.CreateVersion7(),
                    CommitId = commitId,
                    FilePath = Truncate(path, 500),
                    ChangeType = changeType,
                    Additions = 0,
                    Deletions = 0,
                    DiffSnippet = null
                });
            }
        }

        // Push payloads list changed paths but carry no per-file line counts; those
        // come from the provider's commit API instead, applied afterwards by
        // ApplyFileStats when that call succeeds.
        Add(payload.AddedFiles, FileChangeType.Added);
        Add(payload.ModifiedFiles, FileChangeType.Modified);
        Add(payload.RemovedFiles, FileChangeType.Deleted);

        return files;
    }

    /// <summary>
    /// Copies provider-reported per-file line counts onto the matching
    /// <see cref="CommitFile"/> rows built from the push payload, matched by path.
    /// </summary>
    /// <remarks>
    /// Matched by path rather than index: the provider's file list can differ in
    /// order or, for renames, in count from the push payload's added/modified/
    /// removed lists. A path with no match (e.g. a rename the push payload didn't
    /// carry) is simply left at zero rather than guessed.
    /// </remarks>
    private static void ApplyFileStats(List<CommitFile> files, IReadOnlyList<CommitFileStats> fileStats)
    {
        if (fileStats.Count == 0)
        {
            return;
        }

        var statsByPath = fileStats
            .GroupBy(f => f.Path, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        foreach (var file in files)
        {
            if (statsByPath.TryGetValue(file.FilePath, out var match))
            {
                file.Additions = match.Additions;
                file.Deletions = match.Deletions;
            }
        }
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
