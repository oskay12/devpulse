using DevPulse.Core.Interfaces;
using DevPulse.Core.Settings;
using DevPulse.Infrastructure.Data;
using DevPulse.Infrastructure.Messaging;
using DevPulse.Infrastructure.Search;
using DevPulse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenSearch.Client;

namespace DevPulse.Infrastructure.Extensions;

/// <summary>
/// Shared composition root for the API and the Worker, so both hosts wire up
/// persistence, search and messaging identically.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds and validates every settings section. Validation runs at startup
    /// rather than on first use: a pod with a missing connection string or
    /// OpenSearch password fails to boot, and the rolling update
    /// (<c>maxUnavailable: 0</c>) keeps the previous pods serving traffic.
    /// </summary>
    public static IServiceCollection AddDevPulseOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DatabaseSettings>()
            .Bind(configuration.GetSection(DatabaseSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OpenSearchSettings>()
            .Bind(configuration.GetSection(OpenSearchSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RabbitMqSettings>()
            .Bind(configuration.GetSection(RabbitMqSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Not validated on start: an installation that only receives GitLab webhooks
        // never sets a GitHub secret. Requests are rejected at verification time.
        services.AddOptions<WebhookSettings>()
            .Bind(configuration.GetSection(WebhookSettings.SectionName));

        return services;
    }

    /// <summary>
    /// Registers the RabbitMQ connection and publisher.
    /// </summary>
    public static IServiceCollection AddDevPulseMessaging(this IServiceCollection services)
    {
        // One connection per process; channels are created per publish/consume.
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddSingleton<IMessageBrokerProbe>(sp => sp.GetRequiredService<RabbitMqConnectionProvider>());
        services.AddScoped<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }

    /// <summary>
    /// Registers <see cref="ApplicationDbContext"/> against PostgreSQL, applying
    /// the retry and timeout values from <see cref="DatabaseSettings"/>.
    /// </summary>
    public static IServiceCollection AddDevPulsePersistence(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

            options.UseNpgsql(settings.ConnectionString, npgsql =>
            {
                // RDS fails over and drops connections; retry transient errors
                // instead of surfacing them as 500s.
                npgsql.EnableRetryOnFailure(settings.MaxRetryAttempts);
                npgsql.CommandTimeout(settings.CommandTimeoutSeconds);
            });
        });

        return services;
    }

    /// <summary>
    /// Registers the OpenSearch client and the search services.
    /// </summary>
    /// <remarks>
    /// The client is a singleton: it is thread-safe and owns an internal connection
    /// pool, so creating one per request would waste sockets.
    /// </remarks>
    public static IServiceCollection AddDevPulseSearch(this IServiceCollection services)
    {
        services.AddSingleton<IOpenSearchClient>(serviceProvider =>
            OpenSearchClientFactory.Create(
                serviceProvider.GetRequiredService<IOptions<OpenSearchSettings>>().Value));

        services.AddScoped<OpenSearchIndexInitializer>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<ISearchIndexService, SearchIndexService>();

        return services;
    }

    /// <summary>
    /// Registers the domain services. Scoped, because they depend on the scoped
    /// <see cref="ApplicationDbContext"/>.
    /// </summary>
    public static IServiceCollection AddDevPulseApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryService, RepositoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICommitService, CommitService>();
        services.AddScoped<IPullRequestService, PullRequestService>();
        services.AddScoped<IProjectTokenService, ProjectTokenService>();
        services.AddScoped<IMetricsService, MetricsService>();
        services.AddScoped<IWebhookService, WebhookService>();

        return services;
    }
}
