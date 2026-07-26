namespace DevPulse.Core.Settings;

/// <summary>
/// RabbitMQ message broker configuration.
/// </summary>
public class RabbitMqSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "RabbitMq";

    /// <summary>RabbitMQ hostname (K8s service name)</summary>
    [JsonPropertyName("host_name")]
    [Required]
    public string HostName { get; set; } = string.Empty;

    /// <summary>RabbitMQ port</summary>
    [JsonPropertyName("port")]
    public int Port { get; set; } = 5672;

    /// <summary>Authentication username</summary>
    [JsonPropertyName("username")]
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>Authentication password</summary>
    [JsonPropertyName("password")]
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>Virtual host</summary>
    [JsonPropertyName("virtual_host")]
    public string VirtualHost { get; set; } = "/";

    /// <summary>Queue name configuration</summary>
    [JsonPropertyName("queues")]
    public QueueSettings Queues { get; set; } = new();
}
