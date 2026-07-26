using DevPulse.Core.Interfaces;
using DevPulse.Core.Messages.Jobs;
using DevPulse.Core.Settings;
using DevPulse.Infrastructure.Messaging;
using Microsoft.Extensions.Options;

namespace DevPulse.Worker.Consumers;

/// <summary>
/// Recomputes developer metric aggregates in response to calculation jobs.
/// </summary>
internal sealed class CalculateMetricsConsumer : RabbitMqConsumerBase<CalculateMetricsJob>
{
    private readonly ILogger<CalculateMetricsConsumer> _logger;

    public CalculateMetricsConsumer(
        RabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<CalculateMetricsConsumer> logger)
        : base(connectionProvider, scopeFactory, logger)
    {
        QueueName = settings.Value.Queues.MetricsCalculation;
        _logger = logger;
    }

    protected override string QueueName { get; }

    protected override async Task HandleAsync(
        CalculateMetricsJob message,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var metricsService = services.GetRequiredService<IMetricsService>();

        var written = await metricsService.RecalculateAsync(message, cancellationToken);

        _logger.LogInformation(
            "Metrics job {JobId} completed: {Count} row(s) written.", message.JobId, written);
    }
}
