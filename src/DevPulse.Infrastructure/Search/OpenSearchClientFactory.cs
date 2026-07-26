using System.Text;
using Amazon;
using DevPulse.Core.Enums;
using DevPulse.Core.Settings;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Auth.AwsSigV4;

namespace DevPulse.Infrastructure.Search;

/// <summary>
/// Builds the configured <see cref="IOpenSearchClient"/>.
/// </summary>
internal static class OpenSearchClientFactory
{
    /// <summary>
    /// Creates a client for the configured cluster. Authentication is the only
    /// thing that varies between deployments, and it varies in exactly one place —
    /// the switch below.
    /// </summary>
    public static IOpenSearchClient Create(OpenSearchSettings settings)
    {
        var pool = new SingleNodeConnectionPool(new Uri(settings.Endpoint));

        var connectionSettings = settings.AuthMode == OpenSearchAuthMode.AwsSigV4
            ? new ConnectionSettings(
                pool,
                new AwsSigV4HttpConnection(RegionEndpoint.GetBySystemName(settings.Region)))
            : new ConnectionSettings(pool);

        connectionSettings = connectionSettings
            // Without this the client serialises members as PascalCase, because its
            // own serialiser ignores the System.Text.Json [JsonPropertyName]
            // attributes on the search document classes. Documents would then be
            // indexed under fields the queries never look at, and every search would
            // silently return nothing.
            .DefaultFieldNameInferrer(ToSnakeCase)
            .RequestTimeout(TimeSpan.FromSeconds(settings.RequestTimeoutSeconds));

        if (settings.AuthMode == OpenSearchAuthMode.BasicAuth)
        {
            connectionSettings = connectionSettings
                .BasicAuthentication(settings.Username, settings.Password);
        }

        return new OpenSearchClient(connectionSettings);
    }

    /// <summary>
    /// Converts a PascalCase member name to snake_case, matching the
    /// <c>[JsonPropertyName]</c> values declared on the search document types.
    /// </summary>
    internal static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var builder = new StringBuilder(name.Length + 8);

        for (var i = 0; i < name.Length; i++)
        {
            var current = name[i];

            if (char.IsUpper(current))
            {
                // Boundary when the previous character was lower-case or a digit
                // ("PrNumber" -> "pr_number"), or when an acronym run ends and a new
                // word begins ("HTTPCode" -> "http_code").
                var previousIsLowerOrDigit = i > 0 && (char.IsLower(name[i - 1]) || char.IsDigit(name[i - 1]));
                var endsAcronymRun = i > 0
                                     && char.IsUpper(name[i - 1])
                                     && i + 1 < name.Length
                                     && char.IsLower(name[i + 1]);

                if (previousIsLowerOrDigit || endsAcronymRun)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(current));
            }
            else
            {
                builder.Append(current);
            }
        }

        return builder.ToString();
    }
}
