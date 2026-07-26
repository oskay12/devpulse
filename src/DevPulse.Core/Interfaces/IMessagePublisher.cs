namespace DevPulse.Core.Interfaces;

/// <summary>
/// Publishes job messages onto the broker.
/// </summary>
/// <remarks>
/// Kept deliberately narrow so the API never references RabbitMQ types directly;
/// swapping transports is an Infrastructure concern.
/// </remarks>
public interface IMessagePublisher
{
    /// <summary>
    /// Serialises <paramref name="message"/> and publishes it to
    /// <paramref name="queueName"/> as a persistent message.
    /// </summary>
    /// <exception cref="Exceptions.DependencyUnavailableException">
    /// The broker could not be reached.
    /// </exception>
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
        where T : class;
}
