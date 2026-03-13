using FacturApp.Data;
using FacturApp.Repositories;
using FacturApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace FacturApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // ─── Base de datos PostgreSQL ──────────────────────────────────────
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(Config.AppConfig.ConnectionString));

        // ─── Repositories ──────────────────────────────────────────────────
        builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
        builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
        builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();

        // ─── Azure Services ────────────────────────────────────────────────
        builder.Services.AddScoped<IAzureSpeechService, AzureSpeechService>();
        builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();
        builder.Services.AddScoped<IDocumentIntelligenceService, DocumentIntelligenceService>();
        builder.Services.AddScoped<IQuestPdfService, QuestPdfService>();
        builder.Services.AddScoped<IInvoiceOrchestrationService, InvoiceOrchestrationService>();

        // ─── RabbitMQ (singleton: una sola conexión) ───────────────────────
        builder.Services.AddSingleton<AppSettingsService>();
        builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

        // ─── Flow Service (reemplaza al ApiService HTTP) ───────────────────
        builder.Services.AddScoped<InvoiceFlowService>();

        // ─── Audio (micrófono) ─────────────────────────────────────────────
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddScoped<AudioRecorderService>();

        // ─── ViewModels ────────────────────────────────────────────────────
        builder.Services.AddScoped<ViewModels.InvoiceViewModel>();
        builder.Services.AddScoped<ViewModels.DocumentViewModel>();
        builder.Services.AddScoped<ViewModels.InvoiceListViewModel>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Crear tablas si no existen al iniciar
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        return app;
    }
}