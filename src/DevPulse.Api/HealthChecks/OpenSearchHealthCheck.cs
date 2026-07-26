using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenSearch.Client;

namespace DevPulse.Api.HealthChecks;

/// <summary>
/// Reports whether the OpenSearch cluster is reachable.
/// </summary>
/// <remarks>
/// Uses a ping rather than a cluster-health call on purpose: cluster health returns
/// yellow whenever replicas are unassigned, which is the normal steady state on a
/// single-node development cluster and would make readiness flap for no reason.
/// </remarks>
internal sealed class OpenSearchHealthCheck : IHealthCheck
{
    private readonly IOpenSearchClient _client;

    public OpenSearchHealthCheck(IOpenSearchClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.PingAsync(ct: cancellationToken);

            return response.IsValid
                ? HealthCheckResult.Healthy("OpenSearch is reachable.")
                : new HealthCheckResult(
                    context.Registration.FailureStatus,
                    "OpenSearch ping failed.",
                    response.OriginalException);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                "OpenSearch ping threw.",
                ex);
        }
    }
}
