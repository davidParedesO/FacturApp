using FacturApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FacturApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<LineaFactura> LineasFactura => Set<LineaFactura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("clientes");
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(200);
            e.Property(c => c.Email).HasColumnName("email").HasMaxLength(200);
            e.Property(c => c.Direccion).HasColumnName("direccion");
            e.Property(c => c.Nif).HasColumnName("nif").HasMaxLength(20);
            e.Property(c => c.Telefono).HasColumnName("telefono").HasMaxLength(20);
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("productos");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Nombre).HasColumnName("nombre").IsRequired().HasMaxLength(200);
            e.Property(p => p.Descripcion).HasColumnName("descripcion");
            e.Property(p => p.Precio).HasColumnName("precio").HasPrecision(10, 2);
            e.Property(p => p.Stock).HasColumnName("stock");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Factura>(e =>
        {
            e.ToTable("facturas");
            e.Property(f => f.Id).HasColumnName("id");
            e.Property(f => f.NumeroFactura).HasColumnName("numero_factura").IsRequired().HasMaxLength(50);
            e.Property(f => f.ClienteId).HasColumnName("cliente_id");
            e.Property(f => f.FechaEmision).HasColumnName("fecha_emision");
            e.Property(f => f.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2);
            e.Property(f => f.Iva).HasColumnName("iva").HasPrecision(10, 2);
            e.Property(f => f.Total).HasColumnName("total").HasPrecision(10, 2);
            e.Property(f => f.Estado).HasColumnName("estado").HasMaxLength(50);
            e.Property(f => f.PdfBase64).HasColumnName("pdf_base64");
            e.Property(f => f.Origen).HasColumnName("origen").HasMaxLength(50);
            e.HasOne(f => f.Cliente).WithMany(c => c.Facturas).HasForeignKey(f => f.ClienteId);
        });

        modelBuilder.Entity<LineaFactura>(e =>
        {
            e.ToTable("lineas_factura");
            e.Property(l => l.Id).HasColumnName("id");
            e.Property(l => l.FacturaId).HasColumnName("factura_id");
            e.Property(l => l.ProductoId).HasColumnName("producto_id");
            e.Property(l => l.Cantidad).HasColumnName("cantidad");
            e.Property(l => l.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(10, 2);
            e.Property(l => l.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2);
            e.HasOne(l => l.Factura).WithMany(f => f.Lineas).HasForeignKey(l => l.FacturaId);
            e.HasOne(l => l.Producto).WithMany(p => p.LineasFactura).HasForeignKey(l => l.ProductoId);
        });
    }
}
