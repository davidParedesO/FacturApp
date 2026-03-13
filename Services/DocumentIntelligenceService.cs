using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using FacturApp.Config;
using Microsoft.Extensions.Logging;

namespace FacturApp.Services;

public interface IDocumentIntelligenceService
{
    Task<InvoiceIntent?> AnalyzeInvoiceDocumentAsync(Stream documentStream, string contentType, CancellationToken ct = default);
}

public class DocumentIntelligenceService(ILogger<DocumentIntelligenceService> logger) : IDocumentIntelligenceService
{
    public async Task<InvoiceIntent?> AnalyzeInvoiceDocumentAsync(Stream documentStream, string contentType, CancellationToken ct = default)
    {
        logger.LogInformation("Analizando documento con Azure Document Intelligence...");

        var client = new DocumentAnalysisClient(
            new Uri(AppConfig.DocIntelligenceEndpoint),
            new AzureKeyCredential(AppConfig.DocIntelligenceKey));

        var operation = await client.AnalyzeDocumentAsync(
            WaitUntil.Completed, "prebuilt-invoice", documentStream, cancellationToken: ct);

        var result = operation.Value;
        if (!result.Documents.Any()) return null;

        var doc = result.Documents[0];
        var clienteNombre = string.Empty;
        var items = new List<InvoiceItemIntent>();

        if (doc.Fields.TryGetValue("CustomerName", out var field))
            clienteNombre = field.Content ?? string.Empty;

        if (doc.Fields.TryGetValue("Items", out var itemsField) && itemsField.FieldType == DocumentFieldType.List)
        {
            foreach (var item in itemsField.Value.AsList())
            {
                if (item.FieldType != DocumentFieldType.Dictionary) continue;
                var dict = item.Value.AsDictionary();
                var nombre = string.Empty;
                var cantidad = 1;
                if (dict.TryGetValue("Description", out var desc)) nombre = desc.Content ?? string.Empty;
                if (dict.TryGetValue("Quantity", out var qty) && qty.FieldType == DocumentFieldType.Double)
                    cantidad = (int)qty.Value.AsDouble();
                if (!string.IsNullOrWhiteSpace(nombre))
                    items.Add(new InvoiceItemIntent(nombre, cantidad));
            }
        }

        logger.LogInformation("Documento analizado: Cliente={C}, Items={N}", clienteNombre, items.Count);
        return new InvoiceIntent(clienteNombre, items);
    }
}
