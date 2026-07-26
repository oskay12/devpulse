namespace DevPulse.Core.Exceptions;

/// <summary>
/// The request conflicts with existing state, e.g. registering a repository
/// whose <c>FullName</c> is already taken. Maps to HTTP 409.
/// </summary>
public sealed class ConflictException : DevPulseException
{
    /// <summary>Initialises the exception with a message.</summary>
    public ConflictException(string message)
        : base(message)
    {
    }

    /// <summary>Initialises the exception with a message and inner exception.</summary>
    public ConflictException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
