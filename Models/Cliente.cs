namespace FacturApp.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Nif { get; set; }
    public string? Telefono { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Factura> Facturas { get; set; } = [];
}
