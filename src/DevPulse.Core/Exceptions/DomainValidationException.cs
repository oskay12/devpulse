namespace DevPulse.Core.Exceptions;

/// <summary>
/// The request is well-formed but violates a business rule that model binding
/// cannot express. Maps to HTTP 400.
/// </summary>
public sealed class DomainValidationException : DevPulseException
{
    /// <summary>Initialises the exception with a message.</summary>
    public DomainValidationException(string message)
        : base(message)
    {
    }

    /// <summary>Initialises the exception with a message and inner exception.</summary>
    public DomainValidationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initialises the exception with per-field errors, surfaced to callers under
    /// the <c>errors</c> member of the problem response.
    /// </summary>
    public DomainValidationException(string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    /// <summary>Field-level errors, keyed by field name.</summary>
    public IDictionary<string, string[]>? Errors { get; }
}
