namespace DevPulse.Core.Exceptions;

/// <summary>
/// A downstream dependency (OpenSearch, RabbitMQ) could not be reached. Maps to
/// HTTP 503 so callers and load balancers can retry, rather than a 500 that
/// looks like a bug in this service.
/// </summary>
public sealed class DependencyUnavailableException : DevPulseException
{
    /// <summary>Initialises the exception for the named dependency.</summary>
    public DependencyUnavailableException(string dependencyName, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        DependencyName = dependencyName;
    }

    /// <summary>Name of the unavailable dependency, e.g. "OpenSearch".</summary>
    public string DependencyName { get; }
}
