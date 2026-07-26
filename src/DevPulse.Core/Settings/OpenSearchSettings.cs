using DevPulse.Core.Enums;

namespace DevPulse.Core.Settings;

/// <summary>
/// OpenSearch cluster configuration.
/// </summary>
public class OpenSearchSettings : IValidatableObject
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "OpenSearch";

    /// <summary>OpenSearch cluster endpoint URL</summary>
    [JsonPropertyName("endpoint")]
    [Required]
    [Url]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// How requests are authenticated. Amazon OpenSearch Service with an
    /// internal user database uses <see cref="OpenSearchAuthMode.BasicAuth"/>.
    /// </summary>
    [JsonPropertyName("auth_mode")]
    public OpenSearchAuthMode AuthMode { get; set; } = OpenSearchAuthMode.BasicAuth;

    /// <summary>
    /// Authentication username. Required when <see cref="AuthMode"/> is
    /// <see cref="OpenSearchAuthMode.BasicAuth"/>, otherwise ignored.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Authentication password. Required when <see cref="AuthMode"/> is
    /// <see cref="OpenSearchAuthMode.BasicAuth"/>, otherwise ignored.
    /// </summary>
    [JsonPropertyName("password")]
    public string? Password { get; set; }

    /// <summary>
    /// AWS region used to sign requests. Required when <see cref="AuthMode"/>
    /// is <see cref="OpenSearchAuthMode.AwsSigV4"/>, otherwise ignored.
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    /// <summary>Request timeout in seconds</summary>
    [JsonPropertyName("request_timeout_seconds")]
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>Commits index name</summary>
    [JsonPropertyName("commits_index")]
    [Required]
    public string CommitsIndex { get; set; } = "devpulse-commits";

    /// <summary>Pull requests index name</summary>
    [JsonPropertyName("pull_requests_index")]
    [Required]
    public string PullRequestsIndex { get; set; } = "devpulse-pull-requests";

    /// <summary>Reviews index name</summary>
    [JsonPropertyName("reviews_index")]
    [Required]
    public string ReviewsIndex { get; set; } = "devpulse-reviews";

    /// <summary>
    /// Enforces the credential requirements that vary by <see cref="AuthMode"/>.
    /// Runs at startup via <c>ValidateOnStart</c>, so a misconfigured pod fails
    /// to boot instead of failing on the first search request.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        switch (AuthMode)
        {
            case OpenSearchAuthMode.BasicAuth:
                if (string.IsNullOrWhiteSpace(Username))
                {
                    yield return new ValidationResult(
                        $"{nameof(Username)} is required when {nameof(AuthMode)} is {nameof(OpenSearchAuthMode.BasicAuth)}.",
                        [nameof(Username)]);
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    yield return new ValidationResult(
                        $"{nameof(Password)} is required when {nameof(AuthMode)} is {nameof(OpenSearchAuthMode.BasicAuth)}.",
                        [nameof(Password)]);
                }

                break;

            case OpenSearchAuthMode.AwsSigV4:
                if (string.IsNullOrWhiteSpace(Region))
                {
                    yield return new ValidationResult(
                        $"{nameof(Region)} is required when {nameof(AuthMode)} is {nameof(OpenSearchAuthMode.AwsSigV4)}.",
                        [nameof(Region)]);
                }

                break;
        }
    }
}
