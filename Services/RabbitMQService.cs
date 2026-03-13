using System.Text;
using System.Text.Json;
using FacturApp.Config;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FacturApp.Services;

public interface IRabbitMQService
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken ct = default);
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private const string Exchange = "facturapp";
    private bool _disposed;
    private readonly ILogger<RabbitMQService> _logger;

    public RabbitMQService(ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = AppConfig.RabbitMQHost,
                UserName = AppConfig.RabbitMQUser,
                Password = AppConfig.RabbitMQPassword,
                Port = 5672
            };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
            await _channel.QueueDeclareAsync("invoice.created", durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync("invoice.created", Exchange, "invoice.created");
            _logger.LogInformation("✅ RabbitMQ conectado.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("⚠️ RabbitMQ no disponible: {M}", ex.Message);
        }
    }

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken ct = default)
    {
        if (_channel == null) return;
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = new BasicProperties { Persistent = true, ContentType = "application/json" };
            await _channel.BasicPublishAsync(Exchange, routingKey, false, props, body, ct);
            _logger.LogInformation("📨 RabbitMQ publicado: {K}", routingKey);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error RabbitMQ"); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
