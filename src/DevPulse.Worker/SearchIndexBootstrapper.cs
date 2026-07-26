using DevPulse.Core.Interfaces;

namespace DevPulse.Worker;

/// <summary>
/// Creates the OpenSearch indices once at startup.
/// </summary>
/// <remarks>
/// Runs here rather than in the API because the Worker is single-replica and is the
/// only writer. A failure is logged but does not stop the host: the consumers should
/// still start, and indexing errors surface per message where they can be
/// dead-lettered instead of taking the whole pod down.
/// </remarks>
internal sealed class SearchIndexBootstrapper : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SearchIndexBootstrapper> _logger;

    public SearchIndexBootstrapper(
        IServiceScopeFactory scopeFactory,
        ILogger<SearchIndexBootstrapper> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var indexService = scope.ServiceProvider.GetRequiredService<ISearchIndexService>();

            await indexService.EnsureIndicesAsync(stoppingToken);

            _logger.LogInformation("OpenSearch indices are ready.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure OpenSearch indices exist.");
        }
    }
}
