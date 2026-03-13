using FacturApp.Data;
using FacturApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FacturApp.Repositories;

public interface IFacturaRepository
{
    Task<Factura> SaveAsync(Factura factura, CancellationToken ct = default);
    Task<List<Factura>> GetAllAsync(CancellationToken ct = default);
    Task<Factura?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> GenerateNumeroFacturaAsync(CancellationToken ct = default);
}

public class FacturaRepository(AppDbContext db) : IFacturaRepository
{
    public async Task<Factura> SaveAsync(Factura factura, CancellationToken ct = default)
    {
        db.Facturas.Add(factura);
        await db.SaveChangesAsync(ct);
        return factura;
    }

    public Task<List<Factura>> GetAllAsync(CancellationToken ct = default) =>
        db.Facturas
          .Include(f => f.Cliente)
          .Include(f => f.Lineas).ThenInclude(l => l.Producto)
          .OrderByDescending(f => f.FechaEmision)
          .ToListAsync(ct);

    public Task<Factura?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.Facturas
          .Include(f => f.Cliente)
          .Include(f => f.Lineas).ThenInclude(l => l.Producto)
          .FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<string> GenerateNumeroFacturaAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.Facturas.CountAsync(f => f.FechaEmision.Year == year, ct);
        return $"FAC-{year}-{(count + 1):D4}";
    }
}
