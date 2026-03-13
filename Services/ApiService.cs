using FacturApp.DTOs;
using FacturApp.Repositories;
using Microsoft.Extensions.Logging;

namespace FacturApp.Services;

/// <summary>
/// Servicio de alto nivel que usan directamente los ViewModels.
/// Combina audio/documento → Azure → orquestación → DTO
/// Reemplaza al antiguo ApiService que hacía HTTP a la API externa.
/// </summary>
public class InvoiceFlowService(
    IAzureSpeechService speechService,
    IAzureOpenAIService openAIService,
    IDocumentIntelligenceService documentService,
    IInvoiceOrchestrationService orchestration,
    IFacturaRepository facturaRepo,
    IClienteRepository clienteRepo,
    ILogger<InvoiceFlowService> logger)
{
    // ─── Flujo VOZ ────────────────────────────────────────────────────────────

    public async Task<(string Transcripcion, string? Error)> TranscribeAudioAsync(
        byte[] audioBytes, string fileName, CancellationToken ct = default)
    {
        // 1. Transcripción
        string transcripcion;
        try
        {
            using var ms = new MemoryStream(audioBytes);
            var ext = Path.GetExtension(fileName).TrimStart('.').ToLower();
            transcripcion = await speechService.TranscribeAsync(ms, ext, ct);
            logger.LogInformation("Transcripción: {T}", transcripcion);
        }
        catch (Exception ex)
        {
            return (string.Empty, $"Error en transcripción: {ex.Message}");
        }

        // Solo devolver la transcripción, la lógica de negocio la maneja ProcessChatAsync
        return (transcripcion, null);
    }

    public async Task<(string? TextReply, FacturaDto? Factura, string? Error)> ProcessChatAsync(
        List<ChatTurn> history, CancellationToken ct = default)
    {
        AppIntentResult intent;
        try
        {
            intent = await openAIService.DetectUserIntentAsync(history, ct);
        }
        catch (Exception ex)
        {
            return (null, null, $"Error analizando intención: {ex.Message}");
        }

        if (intent.Type == IntentType.TextReply)
        {
            return (intent.TextReply, null, null);
        }
        
        if (intent.Type == IntentType.AddClient && intent.Client != null)
        {
            try
            {
                var existing = await clienteRepo.SearchByNameAsync(intent.Client.Nombre, ct);
                if (existing != null)
                {
                    return ($"El cliente {intent.Client.Nombre} ya existe en la base de datos.", null, null);
                }
                
                await clienteRepo.AddAsync(new Models.Cliente 
                { 
                    Nombre = intent.Client.Nombre, 
                    Nif = intent.Client.Nif, 
                    Direccion = intent.Client.Direccion,
                    Email = intent.Client.Email
                }, ct);
                
                return ($"✅ Cliente {intent.Client.Nombre} añadido correctamente.", null, null);
            }
            catch (Exception ex)
            {
                return (null, null, $"Error al añadir cliente: {ex.Message}");
            }
        }

        if (intent.Type == IntentType.CreateInvoice && intent.Invoice != null)
        {
            try
            {
                var factura = await orchestration.CreateFromIntentAsync(intent.Invoice, "CHAT", ct);
                return (null, factura, null);
            }
            catch (Exception ex)
            {
                return (null, null, ex.Message);
            }
        }

        return (null, null, "No se detectó una intención válida ni respuesta del asistente.");
    }

    // ─── Flujo DOCUMENTO ──────────────────────────────────────────────────────

    public async Task<(FacturaDto? Factura, string? Error)> ProcessDocumentAsync(
        byte[] documentBytes, string fileName, string contentType, CancellationToken ct = default)
    {
        InvoiceIntent? intent;
        try
        {
            using var ms = new MemoryStream(documentBytes);
            intent = await documentService.AnalyzeInvoiceDocumentAsync(ms, contentType, ct);
        }
        catch (Exception ex)
        {
            return (null, $"Error en Document Intelligence: {ex.Message}");
        }

        if (intent == null || string.IsNullOrWhiteSpace(intent.ClienteNombre))
            return (null, "No se pudieron extraer datos del documento.");

        try
        {
            var factura = await orchestration.CreateFromIntentAsync(intent, "DOCUMENTO", ct);
            return (factura, null);
        }
        catch (Exception ex)
        {
            return (null, ex.Message);
        }
    }

    // ─── Listado ──────────────────────────────────────────────────────────────

    public async Task<List<FacturaDto>> GetFacturasAsync(CancellationToken ct = default)
    {
        var facturas = await facturaRepo.GetAllAsync(ct);
        return facturas.Select(InvoiceOrchestrationService.MapToDto).ToList();
    }

    public async Task<byte[]?> GetPdfBytesAsync(int facturaId, CancellationToken ct = default)
    {
        var factura = await facturaRepo.GetByIdAsync(facturaId, ct);
        if (factura?.PdfBase64 == null) return null;
        return Convert.FromBase64String(factura.PdfBase64);
    }
}
