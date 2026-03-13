using FacturApp.Data;
using FacturApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FacturApp.Repositories;

public interface IProductoRepository
{
    Task<List<Producto>> GetByNamesAsync(IEnumerable<string> nombres, CancellationToken ct = default);
    Task<List<Producto>> GetAllAsync(CancellationToken ct = default);
    Task UpdateStockAsync(int productoId, int cantidad, CancellationToken ct = default);
    Task<Producto> AddAsync(Producto producto, CancellationToken ct = default);
}

public class ProductoRepository(AppDbContext db) : IProductoRepository
{
    public async Task<List<Producto>> GetByNamesAsync(IEnumerable<string> nombres, CancellationToken ct = default)
    {
        var results = new List<Producto>();
        foreach (var nombre in nombres)
        {
            var p = await db.Productos
                .Where(x => EF.Functions.ILike(x.Nombre, $"%{nombre}%"))
                .FirstOrDefaultAsync(ct);
            if (p != null) results.Add(p);
        }
        return results;
    }

    public Task<List<Producto>> GetAllAsync(CancellationToken ct = default) =>
        db.Productos.OrderBy(p => p.Nombre).ToListAsync(ct);

    public async Task UpdateStockAsync(int productoId, int cantidad, CancellationToken ct = default)
    {
        var p = await db.Productos.FindAsync([productoId], ct);
        if (p != null) { p.Stock -= cantidad; await db.SaveChangesAsync(ct); }
    }

    public async Task<Producto> AddAsync(Producto producto, CancellationToken ct = default)
    {
        db.Productos.Add(producto);
        await db.SaveChangesAsync(ct);
        return producto;
    }
}
