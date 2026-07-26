namespace DevPulse.Core.Settings;

/// <summary>
/// PostgreSQL database connection settings.
/// Loaded from appsettings.json or environment variables.
/// </summary>
public class DatabaseSettings
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "DatabaseSettings";

    /// <summary>PostgreSQL connection string</summary>
    [JsonPropertyName("connection_string")]
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Max retry attempts for transient failures</summary>
    [JsonPropertyName("max_retry_attempts")]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Command timeout in seconds</summary>
    [JsonPropertyName("command_timeout_seconds")]
    public int CommandTimeoutSeconds { get; set; } = 30;
}
