using DevPulse.Core.Interfaces;
using DevPulse.Core.Messages.Jobs;
using DevPulse.Core.Settings;
using DevPulse.Infrastructure.Messaging;
using Microsoft.Extensions.Options;

namespace DevPulse.Worker.Consumers;

/// <summary>
/// Projects entities into OpenSearch in response to indexing jobs.
/// </summary>
internal sealed class IndexContentConsumer : RabbitMqConsumerBase<IndexContentJob>
{
    private readonly ILogger<IndexContentConsumer> _logger;

    public IndexContentConsumer(
        RabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqSettings> settings,
        ILogger<IndexContentConsumer> logger)
        : base(connectionProvider, scopeFactory, logger)
    {
        QueueName = settings.Value.Queues.SearchIndexing;
        _logger = logger;
    }

    protected override string QueueName { get; }

    protected override async Task HandleAsync(
        IndexContentJob message,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var indexService = services.GetRequiredService<ISearchIndexService>();

        var indexed = await indexService.IndexAsync(
            message.ContentType, message.EntityId, cancellationToken);

        if (indexed)
        {
            _logger.LogDebug(
                "Indexed {ContentType} {EntityId} (job {JobId}).",
                message.ContentType, message.EntityId, message.JobId);
        }
        else
        {
            // Deleted between enqueue and consumption. Nothing to index and nothing
            // to retry — acknowledging is correct.
            _logger.LogInformation(
                "Skipped job {JobId}: {ContentType} {EntityId} no longer exists.",
                message.JobId, message.ContentType, message.EntityId);
        }
    }
}
