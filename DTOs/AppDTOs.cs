using System.Text.Json.Serialization;

namespace FacturApp.DTOs;

public class FacturaDto
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string ClienteNif { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteDireccion { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string? PdfBase64 { get; set; }
    public List<LineaFacturaDto> Lineas { get; set; } = [];
}

public class LineaFacturaDto
{
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
