using DevPulse.Core.Entities;
using DevPulse.Core.Enums;
using DevPulse.Core.Interfaces;
using DevPulse.Core.Messages.Jobs;
using DevPulse.Core.Settings;
using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevPulse.Infrastructure.Services;

/// <inheritdoc cref="IMetricsService"/>
internal sealed class MetricsService : IMetricsService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMessagePublisher _publisher;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(
        ApplicationDbContext dbContext,
        IMessagePublisher publisher,
        IOptions<RabbitMqSettings> rabbitMqSettings,
        ILogger<MetricsService> logger)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _rabbitMqSettings = rabbitMqSettings.Value;
        _logger = logger;
    }

    public async Task<CalculateMetricsJob> EnqueueRecalculationAsync(
        Guid? userId,
        Guid? repositoryId,
        MetricPeriodType periodType,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        var start = periodStart.HasValue
            ? ToUtc(periodStart.Value)
            : DefaultPeriodStart(periodType);
        var end = periodEnd.HasValue ? ToUtc(periodEnd.Value) : DateTime.UtcNow;

        var job = new CalculateMetricsJob
        {
            JobId = Guid.CreateVersion7(),
            UserId = userId,
            RepositoryId = repositoryId,
            PeriodType = periodType,
            PeriodStart = start,
            PeriodEnd = end,
            QueuedAt = DateTime.UtcNow
        };

        await _publisher.PublishAsync(
            _rabbitMqSettings.Queues.MetricsCalculation, job, cancellationToken);

        return job;
    }

    public async Task<int> RecalculateAsync(
        CalculateMetricsJob job,
        CancellationToken cancellationToken = default)
    {
        var start = ToUtc(job.PeriodStart);
        var end = ToUtc(job.PeriodEnd);

        // Everything below is aggregated by the database and grouped by user, so the
        // cost is a fixed handful of queries regardless of how many developers the
        // period covers.
        var commitStats = await CommitsInWindow(job, start, end)
            .Where(c => c.AuthorId != null)
            .GroupBy(c => c.AuthorId!.Value)
            .Select(g => new
            {
                UserId = g.Key,
                Commits = g.Count(),
                Additions = g.Sum(c => c.Additions),
                Deletions = g.Sum(c => c.Deletions)
            })
            .ToListAsync(cancellationToken);

        var pullRequestStats = await PullRequestsInWindow(job, start, end)
            .GroupBy(pr => pr.AuthorId)
            .Select(g => new
            {
                UserId = g.Key,
                PullRequests = g.Count(),
                AverageMergeHours = g
                    .Where(pr => pr.MergedAt != null)
                    .Average(pr => (double?)(pr.MergedAt!.Value - pr.CreatedAt).TotalHours)
            })
            .ToListAsync(cancellationToken);

        var reviewStats = await ReviewsInWindow(job, start, end)
            .GroupBy(x => x.ReviewerId)
            .Select(g => new
            {
                UserId = g.Key,
                Reviews = g.Count(),
                AverageReviewHours = g.Average(x => (double?)x.LatencyHours)
            })
            .ToListAsync(cancellationToken);

        var userIds = commitStats.Select(s => s.UserId)
            .Union(pullRequestStats.Select(s => s.UserId))
            .Union(reviewStats.Select(s => s.UserId))
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            _logger.LogInformation(
                "Metrics job {JobId} matched no activity between {Start:o} and {End:o}.",
                job.JobId, start, end);
            return 0;
        }

        // Load the existing rows for this slice in one query so the upsert below
        // does not issue a lookup per user.
        var existing = await _dbContext.DeveloperMetrics
            .Where(m => userIds.Contains(m.UserId)
                        && m.RepositoryId == job.RepositoryId
                        && m.PeriodType == job.PeriodType
                        && m.PeriodStart == start)
            .ToListAsync(cancellationToken);

        var written = 0;

        foreach (var userId in userIds)
        {
            var commits = commitStats.FirstOrDefault(s => s.UserId == userId);
            var pullRequests = pullRequestStats.FirstOrDefault(s => s.UserId == userId);
            var reviews = reviewStats.FirstOrDefault(s => s.UserId == userId);

            var linesAdded = commits?.Additions ?? 0;
            var linesDeleted = commits?.Deletions ?? 0;
            var commitCount = commits?.Commits ?? 0;

            var metric = existing.FirstOrDefault(m => m.UserId == userId);

            if (metric is null)
            {
                metric = new DeveloperMetric
                {
                    Id = Guid.CreateVersion7(),
                    UserId = userId,
                    RepositoryId = job.RepositoryId,
                    PeriodType = job.PeriodType,
                    PeriodStart = start
                };

                _dbContext.DeveloperMetrics.Add(metric);
            }

            metric.PeriodEnd = end;
            metric.TotalCommits = commitCount;
            metric.TotalPullRequests = pullRequests?.PullRequests ?? 0;
            metric.PullRequestsReviewed = reviews?.Reviews ?? 0;
            metric.LinesAdded = linesAdded;
            metric.LinesDeleted = linesDeleted;
            metric.AverageReviewTime = Round(reviews?.AverageReviewHours);
            metric.AveragePrMergeTime = Round(pullRequests?.AverageMergeHours);
            // Churn per commit: total lines touched divided by commits. Zero commits
            // means no churn rather than a division by zero.
            metric.CodeChurnRate = commitCount == 0
                ? 0m
                : Math.Round((decimal)(linesAdded + linesDeleted) / commitCount, 2);
            metric.CalculatedAt = DateTime.UtcNow;

            written++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Metrics job {JobId} wrote {Count} developer metric row(s).", job.JobId, written);

        return written;
    }

    private IQueryable<Commit> CommitsInWindow(CalculateMetricsJob job, DateTime start, DateTime end)
    {
        var query = _dbContext.Commits
            .AsNoTracking()
            .Where(c => c.CommittedAt >= start && c.CommittedAt <= end);

        if (job.UserId.HasValue)
        {
            query = query.Where(c => c.AuthorId == job.UserId.Value);
        }

        if (job.RepositoryId.HasValue)
        {
            query = query.Where(c => c.RepositoryId == job.RepositoryId.Value);
        }

        return query;
    }

    private IQueryable<PullRequest> PullRequestsInWindow(
        CalculateMetricsJob job,
        DateTime start,
        DateTime end)
    {
        var query = _dbContext.PullRequests
            .AsNoTracking()
            .Where(pr => pr.CreatedAt >= start && pr.CreatedAt <= end);

        if (job.UserId.HasValue)
        {
            query = query.Where(pr => pr.AuthorId == job.UserId.Value);
        }

        if (job.RepositoryId.HasValue)
        {
            query = query.Where(pr => pr.RepositoryId == job.RepositoryId.Value);
        }

        return query;
    }

    private IQueryable<ReviewLatency> ReviewsInWindow(
        CalculateMetricsJob job,
        DateTime start,
        DateTime end)
    {
        var query = _dbContext.PullRequestReviews
            .AsNoTracking()
            .Where(rv => rv.SubmittedAt >= start && rv.SubmittedAt <= end);

        if (job.UserId.HasValue)
        {
            query = query.Where(rv => rv.ReviewerId == job.UserId.Value);
        }

        var joined = query.Join(
            _dbContext.PullRequests.AsNoTracking(),
            rv => rv.PullRequestId,
            pr => pr.Id,
            (rv, pr) => new ReviewLatency
            {
                ReviewerId = rv.ReviewerId,
                RepositoryId = pr.RepositoryId,
                LatencyHours = (rv.SubmittedAt - pr.CreatedAt).TotalHours
            });

        if (job.RepositoryId.HasValue)
        {
            joined = joined.Where(x => x.RepositoryId == job.RepositoryId.Value);
        }

        return joined;
    }

    private static decimal Round(double? value) =>
        value.HasValue ? Math.Round((decimal)value.Value, 2) : 0m;

    private static DateTime DefaultPeriodStart(MetricPeriodType periodType)
    {
        var now = DateTime.UtcNow;

        return periodType switch
        {
            MetricPeriodType.Daily => now.Date,
            MetricPeriodType.Weekly => now.Date.AddDays(-7),
            MetricPeriodType.Monthly => now.Date.AddMonths(-1),
            MetricPeriodType.Quarterly => now.Date.AddMonths(-3),
            MetricPeriodType.Yearly => now.Date.AddYears(-1),
            _ => now.Date.AddMonths(-1)
        };
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    /// <summary>Projection carrying a review's latency alongside its repository.</summary>
    private sealed class ReviewLatency
    {
        public Guid ReviewerId { get; init; }

        public Guid RepositoryId { get; init; }

        public double LatencyHours { get; init; }
    }
}
