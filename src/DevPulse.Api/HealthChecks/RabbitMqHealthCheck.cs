using DevPulse.Core.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DevPulse.Api.HealthChecks;

/// <summary>
/// Reports whether the message broker can be reached.
/// </summary>
/// <remarks>
/// The API only publishes, so this checks that a connection can be established
/// rather than inspecting queue depth — an unreachable broker is precisely what
/// would make webhook ingestion fail.
/// </remarks>
internal sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IMessageBrokerProbe _probe;

    public RabbitMqHealthCheck(IMessageBrokerProbe probe)
    {
        _probe = probe;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _probe.IsReachableAsync(cancellationToken)
                ? HealthCheckResult.Healthy("RabbitMQ connection is open.")
                : new HealthCheckResult(context.Registration.FailureStatus, "RabbitMQ connection is closed.");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                "Could not connect to RabbitMQ.",
                ex);
        }
    }
}
