using RabbitMQ.Client;

namespace DevPulse.Infrastructure.Messaging;

/// <summary>
/// Declares the queue topology.
/// </summary>
/// <remarks>
/// Both the publisher and the consumer declare queues before use, and RabbitMQ
/// rejects a redeclaration whose arguments differ from the existing queue with
/// PRECONDITION_FAILED. Keeping the declaration in one place is what stops the two
/// sides from drifting apart.
/// </remarks>
internal static class RabbitMqTopology
{
    /// <summary>Exchange that dead-lettered messages are routed to.</summary>
    public const string DeadLetterExchange = "devpulse.dlx";

    /// <summary>Suffix appended to a queue name to form its dead-letter queue.</summary>
    public const string DeadLetterSuffix = ".dead";

    /// <summary>
    /// Declares a durable work queue together with its dead-letter queue.
    /// </summary>
    public static async Task DeclareQueueAsync(
        IChannel channel,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        var deadLetterQueue = queueName + DeadLetterSuffix;

        await channel.ExchangeDeclareAsync(
            DeadLetterExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            deadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            deadLetterQueue,
            DeadLetterExchange,
            deadLetterQueue,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                // A message that fails processing is routed here instead of being
                // requeued forever, so one poison payload cannot stall the queue.
                ["x-dead-letter-exchange"] = DeadLetterExchange,
                ["x-dead-letter-routing-key"] = deadLetterQueue
            },
            cancellationToken: cancellationToken);
    }
}
