using System.Text;
using System.Text.Json;
using FacturApp.Config;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FacturApp.Services;

public interface IRabbitMQService
{
    bool IsConnected { get; }
    string? LastErrorMessage { get; }
    Task ReconnectAsync();
    Task PublishAsync<T>(string routingKey, T message, CancellationToken ct = default);
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private const string Exchange = "facturapp";
    public bool IsConnected { get; private set; }
    public string? LastErrorMessage { get; private set; }

    private bool _disposed;
    private readonly ILogger<RabbitMQService> _logger;
    private readonly AppSettingsService _settings;

    public RabbitMQService(ILogger<RabbitMQService> logger, AppSettingsService settings)
    {
        _logger = logger;
        _settings = settings;
        _ = ReconnectAsync();
    }

    public async Task ReconnectAsync()
    {
        _logger.LogInformation("Intentando conectar a RabbitMQ...");
        
        IsConnected = false;
        LastErrorMessage = null;

        // Limpiar conexión previa si existe
        try
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch { /* ignore */ }

        try
        {
            var host = _settings.RabbitMQHost;
            // Sanitizar: si el usuario puso algo como "localhost:5672", quitar el puerto
            if (host.Contains(':'))
            {
                host = host.Split(':')[0];
            }

            _logger.LogInformation("Conectando a RabbitMQ en {H}:5672...", host);

            var factory = new ConnectionFactory
            {
                HostName = host,
                UserName = _settings.RabbitMQUser,
                Password = _settings.RabbitMQPassword,
                Port = 5672,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                AutomaticRecoveryEnabled = true
            };
            
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            
            await _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
            await _channel.QueueDeclareAsync("invoice.created", durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync("invoice.created", Exchange, "invoice.created");
            
            IsConnected = true;
            _logger.LogInformation("✅ RabbitMQ conectado exitosamente.");
        }
        catch (Exception ex)
        {
            var friendlyMessage = ex.Message switch
            {
                var m when m.Contains("None of the specified endpoints were reachable") => "No se pudo alcanzar el servidor RabbitMQ. Verifica la dirección IP y que el contenedor Docker esté corriendo.",
                var m when m.Contains("Access refused") || m.Contains("ACCESS_REFUSED") => "Acceso denegado. Verifica el usuario y la contraseña.",
                var m when m.Contains("Connection timed out") => "Tiempo de conexión agotado. Revisa tu red o firewall.",
                _ => ex.Message
            };

            LastErrorMessage = friendlyMessage + (ex.InnerException != null ? " (Detalle: " + ex.InnerException.Message + ")" : "");
            _logger.LogError(ex, "⚠️ Error al conectar a RabbitMQ");
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
