using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Clientes;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Controllers;

/// <summary>
/// Clientes (hoteles y restaurantes). Todos pueden consultar y registrar;
/// solo el Jefe y la Secretaria pueden editar o eliminar.
/// </summary>
[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private const string RolesAdministrativos = $"{nameof(Rol.Jefe)},{nameof(Rol.Secretaria)}";

    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>Busca clientes por nombre y/o provincia.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ClienteDto>>> Buscar([FromQuery] string? nombre, [FromQuery] Provincia? provincia)
    {
        return Ok(await _clienteService.BuscarAsync(nombre, provincia));
    }//Fin del metodo Buscar

    /// <summary>
    /// Las 7 provincias con sus clientes y entregas: datos para el mapa.
    /// </summary>
    [HttpGet("por-provincia")]
    public async Task<ActionResult<List<ProvinciaClientesDto>>> ObtenerPorProvincia()
    {
        return Ok(await _clienteService.ObtenerPorProvinciaAsync());
    }//Fin del metodo ObtenerPorProvincia

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(int id)
    {
        return Ok(await _clienteService.ObtenerPorIdAsync(id));
    }//Fin del metodo ObtenerPorId

    /// <summary>Registra un cliente. Disponible para todos los roles.</summary>
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(CrearClienteRequest datos)
    {
        var cliente = await _clienteService.CrearAsync(datos);
        return StatusCode(StatusCodes.Status201Created, cliente);
    }//Fin del metodo Crear

    /// <summary>Actualiza un cliente (incluye agregar telefono o contacto). 
    /// Jefe y Secretaria.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, ActualizarClienteRequest datos)
    {
        return Ok(await _clienteService.ActualizarAsync(id, datos));
    }//Fin del metodo Actualizar

    /// <summary>Elimina un cliente sin pedidos. Jefe y Secretaria.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _clienteService.EliminarAsync(id);
        return NoContent();
    }//Fin del metodo Eliminar
}//Fin del metodo ClientesController