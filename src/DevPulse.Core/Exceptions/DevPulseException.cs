namespace DevPulse.Core.Exceptions;

/// <summary>
/// Base type for errors that carry a deliberate HTTP outcome. Anything deriving
/// from this is treated as an expected failure and mapped to a specific status
/// code; every other exception becomes a 500 with no detail leaked to callers.
/// </summary>
public abstract class DevPulseException : Exception
{
    /// <summary>Initialises the exception with a message.</summary>
    protected DevPulseException(string message)
        : base(message)
    {
    }

    /// <summary>Initialises the exception with a message and inner exception.</summary>
    protected DevPulseException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
