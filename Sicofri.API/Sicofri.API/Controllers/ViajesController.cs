using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Security;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Viajes;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Controllers;

/// <summary>
/// Viajes de entrega. Jefe y Secretaria los programan; los empleados encargados \
/// (o el Jefe)
/// los inician y registran las entregas.
/// </summary>
[ApiController]
[Route("api/viajes")]
[Authorize]
public class ViajesController : ControllerBase
{
    private const string RolesAdministrativos = $"{nameof(Rol.Jefe)},{nameof(Rol.Secretaria)}";
    private const string RolesDeRuta = $"{nameof(Rol.Jefe)},{nameof(Rol.Empleado)}";

    private readonly IViajeService _viajeService;

    public ViajesController(IViajeService viajeService)
    {
        _viajeService = viajeService;
    }

    /// <summary>Lista de viajes con filtros por estado y rango de fechas.</summary>
    [HttpGet]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<ActionResult<ResultadoPaginado<ViajeResumenDto>>> Buscar(
        [FromQuery] EstadoViaje? estado,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        return Ok(await _viajeService.BuscarAsync(estado, desde, hasta, pagina, tamanoPagina));
    }//Fin del metodo Buscar

    [HttpGet("{id:int}")]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<ActionResult<ViajeDto>> ObtenerPorId(int id)
    {
        return Ok(await _viajeService.ObtenerPorIdAsync(id));
    }//Fin del metodo ObtenerPorId

    /// <summary>Programa un viaje con 1 a 3 empleados y sus pedidos.</summary>
    [HttpPost]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<ActionResult<ViajeDto>> Crear(CrearViajeRequest datos)
    {
        var viaje = await _viajeService.CrearAsync(datos, User.ObtenerIdUsuario());
        return StatusCode(StatusCodes.Status201Created, viaje);
    }//Fin del metodo Crear

    /// <summary>Modifica un viaje Programado.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<ActionResult<ViajeDto>> Actualizar(int id, ActualizarViajeRequest datos)
    {
        return Ok(await _viajeService.ActualizarAsync(id, datos));
    }//Fin del metodo Actualizar

    /// <summary>Elimina un viaje Programado; sus pedidos quedan libres.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _viajeService.EliminarAsync(id);
        return NoContent();
    }//Fin del metodo Eliminar

    /// <summary>Inicia el viaje: pasa a EnCurso y sus pedidos a EnViaje. 
    /// Encargados o Jefe.</summary>
    [HttpPut("{id:int}/iniciar")]
    [Authorize(Roles = RolesDeRuta)]
    public async Task<ActionResult<ViajeDto>> Iniciar(int id)
    {
        return Ok(await _viajeService.IniciarAsync(id, User.ObtenerIdUsuario(), User.IsInRole(nameof(Rol.Jefe))));
    }//Fin del metodo Iniciar

    /// <summary>Marca un pedido del viaje como entregado. Encargados o Jefe.</summary>
    [HttpPut("{id:int}/pedidos/{idPedido:int}/entregar")]
    [Authorize(Roles = RolesDeRuta)]
    public async Task<ActionResult<ViajeDto>> EntregarPedido(int id, int idPedido)
    {
        return Ok(await _viajeService.EntregarPedidoAsync(id, idPedido, User.ObtenerIdUsuario(), User.IsInRole(nameof(Rol.Jefe))));
    }//Fin del metodo EntregarPedido
}//Fin de la clase ViajesController