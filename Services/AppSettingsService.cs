using FacturApp.Config;
using Microsoft.Maui.Storage;

namespace FacturApp.Services;

public class AppSettingsService
{
    private const string RabbitMQHostKey = "rabbitmq_host";
    private const string RabbitMQUserKey = "rabbitmq_user";
    private const string RabbitMQPasswordKey = "rabbitmq_password";

    // Azure Keys
    private const string SpeechKeyKey = "azure_speech_key";
    private const string SpeechRegionKey = "azure_speech_region";
    private const string OpenAIEndpointKey = "azure_openai_endpoint";
    private const string OpenAIKeyKey = "azure_openai_key";
    private const string OpenAIDeploymentKey = "azure_openai_deployment";
    private const string DocIntelligenceEndpointKey = "azure_docinfo_endpoint";
    private const string DocIntelligenceKeyKey = "azure_docinfo_key";

    public string RabbitMQHost
    {
        get => Preferences.Default.Get(RabbitMQHostKey, AppConfig.RabbitMQHost);
        set => Preferences.Default.Set(RabbitMQHostKey, value);
    }

    public string RabbitMQUser
    {
        get => Preferences.Default.Get(RabbitMQUserKey, AppConfig.RabbitMQUser);
        set => Preferences.Default.Set(RabbitMQUserKey, value);
    }

    public string RabbitMQPassword
    {
        get => Preferences.Default.Get(RabbitMQPasswordKey, AppConfig.RabbitMQPassword);
        set => Preferences.Default.Set(RabbitMQPasswordKey, value);
    }

    // Azure Speech
    public string SpeechKey
    {
        get => Preferences.Default.Get(SpeechKeyKey, AppConfig.SpeechKey);
        set => Preferences.Default.Set(SpeechKeyKey, value);
    }

    public string SpeechRegion
    {
        get => Preferences.Default.Get(SpeechRegionKey, AppConfig.SpeechRegion);
        set => Preferences.Default.Set(SpeechRegionKey, value);
    }

    // Azure OpenAI
    public string OpenAIEndpoint
    {
        get => Preferences.Default.Get(OpenAIEndpointKey, AppConfig.OpenAIEndpoint);
        set => Preferences.Default.Set(OpenAIEndpointKey, value);
    }

    public string OpenAIKey
    {
        get => Preferences.Default.Get(OpenAIKeyKey, AppConfig.OpenAIKey);
        set => Preferences.Default.Set(OpenAIKeyKey, value);
    }

    public string OpenAIDeployment
    {
        get => Preferences.Default.Get(OpenAIDeploymentKey, AppConfig.OpenAIDeployment);
        set => Preferences.Default.Set(OpenAIDeploymentKey, value);
    }

    // Azure Document Intelligence
    public string DocIntelligenceEndpoint
    {
        get => Preferences.Default.Get(DocIntelligenceEndpointKey, AppConfig.DocIntelligenceEndpoint);
        set => Preferences.Default.Set(DocIntelligenceEndpointKey, value);
    }

    public string DocIntelligenceKey
    {
        get => Preferences.Default.Get(DocIntelligenceKeyKey, AppConfig.DocIntelligenceKey);
        set => Preferences.Default.Set(DocIntelligenceKeyKey, value);
    }

    public void SaveRabbitMQ(string host, string user, string password)
    {
        RabbitMQHost = host;
        RabbitMQUser = user;
        RabbitMQPassword = password;
    }

    public void SaveAzureSettings(string sKey, string sRegion, string oEndpoint, string oKey, string oDeploy, string dEndpoint, string dKey)
    {
        SpeechKey = sKey;
        SpeechRegion = sRegion;
        OpenAIEndpoint = oEndpoint;
        OpenAIKey = oKey;
        OpenAIDeployment = oDeploy;
        DocIntelligenceEndpoint = dEndpoint;
        DocIntelligenceKey = dKey;
    }
}
