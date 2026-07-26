namespace DevPulse.Core.Exceptions;

/// <summary>
/// A requested resource does not exist. Maps to HTTP 404.
/// </summary>
public sealed class NotFoundException : DevPulseException
{
    /// <summary>Initialises the exception with a message.</summary>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initialises the exception for a resource identified by a key, e.g.
    /// <c>new NotFoundException("Repository", id)</c>.
    /// </summary>
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} '{key}' was not found.")
    {
        ResourceName = resourceName;
        Key = key;
    }

    /// <summary>Name of the resource type that was not found.</summary>
    public string? ResourceName { get; }

    /// <summary>Key that was looked up.</summary>
    public object? Key { get; }
}
