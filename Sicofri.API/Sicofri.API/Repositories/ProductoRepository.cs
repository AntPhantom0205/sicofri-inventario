using Microsoft.EntityFrameworkCore;
using Sicofri.API.Data;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;

namespace Sicofri.API.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly SicofriDbContext _db;

    public ProductoRepository(SicofriDbContext db)
    {
        _db = db;
    }

    public async Task<Producto?> ObtenerPorIdAsync(int idProducto)
    {
        return await _db.Productos
            .Include(p => p.Ubicacion)
            .FirstOrDefaultAsync(p => p.IdProducto == idProducto);
    }//Fin del metodo ObtenerPorIdAsync

    /// <summary>
    /// Busca lotes por nombre (sin importar las mayusculas) y/o ubicacion.
    /// Por defecto no muestra los lotes agotados (0 kg).
    /// </summary>
    public async Task<List<Producto>> BuscarAsync(string? nombre, int? idUbicacion, bool incluirAgotados)
    {
        var consulta = _db.Productos
            .AsNoTracking()
            .Include(p => p.Ubicacion)
            .AsQueryable();

        if (!incluirAgotados)
            consulta = consulta.Where(p => p.Cantidad > 0);

        if (idUbicacion.HasValue)
            consulta = consulta.Where(p => p.IdUbicacion == idUbicacion.Value);

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            string patron = $"%{nombre.Trim()}%";
            consulta = consulta.Where(p => EF.Functions.ILike(p.Nombre, patron));
        }

        return await consulta
            .OrderBy(p => p.FechaCaducidad)
            .ToListAsync();
    }//Fin del metodo BuscarAsync

    public async Task<List<Producto>> ObtenerConCaducidadHastaAsync(DateOnly fechaLimite)
    {
        return await _db.Productos
            .AsNoTracking()
            .Include(p => p.Ubicacion)
            .Where(p => p.Cantidad > 0 && p.FechaCaducidad <= fechaLimite)
            .OrderBy(p => p.FechaCaducidad)
            .ToListAsync();
    }//Fin del metodo ObtenerConCaducidadHastaAsync

    public async Task<bool> TieneMovimientosOPedidosAsync(int idProducto)
    {
        bool tieneMovimientos = await _db.MovimientosTraslado
            .AnyAsync(m => m.IdProductoOrigen == idProducto || m.IdProductoDestino == idProducto);

        bool tienePedidos = await _db.LineasPedido
            .AnyAsync(l => l.IdProducto == idProducto);

        return tieneMovimientos || tienePedidos;
    }//Fin del metodo TieneMovimientosOPedidosAsync

    public async Task<Ubicacion?> ObtenerUbicacionAsync(int idUbicacion)
    {
        return await _db.Ubicaciones.FirstOrDefaultAsync(u => u.IdUbicacion == idUbicacion);
    }//Fin del metodo ObtenerUbicacionAsync

    /// <summary>Las 6 ubicaciones, cada una con solo sus lotes que tienen existencia.</summary>
    public async Task<List<Ubicacion>> ObtenerUbicacionesConExistenciasAsync()
    {
        return await _db.Ubicaciones
            .AsNoTracking()
            .Include(u => u.Productos.Where(p => p.Cantidad > 0))
            .OrderBy(u => u.IdUbicacion)
            .ToListAsync();
    }//Fin del metodo ObtenerUbicacionesConExistenciasAsync

    public async Task AgregarAsync(Producto producto)
    {
        await _db.Productos.AddAsync(producto);
    }//Fin del metodo AgregarAsync

    public void Eliminar(Producto producto)
    {
        _db.Productos.Remove(producto);
    }//Fin del metodo Eliminar

    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }//Fin del metodo GuardarCambiosAsync

}//Fin de la clase ProductoRepository
