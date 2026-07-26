using System.Text.Json;
using DevPulse.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DevPulse.Worker.Consumers;

/// <summary>
/// Base class for queue consumers: owns the connection lifecycle, message
/// deserialisation, per-message scoping and acknowledgement.
/// </summary>
/// <typeparam name="TMessage">Message contract carried by the queue.</typeparam>
internal abstract class RabbitMqConsumerBase<TMessage> : BackgroundService
    where TMessage : class
{
    private const ushort PrefetchCount = 10;

    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    protected RabbitMqConsumerBase(
        RabbitMqConnectionProvider connectionProvider,
        IServiceScopeFactory scopeFactory,
        ILogger logger)
    {
        _connectionProvider = connectionProvider;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Queue this consumer subscribes to.</summary>
    protected abstract string QueueName { get; }

    /// <summary>
    /// Handles one message. Throwing dead-letters the message; returning normally
    /// acknowledges it.
    /// </summary>
    /// <param name="message">Deserialised message.</param>
    /// <param name="services">A fresh scope, so a scoped DbContext can be resolved.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    protected abstract Task HandleAsync(
        TMessage message,
        IServiceProvider services,
        CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                // The broker may simply not be up yet — the Worker and RabbitMQ roll
                // out together. Retry rather than crash-looping the pod.
                _logger.LogError(
                    ex,
                    "Consumer for {Queue} stopped unexpectedly; reconnecting in 5s.",
                    QueueName);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var connection = await _connectionProvider.GetConnectionAsync(stoppingToken);

        await using var channel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await RabbitMqTopology.DeclareQueueAsync(channel, QueueName, stoppingToken);

        // Bounded prefetch: without it the broker pushes the whole queue at this
        // consumer and a single Worker pod would hold every message hostage.
        await channel.BasicQosAsync(0, PrefetchCount, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, args) => OnReceivedAsync(channel, args, stoppingToken);

        await channel.BasicConsumeAsync(
            QueueName,
            autoAck: false,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Consuming {Queue} (prefetch {Prefetch}).", QueueName, PrefetchCount);

        // Hold the channel open until shutdown; delivery happens on the callback.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnReceivedAsync(
        IChannel channel,
        BasicDeliverEventArgs args,
        CancellationToken stoppingToken)
    {
        TMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<TMessage>(args.Body.Span, DevPulseJson.Options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Discarding malformed message from {Queue} (delivery {DeliveryTag}).",
                QueueName,
                args.DeliveryTag);

            // Unparseable now means unparseable forever — dead-letter it rather than
            // requeue, which would spin on the same bytes indefinitely.
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            return;
        }

        if (message is null)
        {
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            await HandleAsync(message, scope.ServiceProvider, stoppingToken);

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process message from {Queue} (delivery {DeliveryTag}); dead-lettering.",
                QueueName,
                args.DeliveryTag);

            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, stoppingToken);
        }
    }
}
