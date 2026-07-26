namespace DevPulse.Core.Interfaces;

/// <summary>
/// Liveness probe for the message broker, used by the readiness health check.
/// </summary>
/// <remarks>
/// Exists so the API can report broker reachability without taking a dependency on
/// the broker client library or on Infrastructure's internal types.
/// </remarks>
public interface IMessageBrokerProbe
{
    /// <summary>Returns whether the broker is currently reachable.</summary>
    Task<bool> IsReachableAsync(CancellationToken cancellationToken = default);
}
