namespace FacturApp.Config;

/// <summary>
/// Configuración centralizada con todas las claves hardcodeadas.
/// Sustituye appsettings.json al no haber API separada.
/// </summary>
public static class AppConfig
{
    // ─── Azure Speech ─────────────────────────────────────────────────────────
    public const string SpeechKey = "aqui_va_tu_key";
    public const string SpeechRegion = "francecentral";

    // ─── Azure OpenAI ─────────────────────────────────────────────────────────
    public const string OpenAIEndpoint = "aqui_va_tu_endpoint";
    public const string OpenAIKey = "aqui_va_tu_key";
    public const string OpenAIDeployment = "gpt-4o";

    // ─── Azure Document Intelligence ──────────────────────────────────────────
    public const string DocIntelligenceEndpoint = "aqui_va_tu_endpoint";
    public const string DocIntelligenceKey = "aqui_va_tu_key";

    // ─── PostgreSQL ───────────────────────────────────────────────────────────
    // En Windows dev: localhost. En Android emulador: 10.0.2.2
    // En Android físico: usa la IP local de tu PC (ej. 192.168.1.100) o URL ngrok
#if ANDROID
    public const string LocalHostIp = "192.168.5.94";
#else
    public const string LocalHostIp = "127.0.0.1";
#endif

    public const string ConnectionString =
        $"Host={LocalHostIp};Port=5432;Database=facturacion_db;Username=admin;Password=superpassword123";

    // ─── RabbitMQ ─────────────────────────────────────────────────────────────
    public const string RabbitMQHost = LocalHostIp;
    public const string RabbitMQUser = "admin";
    public const string RabbitMQPassword = "superpassword123";
}
