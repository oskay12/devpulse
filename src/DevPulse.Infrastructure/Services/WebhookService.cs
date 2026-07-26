using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DevPulse.Core.Enums;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using DevPulse.Core.Messages;
using DevPulse.Core.Settings;
using DevPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DevPulse.Infrastructure.Services;

/// <inheritdoc cref="IWebhookService"/>
internal sealed class WebhookService : IWebhookService
{
    private const string GitHubSignaturePrefix = "sha256=";

    private readonly ApplicationDbContext _dbContext;
    private readonly IProjectTokenService _projectTokenService;
    private readonly IMessagePublisher _publisher;
    private readonly WebhookSettings _webhookSettings;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        ApplicationDbContext dbContext,
        IProjectTokenService projectTokenService,
        IMessagePublisher publisher,
        IOptions<WebhookSettings> webhookSettings,
        IOptions<RabbitMqSettings> rabbitMqSettings,
        ILogger<WebhookService> logger)
    {
        _dbContext = dbContext;
        _projectTokenService = projectTokenService;
        _publisher = publisher;
        _webhookSettings = webhookSettings.Value;
        _rabbitMqSettings = rabbitMqSettings.Value;
        _logger = logger;
    }

    public async Task<Guid> ProcessAsync(
        RepositoryProvider provider,
        byte[] rawBody,
        string? signatureHeader,
        string? eventType,
        CancellationToken cancellationToken = default)
    {
        var (repositoryId, tokenId) = provider switch
        {
            RepositoryProvider.GitHub =>
                (await VerifyGitHubAsync(rawBody, signatureHeader, cancellationToken), (Guid?)null),
            RepositoryProvider.GitLab =>
                await VerifyGitLabAsync(signatureHeader, cancellationToken),
            _ => throw new DomainValidationException($"Provider '{provider}' is not supported.")
        };

        if (tokenId.HasValue)
        {
            await _projectTokenService.TouchAsync(tokenId.Value, cancellationToken);
        }

        var payload = Encoding.UTF8.GetString(rawBody);
        var eventId = Guid.CreateVersion7();

        var pushEvent = TryBuildPushEvent(
            provider, repositoryId, eventId, eventType, payload);

        if (pushEvent is null)
        {
            // Accepted and verified, but nothing downstream consumes this event type
            // yet. Reporting success is honest: the caller's request was valid.
            _logger.LogInformation(
                "Webhook {EventId} of type '{EventType}' from {Provider} accepted but not queued.",
                eventId, eventType, provider);

            await TouchRepositoryAsync(repositoryId, cancellationToken);

            return eventId;
        }

        await _publisher.PublishAsync(
            _rabbitMqSettings.Queues.WebhookEvents, pushEvent, cancellationToken);

        await TouchRepositoryAsync(repositoryId, cancellationToken);

        _logger.LogInformation(
            "Queued webhook {EventId} ({CommitCount} commit(s)) for repository {RepositoryId}.",
            eventId, pushEvent.Commits.Count, repositoryId);

        return eventId;
    }

    /// <summary>
    /// Verifies the GitHub HMAC signature, then resolves the repository from the
    /// payload. Resolution happens only after verification, so an unauthenticated
    /// caller cannot use this endpoint to probe which repositories exist.
    /// </summary>
    private async Task<Guid> VerifyGitHubAsync(
        byte[] rawBody,
        string? signatureHeader,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_webhookSettings.GitHubSecret))
        {
            // Fail closed: an unconfigured secret must not mean "accept anything".
            throw new WebhookAuthenticationException(
                "Webhooks:GitHubSecret is not configured; refusing to accept unverified GitHub webhooks.");
        }

        if (string.IsNullOrWhiteSpace(signatureHeader)
            || !signatureHeader.StartsWith(GitHubSignaturePrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new WebhookAuthenticationException("Missing or malformed X-Hub-Signature-256 header.");
        }

        var presented = signatureHeader[GitHubSignaturePrefix.Length..];

        byte[] presentedBytes;
        try
        {
            presentedBytes = Convert.FromHexString(presented);
        }
        catch (FormatException)
        {
            throw new WebhookAuthenticationException("X-Hub-Signature-256 is not valid hex.");
        }

        var expected = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_webhookSettings.GitHubSecret),
            rawBody);

        // Constant-time: a byte-by-byte comparison would leak how much of the
        // signature was correct and let an attacker forge one byte at a time.
        if (!CryptographicOperations.FixedTimeEquals(expected, presentedBytes))
        {
            throw new WebhookAuthenticationException("X-Hub-Signature-256 did not match.");
        }

        return await ResolveRepositoryFromPayloadAsync(
            RepositoryProvider.GitHub, rawBody, cancellationToken);
    }

    /// <summary>
    /// Verifies the GitLab token. The token itself identifies the repository, so the
    /// payload is never consulted for authentication.
    /// </summary>
    private async Task<(Guid RepositoryId, Guid? TokenId)> VerifyGitLabAsync(
        string? tokenHeader,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tokenHeader))
        {
            throw new WebhookAuthenticationException("Missing X-Gitlab-Token header.");
        }

        var token = await _projectTokenService.FindActiveByTokenAsync(tokenHeader, cancellationToken)
            ?? throw new WebhookAuthenticationException("X-Gitlab-Token did not match an active token.");

        if (!token.Permissions.HasFlag(TokenPermission.WriteWebhooks))
        {
            throw new WebhookAuthenticationException(
                $"Token {token.Id} lacks the WriteWebhooks permission.");
        }

        return (token.RepositoryId, token.Id);
    }

    private async Task<Guid> ResolveRepositoryFromPayloadAsync(
        RepositoryProvider provider,
        byte[] rawBody,
        CancellationToken cancellationToken)
    {
        using var document = Parse(rawBody);
        var root = document.RootElement;

        string? externalId = null;
        string? fullName = null;

        if (root.TryGetProperty("repository", out var repository))
        {
            externalId = ReadId(repository, "id");
            fullName = repository.TryGetProperty("full_name", out var name) ? name.GetString() : null;
        }

        if (externalId is null && fullName is null)
        {
            throw new DomainValidationException("Payload does not identify a repository.");
        }

        var resolved = await _dbContext.Repositories
            .AsNoTracking()
            .Where(r => r.Provider == provider
                        && ((externalId != null && r.ExternalId == externalId)
                            || (fullName != null && r.FullName == fullName)))
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return resolved
               ?? throw new DomainValidationException(
                   $"Repository '{fullName ?? externalId}' is not registered in DevPulse.");
    }

    private PushWebhookEvent? TryBuildPushEvent(
        RepositoryProvider provider,
        Guid repositoryId,
        Guid eventId,
        string? eventType,
        string payload)
    {
        var isPush = provider switch
        {
            RepositoryProvider.GitHub => string.Equals(eventType, "push", StringComparison.OrdinalIgnoreCase),
            RepositoryProvider.GitLab => eventType is null
                                         || eventType.Contains("push", StringComparison.OrdinalIgnoreCase),
            _ => false
        };

        if (!isPush)
        {
            return null;
        }

        using var document = Parse(Encoding.UTF8.GetBytes(payload));
        var root = document.RootElement;

        if (!root.TryGetProperty("commits", out var commits) || commits.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var branch = root.TryGetProperty("ref", out var reference)
            ? BranchFromRef(reference.GetString())
            : string.Empty;

        return new PushWebhookEvent
        {
            EventId = eventId,
            EventType = "push",
            RepositoryId = repositoryId,
            Provider = provider,
            ReceivedAt = DateTime.UtcNow,
            RawPayload = payload,
            Branch = branch,
            // The pusher is a provider identity, not a DevPulse user; the Worker
            // matches commit authors by email instead.
            PushedById = Guid.Empty,
            Commits = commits.EnumerateArray().Select(ReadCommit).ToList()
        };
    }

    private static CommitPayload ReadCommit(JsonElement element)
    {
        var author = element.TryGetProperty("author", out var authorElement)
            ? authorElement
            : default;

        return new CommitPayload
        {
            Sha = ReadString(element, "id") ?? ReadString(element, "sha") ?? string.Empty,
            Message = ReadString(element, "message") ?? string.Empty,
            AuthorName = author.ValueKind == JsonValueKind.Object
                ? ReadString(author, "name") ?? string.Empty
                : string.Empty,
            AuthorEmail = author.ValueKind == JsonValueKind.Object
                ? ReadString(author, "email") ?? string.Empty
                : string.Empty,
            Timestamp = ReadTimestamp(element, "timestamp"),
            AddedFiles = ReadStringArray(element, "added"),
            ModifiedFiles = ReadStringArray(element, "modified"),
            RemovedFiles = ReadStringArray(element, "removed")
        };
    }

    private async Task TouchRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken)
        => await _dbContext.Repositories
            .Where(r => r.Id == repositoryId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(r => r.LastSyncedAt, DateTime.UtcNow),
                cancellationToken);

    private static JsonDocument Parse(byte[] rawBody)
    {
        try
        {
            return JsonDocument.Parse(rawBody);
        }
        catch (JsonException ex)
        {
            throw new DomainValidationException("Webhook payload is not valid JSON.", ex);
        }
    }

    private static string BranchFromRef(string? reference) =>
        string.IsNullOrEmpty(reference)
            ? string.Empty
            : reference.StartsWith("refs/heads/", StringComparison.Ordinal)
                ? reference["refs/heads/".Length..]
                : reference;

    /// <summary>Reads an id that providers send as either a number or a string.</summary>
    private static string? ReadId(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.String => value.GetString(),
                _ => null
            }
            : null;

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static DateTime ReadTimestamp(JsonElement element, string propertyName)
    {
        var raw = ReadString(element, propertyName);

        return DateTimeOffset.TryParse(raw, out var parsed)
            ? parsed.UtcDateTime
            : DateTime.UtcNow;
    }

    private static List<string> ReadStringArray(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .ToList()
            : [];
}
