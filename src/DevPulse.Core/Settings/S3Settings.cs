namespace DevPulse.Core.Settings;

/// <summary>
/// AWS S3 storage configuration.
/// Values are sourced from Environment Variables, K8s Secrets, or AWS Parameter Store — never hardcoded.
/// </summary>
public class S3Settings
{
    /// <summary>S3 bucket name</summary>
    [JsonPropertyName("bucket_name")]
    [Required]
    public string BucketName { get; set; } = string.Empty;

    /// <summary>AWS region</summary>
    [JsonPropertyName("region")]
    [Required]
    public string Region { get; set; } = "us-east-1";

    /// <summary>AWS access key ID</summary>
    [JsonPropertyName("access_key_id")]
    [Required]
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>AWS secret access key</summary>
    [JsonPropertyName("secret_access_key")]
    [Required]
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>CloudFront distribution URL (nullable)</summary>
    [JsonPropertyName("cloudfront_distribution")]
    [Url]
    public string? CloudFrontDistribution { get; set; }
}
