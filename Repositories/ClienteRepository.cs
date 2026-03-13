using FacturApp.Data;
using FacturApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FacturApp.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> SearchByNameAsync(string nombre, CancellationToken ct = default);
    Task<List<Cliente>> GetAllAsync(CancellationToken ct = default);
    Task<Cliente> AddAsync(Cliente cliente, CancellationToken ct = default);
}

public class ClienteRepository(AppDbContext db) : IClienteRepository
{
    public Task<Cliente?> SearchByNameAsync(string nombre, CancellationToken ct = default) =>
        db.Clientes
          .Where(c => EF.Functions.ILike(c.Nombre, $"%{nombre}%"))
          .FirstOrDefaultAsync(ct);

    public Task<List<Cliente>> GetAllAsync(CancellationToken ct = default) =>
        db.Clientes.OrderBy(c => c.Nombre).ToListAsync(ct);

    public async Task<Cliente> AddAsync(Cliente cliente, CancellationToken ct = default)
    {
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        return cliente;
    }
}
