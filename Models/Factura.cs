namespace FacturApp.Models;

public class Factura
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public string? PdfBase64 { get; set; }
    public string Origen { get; set; } = "VOZ";
    public ICollection<LineaFactura> Lineas { get; set; } = [];
}
