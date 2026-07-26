using DevPulse.Api.ErrorHandling;
using DevPulse.Api.HealthChecks;
using DevPulse.Core.Settings;
using DevPulse.Infrastructure.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDevPulseOptions(builder.Configuration);
builder.Services.AddDevPulsePersistence();
builder.Services.AddDevPulseSearch();
builder.Services.AddDevPulseMessaging();
builder.Services.AddDevPulseApplicationServices();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionStringFactory: serviceProvider =>
            serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value.ConnectionString,
        name: "postgres",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"])
    .AddCheck<OpenSearchHealthCheck>(
        "opensearch",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"])
    .AddCheck<RabbitMqHealthCheck>(
        "rabbitmq",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready"]);

var app = builder.Build();

// ValidateOnStart() would otherwise run inside app.Run(), i.e. after the
// migration below. Forcing it here means a bad connection string or a missing
// OpenSearch password fails immediately instead of after a database round trip.
app.Services.GetRequiredService<IStartupValidator>().Validate();

// Applied under an advisory lock so the two API replicas cannot race. Can be
// turned off to run schema changes from a dedicated job instead of on startup.
if (builder.Configuration.GetValue("Database:AutoMigrate", true))
{
    await app.Services.MigrateDevPulseDatabaseAsync();
}

// First in the pipeline: also catches exceptions thrown by later middleware.
app.UseExceptionHandler();

// Off by default. The API sits behind an internal ClusterIP service, but the
// schema still describes every endpoint, so exposing it stays an explicit choice.
if (builder.Configuration.GetValue("Swagger:Enabled", app.Environment.IsDevelopment()))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Liveness deliberately checks nothing but the process itself: a database blip
// must not restart otherwise-healthy pods. Readiness is what pulls a pod out of
// the load balancer while a dependency is down.
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });

app.MapControllers();

app.Run();
