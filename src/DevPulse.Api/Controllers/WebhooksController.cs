using DevPulse.Core.Enums;
using DevPulse.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevPulse.Api.Controllers;

/// <summary>
/// Inbound webhook receivers for GitHub and GitLab.
/// </summary>
/// <remarks>
/// These are the only authenticated endpoints in this service. Verified events are
/// queued and processed by the Worker, so the response returns as soon as the
/// payload is accepted rather than waiting for ingestion.
/// </remarks>
[ApiController]
[Route("api/webhooks")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
public sealed class WebhooksController : ControllerBase
{
    /// <summary>Guards against an oversized body being buffered into memory.</summary>
    private const int MaxPayloadBytes = 5 * 1024 * 1024;

    private readonly IWebhookService _webhookService;

    /// <summary>Initialises the controller.</summary>
    public WebhooksController(IWebhookService webhookService)
    {
        _webhookService = webhookService;
    }

    /// <summary>Receives a GitHub webhook.</summary>
    /// <remarks>
    /// Requires a valid <c>X-Hub-Signature-256</c> HMAC computed with the configured
    /// webhook secret over the exact request body.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The accepted event identifier.</returns>
    [HttpPost("github", Name = "ReceiveGitHubWebhook")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status413PayloadTooLarge)]
    public Task<IActionResult> ReceiveGitHub(CancellationToken cancellationToken)
        => ReceiveAsync(
            RepositoryProvider.GitHub,
            "X-Hub-Signature-256",
            "X-GitHub-Event",
            cancellationToken);

    /// <summary>Receives a GitLab webhook.</summary>
    /// <remarks>
    /// Requires an <c>X-Gitlab-Token</c> matching an active project token that
    /// carries the <c>WriteWebhooks</c> permission. The token also determines which
    /// repository the event belongs to.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The accepted event identifier.</returns>
    [HttpPost("gitlab", Name = "ReceiveGitLabWebhook")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status413PayloadTooLarge)]
    public Task<IActionResult> ReceiveGitLab(CancellationToken cancellationToken)
        => ReceiveAsync(
            RepositoryProvider.GitLab,
            "X-Gitlab-Token",
            "X-Gitlab-Event",
            cancellationToken);

    private async Task<IActionResult> ReceiveAsync(
        RepositoryProvider provider,
        string signatureHeaderName,
        string eventHeaderName,
        CancellationToken cancellationToken)
    {
        // Signatures cover the bytes as sent. Reading the raw stream rather than a
        // bound model is what makes verification possible at all — re-serialising a
        // deserialised object would produce different bytes and never match.
        var rawBody = await ReadBodyAsync(cancellationToken);

        if (rawBody is null)
        {
            return Problem(
                statusCode: StatusCodes.Status413PayloadTooLarge,
                title: "Payload too large",
                detail: $"Webhook payloads are limited to {MaxPayloadBytes} bytes.");
        }

        var signature = Request.Headers[signatureHeaderName].FirstOrDefault();
        var eventType = Request.Headers[eventHeaderName].FirstOrDefault();

        var eventId = await _webhookService.ProcessAsync(
            provider, rawBody, signature, eventType, cancellationToken);

        return Accepted(new { event_id = eventId });
    }

    private async Task<byte[]?> ReadBodyAsync(CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;

        while ((read = await Request.Body.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaxPayloadBytes)
            {
                return null;
            }

            buffer.Write(chunk, 0, read);
        }

        return buffer.ToArray();
    }
}
