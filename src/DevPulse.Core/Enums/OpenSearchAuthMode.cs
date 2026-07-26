namespace DevPulse.Core.Enums;

/// <summary>
/// Authentication mechanism used when connecting to the OpenSearch cluster.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OpenSearchAuthMode
{
    /// <summary>HTTP basic authentication (Amazon OpenSearch internal user database).</summary>
    [JsonPropertyName("basic_auth")]
    BasicAuth = 0,

    /// <summary>AWS SigV4 request signing against an IAM-authenticated domain.</summary>
    [JsonPropertyName("aws_sig_v4")]
    AwsSigV4 = 1,

    /// <summary>
    /// No authentication. Only valid against a cluster running with the security
    /// plugin disabled, e.g. a local development container.
    /// </summary>
    [JsonPropertyName("none")]
    None = 2
}
