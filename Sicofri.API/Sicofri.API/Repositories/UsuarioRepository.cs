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

    public async Task<Usuario?> ObtenerPorIdAsync(int idUsuario)
    {
        return await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
    }//Fin del metodo obtenerPorIdAsync

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario)
    {
        return await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);
    }//Fin del metodo ObtenerPorNombreUsuarioAsync

    public async Task<List<Usuario>> ObtenerActivosAsync()
    {
        return await _db.Usuarios
            .AsNoTracking()
            .Where(u => u.Activo)
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }//Fin del metodo ObtenerActivosAsync

    /// <summary>
    /// Revisa si el nombre de usuario ya está tomado.
    /// Al actualizar, se excluye al propio usuario para que no choque consigo mismo.
    /// </summary>
    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirIdUsuario = null)
    {
        return await _db.Usuarios.AnyAsync(u =>
            u.NombreUsuario == nombreUsuario &&
            (excluirIdUsuario == null || u.IdUsuario != excluirIdUsuario));
    }//Fin del metodo ExisteNombreUsuarioAsync

    public async Task<bool> ExisteAlgunoAsync()
    {
        return await _db.Usuarios.AnyAsync();
    }//Fin del metodo ExisteAlgunoAsync

    /// <summary>
    /// Los telefonos de Calimar son compartidos: si un token ya pertenecia a otro 
    /// usuario,
    /// se le quita, para que los avisos lleguen solo a quien tiene la sesión abierta
    /// en ese celular.
    /// </summary>
    /// 
    public async Task QuitarTokenDeOtrosUsuariosAsync(string tokenFcm, int idUsuario)
    {
        await _db.Usuarios
            .Where(u => u.TokenFcm == tokenFcm && u.IdUsuario != idUsuario)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.TokenFcm, (string?)null));
    }//Fin del metodo QuitarTokenDeOtroUsuarioAsync

    public async Task AgregarAsync(Usuario usuario)
    {
        await _db.Usuarios.AddAsync(usuario);
    }//Fin del metodo AgregarAsync

    public async Task GuardarCambiosAsync()
    {
        await _db.SaveChangesAsync();
    }//Fin del metodo GuardarCambiosAsync

}//Fin de la clase