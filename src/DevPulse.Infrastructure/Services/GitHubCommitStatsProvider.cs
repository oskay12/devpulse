using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DevPulse.Core.Enums;
using DevPulse.Core.Interfaces;
using DevPulse.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevPulse.Infrastructure.Services;

/// <summary>
/// Fetches per-commit line-change stats from the GitHub REST API.
/// </summary>
/// <remarks>
/// Called once per newly-ingested commit, after the push payload itself is
/// persisted. Every failure mode (no token configured, network error, 404, rate
/// limit) is swallowed and logged rather than thrown — this is an enrichment step,
/// not part of the ingestion guarantee, and <see cref="RabbitMqConsumerBase{TMessage}"/>
/// dead-letters on any unhandled exception from a consumer.
/// </remarks>
public sealed class GitHubCommitStatsProvider : ICommitStatsProvider
{
    // GitHub's JSON uses lowercase field names (e.g. "additions", "filename"); the
    // response DTOs below are PascalCase for readability, so case-insensitive
    // matching is required rather than a naming policy translation.
    private static readonly JsonSerializerOptions ResponseJsonOptions =
        new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly WebhookSettings _settings;
    private readonly ILogger<GitHubCommitStatsProvider> _logger;

    public GitHubCommitStatsProvider(
        HttpClient httpClient,
        IOptions<WebhookSettings> settings,
        ILogger<GitHubCommitStatsProvider> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public RepositoryProvider Provider => RepositoryProvider.GitHub;

    public async Task<CommitStats?> GetCommitStatsAsync(
        string repositoryFullName,
        string sha,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.GitHubApiToken))
        {
            // Not configured: stats stay zero rather than the Worker crash-looping
            // on every push. Logged once per call at Debug so it doesn't drown real
            // errors when this is intentionally left unset (see WebhookSettings).
            _logger.LogDebug("GitHub API token not configured; skipping commit stats fetch.");
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, $"repos/{repositoryFullName}/commits/{sha}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.GitHubApiToken);
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("DevPulse", "1.0"));

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "GitHub commit stats request for {Repo}@{Sha} returned {StatusCode}.",
                    repositoryFullName, sha, (int)response.StatusCode);
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<GitHubCommitResponse>(
                ResponseJsonOptions, cancellationToken);

            if (payload?.Stats is null)
            {
                return null;
            }

            var files = (payload.Files ?? [])
                .Where(f => !string.IsNullOrWhiteSpace(f.Filename))
                .Select(f => new CommitFileStats(f.Filename!, f.Additions, f.Deletions))
                .ToList();

            return new CommitStats(payload.Stats.Additions, payload.Stats.Deletions, files);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(
                ex, "Failed to fetch GitHub commit stats for {Repo}@{Sha}.", repositoryFullName, sha);
            return null;
        }
    }

    private sealed class GitHubCommitResponse
    {
        public GitHubCommitStats? Stats { get; set; }
        public List<GitHubCommitFile>? Files { get; set; }
    }

    private sealed class GitHubCommitStats
    {
        public int Additions { get; set; }
        public int Deletions { get; set; }
    }

    private sealed class GitHubCommitFile
    {
        public string? Filename { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
    }
}
