using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Security;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Pedidos;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Controllers;

/// <summary>
/// Pedidos a clientes. Todos los roles pueden consultar y armar;
/// solo el Jefe y la Secretaria pueden modificar o cancelar.
/// </summary>
[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private const string RolesAdministrativos = $"{nameof(Rol.Jefe)},{nameof(Rol.Secretaria)}";

    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>
    /// Lista de pedidos, del más reciente al mas antiguo.
    /// busqueda: numero de pedido ("PE-000012" o "12") o parte del nombre del cliente.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<PedidoResumenDto>>> Buscar(
        [FromQuery] EstadoPedido? estado,
        [FromQuery] string? busqueda,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        return Ok(await _pedidoService.BuscarAsync(estado, busqueda, pagina, tamanoPagina));
    }//Fin del metodo Buscar

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoDto>> ObtenerPorId(int id)
    {
        return Ok(await _pedidoService.ObtenerPorIdAsync(id));
    }//Fin del metodo ObtenerPorId

    /// <summary>
    /// Arma un pedido y descuenta los kilos. Disponible para todos los roles.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PedidoDto>> Crear(CrearPedidoRequest datos)
    {
        var pedido = await _pedidoService.CrearAsync(datos, User.ObtenerIdUsuario());
        return StatusCode(StatusCodes.Status201Created, pedido);
    }//Fin del metodo Crear

    /// <summary>
    /// Modifica un pedido Armado (reemplaza sus lineas). Jefe y Secretaria.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<ActionResult<PedidoDto>> Actualizar(int id, ActualizarPedidoRequest datos)
    {
        return Ok(await _pedidoService.ActualizarAsync(id, datos));
    }//Fin del metodo Actualizar

    /// <summary>
    /// Cancela un pedido Armado y devuelve los kilos a sus lotes.Jefe y Secretaria.
    /// </summary>
    [HttpPut("{id:int}/cancelar")]
    [Authorize(Roles = RolesAdministrativos)]
    public async Task<ActionResult<PedidoDto>> Cancelar(int id)
    {
        return Ok(await _pedidoService.CancelarAsync(id, User.ObtenerIdUsuario()));
    }//Fin del metodo Cancelar
}//Fin del metodo PedidosController