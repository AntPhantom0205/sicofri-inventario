using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Usuarios;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Controllers;
/// <summary>Administracion de cuentas. Exclusivo del rol Jefe.</summary>
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

    /// <summary>Lista los usuarios activos.</summary>
    [HttpGet]
    public async Task<ActionResult<List<UsuarioDto>>> ObtenerTodos()
    {
        return Ok(await _usuarioService.ObtenerTodosAsync());
    }//Fin del metodo ObtenerTodos

    /// <summary>Obtiene un usuario por su id (para la pantalla de edición).</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(int id)
    {
        return Ok(await _usuarioService.ObtenerPorIdAsync(id));
    }//Fin del metodo ObtenerPorId

    /// <summary>Crea un usuario con rol Secretaria o Empleado.</summary>
    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioRequest datos)
    {
        var usuario = await _usuarioService.CrearAsync(datos);
        return StatusCode(StatusCodes.Status201Created, usuario);
    }//Fin del metodo Crear

    /// <summary>Actualiza nombre, usuario, rol y (opcionalmente) la contraseña.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Actualizar(int id, ActualizarUsuarioRequest datos)
    {
        return Ok(await _usuarioService.ActualizarAsync(id, datos));
    }//Fin del metodo Actualizar

    /// <summary>Elimina (desactiva) un usuario. No aplica a la cuenta del Jefe.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _usuarioService.EliminarAsync(id);
        return NoContent();
    }//Fin del metodo Eliminar

}//Fin de la clase