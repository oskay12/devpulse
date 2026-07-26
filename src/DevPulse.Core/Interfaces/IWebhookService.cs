using DevPulse.Core.Enums;

namespace DevPulse.Core.Interfaces;

/// <summary>
/// Inbound webhook verification and ingestion.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Verifies the request signature against the repository's active tokens, then
    /// persists the event and enqueues follow-up jobs.
    /// </summary>
    /// <param name="provider">Which provider sent the request.</param>
    /// <param name="rawBody">
    /// The exact bytes received. Signatures cover the raw payload, so a
    /// re-serialised object would not verify.
    /// </param>
    /// <param name="signatureHeader">Provider signature header value, if present.</param>
    /// <param name="eventType">Provider event type header value, if present.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Identifier of the accepted event.</returns>
    /// <exception cref="Exceptions.WebhookAuthenticationException">
    /// Signature missing, malformed, or matching no active token.
    /// </exception>
    /// <exception cref="Exceptions.DomainValidationException">
    /// Payload could not be parsed or referenced an unknown repository.
    /// </exception>
    Task<Guid> ProcessAsync(
        RepositoryProvider provider,
        byte[] rawBody,
        string? signatureHeader,
        string? eventType,
        CancellationToken cancellationToken = default);
}
