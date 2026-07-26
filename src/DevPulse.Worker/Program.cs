using DevPulse.Infrastructure.Extensions;
using DevPulse.Worker;
using DevPulse.Worker.Consumers;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDevPulseOptions(builder.Configuration);
builder.Services.AddDevPulsePersistence();
builder.Services.AddDevPulseSearch();
builder.Services.AddDevPulseMessaging();
builder.Services.AddDevPulseApplicationServices();

// The Worker owns index creation rather than the API: it runs a single replica, so
// there is no startup race, and it is the only process that writes to the indices.
builder.Services.AddHostedService<SearchIndexBootstrapper>();

builder.Services.AddHostedService<WebhookEventConsumer>();
builder.Services.AddHostedService<IndexContentConsumer>();
builder.Services.AddHostedService<CalculateMetricsConsumer>();

var host = builder.Build();

// Fail on bad configuration before any consumer starts pulling messages.
host.Services.GetRequiredService<IStartupValidator>().Validate();

host.Run();
