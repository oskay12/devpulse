using DevPulse.Core.Enums;
using DevPulse.Core.Messages.Jobs;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Developer metric aggregation.
/// </summary>
/// <remarks>
/// Reads live in <see cref="IUserService"/> and <see cref="IRepositoryService"/>;
/// this interface owns the (expensive) recalculation performed by the Worker and
/// the API-side trigger that enqueues it.
/// </remarks>
public interface IMetricsService
{
    /// <summary>
    /// Recomputes and upserts <c>DeveloperMetric</c> rows described by the job.
    /// </summary>
    /// <returns>Number of metric rows written.</returns>
    Task<int> RecalculateAsync(CalculateMetricsJob job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues a recalculation instead of running it inline — aggregating across a
    /// large repository is far too slow for a request/response cycle.
    /// </summary>
    /// <returns>The job that was queued.</returns>
    Task<CalculateMetricsJob> EnqueueRecalculationAsync(
        Guid? userId,
        Guid? repositoryId,
        MetricPeriodType periodType,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default);
}
