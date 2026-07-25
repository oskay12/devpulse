namespace DevPulse.Core.Settings;

/// <summary>
/// RabbitMQ queue names.
/// </summary>
public class QueueSettings
{
    /// <summary>Queue for webhook events</summary>
    [JsonPropertyName("webhook_events")]
    public string WebhookEvents { get; set; } = "devpulse.webhook.events";

    /// <summary>Queue for metrics calculation jobs</summary>
    [JsonPropertyName("metrics_calculation")]
    public string MetricsCalculation { get; set; } = "devpulse.metrics.calculate";

    /// <summary>Queue for OpenSearch indexing jobs</summary>
    [JsonPropertyName("search_indexing")]
    public string SearchIndexing { get; set; } = "devpulse.search.index";

    /// <summary>Queue for image optimization jobs</summary>
    [JsonPropertyName("image_optimization")]
    public string ImageOptimization { get; set; } = "devpulse.media.optimize";
}
