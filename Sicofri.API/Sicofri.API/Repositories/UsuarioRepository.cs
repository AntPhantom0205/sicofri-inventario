using Microsoft.EntityFrameworkCore;
using Sicofri.API.Data;
using Sicofri.API.Entities;
using Sicofri.API.Repositories.Interfaces;

namespace Sicofri.API.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly SicofriDbContext _db;

    public UsuarioRepository(SicofriDbContext db)
    {
        _db = db;
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        return await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        return await _db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario)
    {
        return await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);
    }

    public async Task<bool> ExisteAlgunoAsync()
    {
        return await _db.Usuarios.AnyAsync();
    }

    public async Task AgregarAsync(Usuario usuario)
    {
        await _db.Usuarios.AddAsync(usuario);
    }

    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }

}//Fin de la clase