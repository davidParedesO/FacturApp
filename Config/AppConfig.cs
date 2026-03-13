namespace FacturApp.Config;

/// <summary>
/// Configuración centralizada con todas las claves hardcodeadas.
/// Sustituye appsettings.json al no haber API separada.
/// </summary>
public static class AppConfig
{
    // ─── Azure Speech ─────────────────────────────────────────────────────────
    public const string SpeechKey = "58NGtvhK8n9eYuoDaYKLnbH6is9wxI1RRxpBXOhca4N6umxEDkPWJQQJ99CCAC5T7U2XJ3w3AAAYACOGBAxv";
    public const string SpeechRegion = "francecentral";

    // ─── Azure OpenAI ─────────────────────────────────────────────────────────
    public const string OpenAIEndpoint = "https://davidopenia.openai.azure.com/";
    public const string OpenAIKey = "9drG8bViXw3Jy54BJFXBXhqCzxD0tDhrterTXvtB5O8s0p9ppxzHJQQJ99CBAC5T7U2XJ3w3AAABACOGN5Ec";
    public const string OpenAIDeployment = "gpt-4o";

    // ─── Azure Document Intelligence ──────────────────────────────────────────
    public const string DocIntelligenceEndpoint = "https://davidia.cognitiveservices.azure.com/";
    public const string DocIntelligenceKey = "EGIpQPkwUuCClrrm2x1v9pOTvfhONOhP2bQ6PsW7VbipXnGsv3T9JQQJ99CBAC5T7U2XJ3w3AAALACOG36VT";

    // ─── PostgreSQL ───────────────────────────────────────────────────────────
    // En Windows dev: localhost. En Android emulador: 10.0.2.2
    // En Android físico: usa la IP local de tu PC (ej. 192.168.1.100) o URL ngrok
#if ANDROID
    public const string LocalHostIp = "192.168.5.94";
#else
    public const string LocalHostIp = "localhost";
#endif

    public const string ConnectionString =
        $"Host={LocalHostIp};Port=5432;Database=facturacion_db;Username=admin;Password=superpassword123";

    // ─── RabbitMQ ─────────────────────────────────────────────────────────────
    public const string RabbitMQHost = LocalHostIp;
    public const string RabbitMQUser = "admin";
    public const string RabbitMQPassword = "superpassword123";
}
