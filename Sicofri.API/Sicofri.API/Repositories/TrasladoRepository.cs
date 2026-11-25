using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sicofri.API.Data;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;

namespace Sicofri.API.Repositories;

public class TrasladoRepository : ITrasladoRepository
{
    private readonly SicofriDbContext _db;

    public TrasladoRepository(SicofriDbContext db)
    {
        _db = db;
    }

    public async Task<Producto?> ObtenerLoteAsync(int idProducto)
    {
        return await _db.Productos
            .AsNoTracking()
            .Include(p => p.Ubicacion)
            .FirstOrDefaultAsync(p => p.IdProducto == idProducto);
    }

    public async Task<Ubicacion?> ObtenerUbicacionAsync(int idUbicacion)
    {
        return await _db.Ubicaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUbicacion == idUbicacion);
    }

    public async Task<string?> ObtenerNombreUsuarioAsync(int idUsuario)
    {
        return await _db.Usuarios
            .Where(u => u.IdUsuario == idUsuario && u.Activo)
            .Select(u => u.Nombre)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Busca en el destino un lote identico al de origen (mismo producto, proveedor,
    /// tipo, codigo y fechas). Si existe, el traslado le suma los kilos en vez de crear otro lote.
    /// </summary>
    public async Task<Producto?> BuscarLoteGemeloAsync(Producto origen, int idUbicacionDestino)
    {
        return await _db.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.IdUbicacion == idUbicacionDestino &&
                p.Nombre == origen.Nombre &&
                p.NombreProveedor == origen.NombreProveedor &&
                p.TipoLote == origen.TipoLote &&
                p.CodigoLote == origen.CodigoLote &&
                p.FechaIngreso == origen.FechaIngreso &&
                p.FechaProduccion == origen.FechaProduccion &&
                p.FechaCaducidad == origen.FechaCaducidad);
    }

    /// <summary>
    /// Resta los kilos en UNA sola instrucción, y solo si todavia alcanzan:
    /// UPDATE producto SET cantidad = cantidad - X WHERE id = ... AND cantidad >= X
    /// Devuelve false si no se pudo (otro usuario se adelantó o no alcanza).
    /// </summary>
    public async Task<bool> DescontarAsync(int idProducto, decimal cantidad)
    {
        int filasAfectadas = await _db.Productos
            .Where(p => p.IdProducto == idProducto && p.Cantidad >= cantidad)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Cantidad, p => p.Cantidad - cantidad));

        return filasAfectadas == 1;
    }

    public async Task SumarAsync(int idProducto, decimal cantidad)
    {
        await _db.Productos
            .Where(p => p.IdProducto == idProducto)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Cantidad, p => p.Cantidad + cantidad));
    }

    public async Task AgregarLoteAsync(Producto lote)
    {
        await _db.Productos.AddAsync(lote);
    }

    public async Task AgregarMovimientoAsync(MovimientoTraslado movimiento)
    {
        await _db.MovimientosTraslado.AddAsync(movimiento);
    }

    public async Task<(List<MovimientoTraslado> Movimientos, int Total)> ObtenerBitacoraAsync(
        string? nombreProducto, DateTime? desdeUtc, DateTime? hastaUtc, int saltar, int tomar)
    {
        var consulta = _db.MovimientosTraslado.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombreProducto))
        {
            string patron = $"%{nombreProducto.Trim()}%";
            consulta = consulta.Where(m => EF.Functions.ILike(m.ProductoOrigen.Nombre, patron));
        }

        if (desdeUtc.HasValue)
            consulta = consulta.Where(m => m.FechaHora >= desdeUtc.Value);

        if (hastaUtc.HasValue)
            consulta = consulta.Where(m => m.FechaHora < hastaUtc.Value);

        int total = await consulta.CountAsync();

        var movimientos = await consulta
            .Include(m => m.ProductoOrigen)
            .Include(m => m.UbicacionOrigen)
            .Include(m => m.UbicacionDestino)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.FechaHora)
            .Skip(saltar)
            .Take(tomar)
            .ToListAsync();

        return (movimientos, total);
    }//Fin del metood ObtenerBitacora

    public async Task<IDbContextTransaction> IniciarTransaccionAsync()
    {
        return await _db.Database.BeginTransactionAsync();
    }//Fin del metodo IniciarTransaccionAsync

    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }//Fin del metodo GuardarCambiosAsync
}//Fin de la clase TrasladoRepository