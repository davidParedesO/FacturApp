using FacturApp.Models;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;

namespace FacturApp.Services;

public interface IQuestPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(Factura factura);
}

public class QuestPdfService(ILogger<QuestPdfService> logger) : IQuestPdfService
{
    public Task<byte[]> GenerateInvoicePdfAsync(Factura factura)
    {
        logger.LogInformation("Generando PDF: {N}", factura.NumeroFactura);
        QuestPDF.Settings.License = LicenseType.Community;

        // Definición de strings con escape sequences para evitar problemas de codificación
        string strFacturacion = "Sistema de Facturaci\u00f3n";
        string strNumero = "N\u00ba " + factura.NumeroFactura;
        string strDescripcion = "Descripci\u00f3n";
        string strEuro = " \u20ac";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(11).FontColor("#374151"));

                // Header Premium
                page.Header().PaddingBottom(20).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FacturApp").Bold().FontSize(28).FontColor("#2563EB");
                        col.Item().Text(strFacturacion).FontSize(12).FontColor("#6B7280").Italic();
                    });
                    
                    row.ConstantItem(180).AlignRight().Column(col =>
                    {
                        col.Item().Text("FACTURA").ExtraBold().FontSize(24).FontColor("#111827");
                        col.Item().PaddingTop(2).Text(strNumero).SemiBold().FontSize(12).FontColor("#2563EB");
                        col.Item().Text($"Fecha: {factura.FechaEmision:dd/MM/yyyy}").FontSize(10).FontColor("#6B7280");
                    });
                });

                page.Content().Column(col =>
                {
                    // Separador sutil
                    col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#E5E7EB");

                    // Bloque de Información (Dos columnas)
                    col.Item().PaddingVertical(20).Row(row =>
                    {
                        // Datos del Emisor (Podrías hardcodear los tuyos si quisieras)
                        row.RelativeItem().Column(cc =>
                        {
                            cc.Item().Text("DE:").Bold().FontSize(9).FontColor("#9CA3AF");
                            cc.Item().Text("DAVID PAREDES").Bold().FontSize(12);
                            cc.Item().Text("Calle Universidad, 123");
                            cc.Item().Text("08001 Barcelona");
                            cc.Item().Text("NIF: 12345678Z");
                        });

                        // Datos del Cliente
                        row.RelativeItem().BorderLeft(1).BorderColor("#E5E7EB").PaddingLeft(20).Column(cc =>
                        {
                            cc.Item().Text("PARA:").Bold().FontSize(9).FontColor("#2563EB");
                            cc.Item().Text(factura.Cliente?.Nombre ?? "---").Bold().FontSize(14);
                            cc.Item().PaddingTop(4).Column(c =>
                            {
                                c.Item().Text($"NIF: {factura.Cliente?.Nif ?? "---"}");
                                c.Item().Text($"Email: {factura.Cliente?.Email ?? "---"}");
                                c.Item().Text($"Direcci\u00f3n: {factura.Cliente?.Direccion ?? "---"}");
                            });
                        });
                    });

                    // Tabla de Productos con Estilo Moderno
                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(5); // Descripción
                            cols.RelativeColumn(1); // Cant.
                            cols.RelativeColumn(2); // Precio
                            cols.RelativeColumn(2); // Subtotal
                        });

                        table.Header(h =>
                        {
                            h.Cell().Padding(10).Background("#F8FAFC").BorderBottom(2).BorderColor("#2563EB").Text(strDescripcion).Bold().FontColor("#1E3A8A");
                            h.Cell().Padding(10).Background("#F8FAFC").BorderBottom(2).BorderColor("#2563EB").AlignCenter().Text("Cant.").Bold().FontColor("#1E3A8A");
                            h.Cell().Padding(10).Background("#F8FAFC").BorderBottom(2).BorderColor("#2563EB").AlignRight().Text("Precio").Bold().FontColor("#1E3A8A");
                            h.Cell().Padding(10).Background("#F8FAFC").BorderBottom(2).BorderColor("#2563EB").AlignRight().Text("Subtotal").Bold().FontColor("#1E3A8A");
                        });

                        foreach (var linea in factura.Lineas)
                        {
                            table.Cell().Padding(10).BorderBottom(1).BorderColor("#F1F5F9").Text(linea.Producto?.Nombre ?? "---");
                            table.Cell().Padding(10).BorderBottom(1).BorderColor("#F1F5F9").AlignCenter().Text(linea.Cantidad.ToString());
                            table.Cell().Padding(10).BorderBottom(1).BorderColor("#F1F5F9").AlignRight().Text($"{linea.PrecioUnitario:N2}{strEuro}");
                            table.Cell().Padding(10).BorderBottom(1).BorderColor("#F1F5F9").AlignRight().Text($"{linea.Subtotal:N2}{strEuro}");
                        }
                    });

                    // Totales con fondo sutil
                    col.Item().AlignRight().PaddingTop(20).Width(220).Container().Background("#F8FAFC").Padding(15).Column(t =>
                    {
                        t.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontSize(10).FontColor("#64748B");
                            r.ConstantItem(80).AlignRight().Text($"{factura.Subtotal:N2}{strEuro}");
                        });
                        t.Item().PaddingVertical(5).Row(r =>
                        {
                            r.RelativeItem().Text("IVA (21%):").FontSize(10).FontColor("#64748B");
                            r.ConstantItem(80).AlignRight().Text($"{factura.Iva:N2}{strEuro}");
                        });
                        t.Item().BorderTop(1).BorderColor("#E2E8F0").PaddingTop(10).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL").Bold().FontSize(16).FontColor("#1E3A8A");
                            r.ConstantItem(100).AlignRight().Text($"{factura.Total:N2}{strEuro}").Bold().FontSize(16).FontColor("#2563EB");
                        });
                    });
                });

                // Footer decorativo
                page.Footer().PaddingTop(40).Column(f =>
                {
                    f.Item().AlignCenter().Text(t =>
                    {
                        t.Span("Gracias por elegir FacturApp").Bold().FontColor("#94A3B8");
                    });
                    f.Item().PaddingTop(5).AlignCenter().Text(t =>
                    {
                        t.Span("Documento generado electr\u00f3nicamente el ").FontColor("#CBD5E1").FontSize(8);
                        t.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontColor("#CBD5E1").FontSize(8).Italic();
                    });
                });
            });
        });

        return Task.FromResult(doc.GeneratePdf());
    }
}
