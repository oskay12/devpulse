namespace DevPulse.Core.Exceptions;

/// <summary>
/// An inbound webhook failed signature or token verification. Maps to HTTP 401.
/// </summary>
/// <remarks>
/// The message is intentionally coarse. Telling a caller <em>why</em> verification
/// failed (unknown repository vs. bad signature vs. revoked token) hands an
/// attacker a probing oracle, so the detail is logged and never returned.
/// </remarks>
public sealed class WebhookAuthenticationException : DevPulseException
{
    /// <summary>Initialises the exception with an internal-only reason.</summary>
    public WebhookAuthenticationException(string internalReason)
        : base(internalReason)
    {
    }
}
