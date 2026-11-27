using Microsoft.EntityFrameworkCore;
using Sicofri.API.Data;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Repositories;

public class ViajeRepository : IViajeRepository
{
    private readonly SicofriDbContext _db;

    public ViajeRepository(SicofriDbContext db)
    {
        _db = db;
    }

    /// <summary>El viaje completo, con todo el detalle de sus pedidos.</summary>
    public async Task<Viaje?> ObtenerPorIdAsync(int idViaje)
    {
        return await ConDetalleCompleto(_db.Viajes)
            .FirstOrDefaultAsync(v => v.IdViaje == idViaje);
    }//Fin del metodo ObtenerPorIdAsync

    /// <summary>Lista paginada (resumen), del mas reciente al mas antiguo.</summary>
    public async Task<(List<Viaje> Viajes, int Total)> BuscarAsync(
        EstadoViaje? estado, DateOnly? desde, DateOnly? hasta, int? idEmpleado, int saltar, int tomar)
    {
        var consulta = AplicarFiltros(_db.Viajes.AsNoTracking(), estado, desde, hasta, idEmpleado);
        int total = await consulta.CountAsync();

        var viajes = await consulta
            .Include(v => v.Encargados).ThenInclude(e => e.Usuario)
            .Include(v => v.Pedidos).ThenInclude(p => p.Lineas)
            .OrderByDescending(v => v.Fecha).ThenByDescending(v => v.IdViaje)
            .Skip(saltar).Take(tomar)
            .AsSplitQuery()
            .ToListAsync();

        return (viajes, total);
    }//Fin del metodo BuscarAsync

    /// <summary>
    /// Bitacora de entregas: igual que la lista, pero trae tambien el creador,
    /// el cliente de cada pedido y quien lo entrego.
    /// </summary>
    public async Task<(List<Viaje> Viajes, int Total)> BuscarParaBitacoraAsync(
        EstadoViaje? estado, DateOnly? desde, DateOnly? hasta, int? idEmpleado, int saltar, int tomar)
    {
        var consulta = AplicarFiltros(_db.Viajes.AsNoTracking(), estado, desde, hasta, idEmpleado);
        int total = await consulta.CountAsync();

        var viajes = await consulta
            .Include(v => v.Encargados).ThenInclude(e => e.Usuario)
            .Include(v => v.UsuarioCreador)
            .Include(v => v.Pedidos).ThenInclude(p => p.Cliente)
            .Include(v => v.Pedidos).ThenInclude(p => p.UsuarioEntrega)
            .Include(v => v.Pedidos).ThenInclude(p => p.Lineas)
            .OrderByDescending(v => v.Fecha).ThenByDescending(v => v.IdViaje)
            .Skip(saltar).Take(tomar)
            .AsSplitQuery()
            .ToListAsync();

        return (viajes, total);
    }//Fin del metodo BuscarParaBitacoraAsync

    /// <summary>"Mis entregas": viajes Programados o EnCurso donde el empleado 
    /// es encargado.</summary>
    public async Task<List<Viaje>> ObtenerActivosDeEmpleadoAsync(int idEmpleado)
    {
        var consulta = _db.Viajes.AsNoTracking()
            .Where(v => v.Estado == EstadoViaje.Programado || v.Estado == EstadoViaje.EnCurso)
            .Where(v => v.Encargados.Any(e => e.IdUsuario == idEmpleado));

        return await ConDetalleCompleto(consulta).ToListAsync();
    }//Fin del metodo ObtenerActivosDeEmpleadosAsync

    public async Task<List<Usuario>> ObtenerUsuariosAsync(IEnumerable<int> idsUsuarios)
    {
        var ids = idsUsuarios.Distinct().ToList();
        return await _db.Usuarios.Where(u => ids.Contains(u.IdUsuario)).ToListAsync();
    }//Fin del metodo ObtenerUsuarioAsync

    /// <summary>Pedidos a asignar al viaje (con seguimiento, porque se 
    /// les va a cambiar el viaje).</summary>
    public async Task<List<Pedido>> ObtenerPedidosAsync(IEnumerable<int> idsPedidos)
    {
        var ids = idsPedidos.Distinct().ToList();
        return await _db.Pedidos.Where(p => ids.Contains(p.IdPedido)).ToListAsync();
    }//Fin del metodo ObtenerPedidosAsync

    public async Task AgregarAsync(Viaje viaje)
    {
        await _db.Viajes.AddAsync(viaje);
    }//Fin del metodo AgregarAsync

    public void Eliminar(Viaje viaje)
    {
        _db.Viajes.Remove(viaje);
    }//FIn del metodo Eliminar

    public void EliminarEncargados(IEnumerable<ViajeEncargado> encargados)
    {
        _db.ViajeEncargados.RemoveRange(encargados);
    }//Fin del metodo EliminarEncargados

    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }//Fin del metodo GuardarCambiosAsync

    //Piezas reutilizables

    /// <summary>
    /// Los filtros de la lista y de la bitacora, escritos una sola vez.
    /// </summary>
    private static IQueryable<Viaje> AplicarFiltros(IQueryable<Viaje> consulta,
        EstadoViaje? estado, DateOnly? desde, DateOnly? hasta, int? idEmpleado)
    {
        if (estado.HasValue)
            consulta = consulta.Where(v => v.Estado == estado.Value);

        if (desde.HasValue)
            consulta = consulta.Where(v => v.Fecha >= desde.Value);

        if (hasta.HasValue)
            consulta = consulta.Where(v => v.Fecha <= hasta.Value);

        if (idEmpleado.HasValue)
            consulta = consulta.Where(v => v.Encargados.Any(e => e.IdUsuario == idEmpleado.Value));

        return consulta;
    }//Fin del metodo AplicarFiltros

    /// <summary>Todo lo necesario para mostrar un viaje completo 
    /// (detalle y "mis entregas").</summary>
    private static IQueryable<Viaje> ConDetalleCompleto(IQueryable<Viaje> consulta)
    {
        return consulta
            .Include(v => v.Encargados).ThenInclude(e => e.Usuario)
            .Include(v => v.UsuarioCreador)
            .Include(v => v.Pedidos).ThenInclude(p => p.Cliente)
            .Include(v => v.Pedidos).ThenInclude(p => p.UsuarioEntrega)
            .Include(v => v.Pedidos).ThenInclude(p => p.Lineas).ThenInclude(l => l.Producto).ThenInclude(pr => pr.Ubicacion)
            .AsSplitQuery();
    }//Fin del metodo ConDetalleCompleto
}//Fin de la clase ViajeRepository