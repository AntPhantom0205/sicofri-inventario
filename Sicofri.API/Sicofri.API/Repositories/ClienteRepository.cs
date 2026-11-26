using Microsoft.EntityFrameworkCore;
using Sicofri.API.Data;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly SicofriDbContext _db;

    public ClienteRepository(SicofriDbContext db)
    {
        _db = db;
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int idCliente)
    {
        return await _db.Clientes.FirstOrDefaultAsync(c => c.IdCliente == idCliente);
    }//Fin del metodo ObtenerPorIdAsync

    public async Task<List<Cliente>> BuscarAsync(string? nombre, Provincia? provincia)
    {
        var consulta = _db.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            string patron = $"%{nombre.Trim()}%";
            consulta = consulta.Where(c => EF.Functions.ILike(c.Nombre, patron));
        }

        if (provincia.HasValue)
            consulta = consulta.Where(c => c.Provincia == provincia.Value);

        return await consulta.OrderBy(c => c.Nombre).ToListAsync();
    }//Fin del metodo BuscarAsync

    /// <summary>
    /// Cuántos pedidos ENTREGADOS tiene cada cliente: { idCliente → cantidad }.
    /// Los clientes sin entregas no aparecen (se toman como 0).
    /// </summary>
    public async Task<Dictionary<int, int>> ContarEntregasPorClienteAsync()
    {
        return await _db.Pedidos
            .Where(p => p.Estado == EstadoPedido.Entregado)
            .GroupBy(p => p.IdCliente)
            .Select(g => new { IdCliente = g.Key, Cantidad = g.Count() })
            .ToDictionaryAsync(x => x.IdCliente, x => x.Cantidad);
    }//Fin del metodo ContarEntregasPorClienteAsync

    /// <summary>
    /// ¿Ya existe un cliente con el mismo nombre en la misma provincia? 
    /// (sin importar mayusculas)
    /// Evita registrar dos veces el mismo hotel.
    /// </summary>
    public async Task<bool> ExisteDuplicadoAsync(string nombre, Provincia provincia, int? excluirIdCliente = null)
    {
        string nombreMinusculas = nombre.ToLower();

        return await _db.Clientes.AnyAsync(c =>
            c.Nombre.ToLower() == nombreMinusculas &&
            c.Provincia == provincia &&
            (excluirIdCliente == null || c.IdCliente != excluirIdCliente));
    }//Fin del metodo ExisteDuplicadoAsync

    public async Task<bool> TienePedidosAsync(int idCliente)
    {
        return await _db.Pedidos.AnyAsync(p => p.IdCliente == idCliente);
    }//Fin del metodo TienePedidosAsync

    public async Task AgregarAsync(Cliente cliente)
    {
        await _db.Clientes.AddAsync(cliente);
    }//Fin del metodo AgregarAsync

    public void Eliminar(Cliente cliente)
    {
        _db.Clientes.Remove(cliente);
    }//Fin del metodo Eliminar

    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }//Fin del metodo GuardarCambiosAsync

}//Fin de la clase ClienteRepository