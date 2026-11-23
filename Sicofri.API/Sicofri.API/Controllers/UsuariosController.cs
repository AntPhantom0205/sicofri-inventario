using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Usuarios;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Controllers;
/// <summary>Administración de cuentas. Exclusivo del rol Jefe.</summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = nameof(Rol.Jefe))]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>Lista todos los usuarios del sistema.</summary>
    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> ObtenerTodos()
    {
        return Ok(await _usuarioService.ObtenerTodosAsync());
    }// Fin del metodo ObtenerTodos

    /// <summary>Crea un nuevo usuario con su rol.</summary>
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioRequest datos)
    {
        var usuario = await _usuarioService.CrearAsync(datos);
        return StatusCode(StatusCodes.Status201Created, usuario);
    }//Fin del método Crear

}//Fin de la clase