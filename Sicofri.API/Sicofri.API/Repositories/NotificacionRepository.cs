using Microsoft.EntityFrameworkCore;
using Sicofri.API.Data;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly SicofriDbContext _db;

    public NotificacionRepository(SicofriDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Ids de los lotes que ya tienen notificacion (pendiente o atendida).
    /// </summary>
    public async Task<HashSet<int>> ObtenerIdsProductosNotificadosAsync()
    {
        var ids = await _db.Notificaciones
            .Select(n => n.IdProducto)
            .ToListAsync();

        return ids.ToHashSet();
    }

    /// <summary>
    /// Notificaciones pendientes cuyo lote ya llego a 0 kg: se cierran solas.
    /// </summary>
    public async Task<List<Notificacion>> ObtenerPendientesDeLotesAgotadosAsync()
    {
        return await _db.Notificaciones
            .Where(n => !n.Atendida && n.Producto.Cantidad <= 0)
            .ToListAsync();
    }

    /// <summary>
    /// Lista paginada. Muestra primero las pendientes, y dentro de ellas
    /// las que vencen mas pronto.
    /// </summary>
    public async Task<(List<Notificacion> Notificaciones, int Total)> BuscarAsync(bool? atendida, int saltar, int tomar)
    {
        var consulta = _db.Notificaciones.AsNoTracking().AsQueryable();

        if (atendida.HasValue)
            consulta = consulta.Where(n => n.Atendida == atendida.Value);

        int total = await consulta.CountAsync();

        var notificaciones = await consulta
            .Include(n => n.Producto).ThenInclude(p => p.Ubicacion)
            .Include(n => n.UsuarioAtiende)
            .OrderBy(n => n.Atendida)
            .ThenBy(n => n.Producto.FechaCaducidad)
            .Skip(saltar)
            .Take(tomar)
            .ToListAsync();

        return (notificaciones, total);
    }

    public async Task<Notificacion?> ObtenerPorIdAsync(int idNotificacion)
    {
        return await _db.Notificaciones
            .Include(n => n.Producto).ThenInclude(p => p.Ubicacion)
            .Include(n => n.UsuarioAtiende)
            .FirstOrDefaultAsync(n => n.IdNotificacion == idNotificacion);
    }

    public async Task<int> ContarPendientesAsync()
    {
        return await _db.Notificaciones.CountAsync(n => !n.Atendida);
    }

    /// <summary>Tokens de los celulares donde el Jefe tiene la sesión abierta.</summary>
    public async Task<List<string>> ObtenerTokensDelJefeAsync()
    {
        return await _db.Usuarios
            .Where(u => u.Rol == Rol.Jefe && u.Activo && u.TokenFcm != null)
            .Select(u => u.TokenFcm!)
            .ToListAsync();
    }

    /// <summary>
    /// Borra los tokens que Firebase rechazó (app desinstalada,
    /// token vencido o invalido).
    /// </summary>
    public async Task QuitarTokensAsync(List<string> tokens)
    {
        await _db.Usuarios
            .Where(u => u.TokenFcm != null && tokens.Contains(u.TokenFcm))
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.TokenFcm, (string?)null));
    }

    public async Task AgregarAsync(Notificacion notificacion)
    {
        await _db.Notificaciones.AddAsync(notificacion);
    }

    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }
}//Fin de la clase NotificacionRepository