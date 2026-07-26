using DevPulse.Core.Exceptions;
using DevPulse.Core.Interfaces;
using DevPulse.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DevPulse.Infrastructure.Messaging;

/// <summary>
/// Owns the single shared RabbitMQ connection.
/// </summary>
/// <remarks>
/// A connection is expensive (one TCP socket plus a heartbeat) and is thread-safe,
/// so it is shared process-wide; channels, which are not thread-safe, are created
/// per unit of work. Automatic recovery is on, but the connection is still
/// re-established here if it is found closed, which covers the case where the
/// broker was unreachable at startup.
/// </remarks>
internal sealed class RabbitMqConnectionProvider : IMessageBrokerProbe, IAsyncDisposable
{
    private const string DependencyName = "RabbitMQ";

    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqConnectionProvider> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnectionProvider(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqConnectionProvider> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>Returns the shared connection, opening it if necessary.</summary>
    /// <exception cref="DependencyUnavailableException">The broker is unreachable.</exception>
    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _gate.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            _logger.LogInformation(
                "Connected to RabbitMQ at {Host}:{Port}{VirtualHost}.",
                _settings.HostName,
                _settings.Port,
                _settings.VirtualHost);

            return _connection;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new DependencyUnavailableException(
                DependencyName,
                $"Could not connect to RabbitMQ at {_settings.HostName}:{_settings.Port}.",
                ex);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsReachableAsync(CancellationToken cancellationToken = default)
    {
        var connection = await GetConnectionAsync(cancellationToken);

        return connection.IsOpen;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        _gate.Dispose();
    }
}
