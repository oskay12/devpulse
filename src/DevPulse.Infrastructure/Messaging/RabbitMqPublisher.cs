using System.Text.Json;
using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DevPulse.Infrastructure.Messaging;

/// <inheritdoc cref="IMessagePublisher"/>
internal sealed class RabbitMqPublisher : IMessagePublisher
{
    private const string DependencyName = "RabbitMQ";

    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(
        RabbitMqConnectionProvider connectionProvider,
        ILogger<RabbitMqPublisher> logger)
    {
        _connectionProvider = connectionProvider;
        _logger = logger;
    }

    public async Task PublishAsync<T>(
        string queueName,
        T message,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);

        try
        {
            // A channel per publish: channels are not thread-safe, and webhook
            // volume here is far below the point where the extra round trip matters.
            // Publisher confirmations mean this method does not return until the
            // broker has accepted the message.
            await using var channel = await connection.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);

            await RabbitMqTopology.DeclareQueueAsync(channel, queueName, cancellationToken);

            var body = JsonSerializer.SerializeToUtf8Bytes(message, DevPulseJson.Options);

            var properties = new BasicProperties
            {
                // Survives a broker restart, given the queue is durable too.
                Persistent = true,
                ContentType = "application/json",
                MessageId = Guid.CreateVersion7().ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Type = typeof(T).Name
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogDebug("Published {MessageType} to {Queue}.", typeof(T).Name, queueName);
        }
        catch (DependencyUnavailableException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new DependencyUnavailableException(
                DependencyName,
                $"Failed to publish {typeof(T).Name} to '{queueName}'.",
                ex);
        }
    }
}
