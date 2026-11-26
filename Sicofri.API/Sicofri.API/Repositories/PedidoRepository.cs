using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sicofri.API.Data;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly SicofriDbContext _db;

    public PedidoRepository(SicofriDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// El pedido completo: cliente, lineas con su lote y ubicacion,
    /// y los usuarios del rastro.
    /// </summary>
    public async Task<Pedido?> ObtenerPorIdAsync(int idPedido)
    {
        return await _db.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Lineas).ThenInclude(l => l.Producto).ThenInclude(pr => pr.Ubicacion)
            .Include(p => p.UsuarioCreador)
            .Include(p => p.UsuarioEntrega)
            .Include(p => p.UsuarioCancela)
            .FirstOrDefaultAsync(p => p.IdPedido == idPedido);
    }//Fin del metodo ObtenerPorIdAsync

    /// <summary>Lista paginada, del mas reciente al mas antiguo.</summary>
    public async Task<(List<Pedido> Pedidos, int Total)> BuscarAsync(
        EstadoPedido? estado, int? idPedido, string? nombreCliente, int saltar, int tomar)
    {
        var consulta = _db.Pedidos.AsNoTracking().AsQueryable();

        if (estado.HasValue)
            consulta = consulta.Where(p => p.Estado == estado.Value);

        if (idPedido.HasValue)
            consulta = consulta.Where(p => p.IdPedido == idPedido.Value);

        if (!string.IsNullOrWhiteSpace(nombreCliente))
        {
            string patron = $"%{nombreCliente.Trim()}%";
            consulta = consulta.Where(p => EF.Functions.ILike(p.Cliente.Nombre, patron));
        }

        int total = await consulta.CountAsync();

        var pedidos = await consulta
            .Include(p => p.Cliente)
            .Include(p => p.Lineas)
            .OrderByDescending(p => p.FechaHoraCreacion)
            .Skip(saltar)
            .Take(tomar)
            .ToListAsync();

        return (pedidos, total);
    }//Fin del metodo BuscarAsync

    public async Task<Cliente?> ObtenerClienteAsync(int idCliente)
    {
        return await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == idCliente);
    }//Fin del metodo ObtenerClienteAsync

    /// <summary>
    /// Trae de una sola vez todos los lotes que aparecen en las lineas.
    /// </summary>
    public async Task<List<Producto>> ObtenerLotesAsync(IEnumerable<int> idsProductos)
    {
        var ids = idsProductos.Distinct().ToList();

        return await _db.Productos
            .AsNoTracking()
            .Include(p => p.Ubicacion)
            .Where(p => ids.Contains(p.IdProducto))
            .ToListAsync();
    }//Fin del metodo ObtenerLotesAsync

    /// <summary>
    /// Resta los kilos solo si todavia alcanzan 
    /// (proteccion contra dos usuarios a la vez).
    /// </summary>
    public async Task<bool> DescontarKilosAsync(int idProducto, decimal cantidad)
    {
        int filas = await _db.Productos
            .Where(p => p.IdProducto == idProducto && p.Cantidad >= cantidad)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Cantidad, p => p.Cantidad - cantidad));

        return filas == 1;
    }//Fin del metodo DescontarKilosAsync

    public async Task DevolverKilosAsync(int idProducto, decimal cantidad)
    {
        await _db.Productos
            .Where(p => p.IdProducto == idProducto)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Cantidad, p => p.Cantidad + cantidad));
    }//Fin del metodo DevolverKilosAsync

    public async Task AgregarAsync(Pedido pedido)
    {
        await _db.Pedidos.AddAsync(pedido);
    }//Fin del metodo AgregarAsync

    public void EliminarLineas(IEnumerable<LineaPedido> lineas)
    {
        _db.LineasPedido.RemoveRange(lineas);
    }//Fin del metodo EliminarLineas

    public async Task<IDbContextTransaction> IniciarTransaccionAsync()
    {
        return await _db.Database.BeginTransactionAsync();
    }//Fin del metodo IniciarTransaccionAsync

    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }//Fin del metodo GuardarCambiosAsync
}//Fin de la clase PedidoRepository