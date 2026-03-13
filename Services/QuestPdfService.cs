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

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FacturApp").Bold().FontSize(24).FontColor("#1A56DB");
                        col.Item().Text("Sistema de FacturaciÃ³n").FontSize(11).FontColor("#6B7280");
                    });
                    row.ConstantItem(160).AlignRight().Column(col =>
                    {
                        col.Item().Text("FACTURA").Bold().FontSize(18).FontColor("#111827");
                        col.Item().Text($"NÂº {factura.NumeroFactura}").FontSize(11).FontColor("#374151");
                        col.Item().Text($"Fecha: {factura.FechaEmision:dd/MM/yyyy}").FontSize(10).FontColor("#6B7280");
                    });
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Border(1).BorderColor("#E5E7EB").Padding(15).Column(cc =>
                    {
                        cc.Item().Text("DATOS DEL CLIENTE").Bold().FontSize(11).FontColor("#374151");
                        cc.Item().PaddingTop(6).Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text(factura.Cliente?.Nombre ?? "â€”").Bold().FontSize(12);
                                c.Item().Text($"NIF: {factura.Cliente?.Nif ?? "â€”"}").FontColor("#6B7280");
                                c.Item().Text($"Email: {factura.Cliente?.Email ?? "â€”"}").FontColor("#6B7280");
                            });
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Dir: {factura.Cliente?.Direccion ?? "â€”"}").FontColor("#6B7280");
                                c.Item().Text($"Tel: {factura.Cliente?.Telefono ?? "â€”"}").FontColor("#6B7280");
                            });
                        });
                    });

                    col.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);
                            cols.RelativeColumn(1);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background("#1A56DB").Padding(8).Text("DescripciÃ³n").Bold().FontColor(QColors.White);
                            h.Cell().Background("#1A56DB").Padding(8).AlignCenter().Text("Cant.").Bold().FontColor(QColors.White);
                            h.Cell().Background("#1A56DB").Padding(8).AlignRight().Text("Precio").Bold().FontColor(QColors.White);
                            h.Cell().Background("#1A56DB").Padding(8).AlignRight().Text("Subtotal").Bold().FontColor(QColors.White);
                        });

                        var alt = false;
                        foreach (var linea in factura.Lineas)
                        {
                            var bg = alt ? "#F9FAFB" : "#FFFFFF";
                            alt = !alt;
                            table.Cell().Background(bg).Padding(8).Text(linea.Producto?.Nombre ?? "â€”");
                            table.Cell().Background(bg).Padding(8).AlignCenter().Text(linea.Cantidad.ToString());
                            table.Cell().Background(bg).Padding(8).AlignRight().Text($"{linea.PrecioUnitario:F2} â‚¬");
                            table.Cell().Background(bg).Padding(8).AlignRight().Text($"{linea.Subtotal:F2} â‚¬");
                        }
                    });

                    col.Item().PaddingTop(20).AlignRight().Column(t =>
                    {
                        t.Item().Width(200).Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").FontColor("#6B7280");
                            r.ConstantItem(80).AlignRight().Text($"{factura.Subtotal:F2} â‚¬");
                        });
                        t.Item().Width(200).PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text("IVA (21%):").FontColor("#6B7280");
                            r.ConstantItem(80).AlignRight().Text($"{factura.Iva:F2} â‚¬");
                        });
                        t.Item().Width(200).PaddingTop(6).Background("#1A56DB").Padding(8).Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:").Bold().FontColor(QColors.White);
                            r.ConstantItem(80).AlignRight().Text($"{factura.Total:F2} â‚¬").Bold().FontColor(QColors.White);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("FacturApp â€” ").FontColor("#6B7280").FontSize(8);
                    t.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontColor("#6B7280").FontSize(8);
                });
            });
        });

        return Task.FromResult(doc.GeneratePdf());
    }
}
