using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Productos;

namespace Sicofri.API.Controllers;

/// <summary>
/// Registro, consulta y edicion de lotes. Disponible para todos los roles.
/// </summary>

[ApiController]
[Route("api/productos")]
[Authorize]

public class ProductosController : ControllerBase
{

    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    /// <summary>
    /// Busca lotes por nombre y/o ubicacion. Por defecto omite los agotados.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductoDto>>> Buscar(
        [FromQuery] string? nombre,
        [FromQuery] int? idUbicacion,
        [FromQuery] bool incluirAgotados = false)
    {
        return Ok(await _productoService.BuscarAsync(nombre, idUbicacion, incluirAgotados));
    }//Fin del metodo Buscar

    /// <summary>Lotes proximos a caducar o vencidos, segun el tipo de lote.</summary>
    [HttpGet("proximos-a-caducar")]
    public async Task<ActionResult<List<ProductoDto>>> ObtenerProximosACaducar()
    {
        return Ok(await _productoService.ObtenerProximosACaducarAsync());
    }//Fin del metodo ObtenerProximosACaducar

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(int id)
    {
        return Ok(await _productoService.ObtenerPorIdAsync(id));
    }//Fin del metodo ObtenerPorId

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear(CrearProductoRequest datos)
    {
        var producto = await _productoService.CrearAsync(datos);
        return StatusCode(StatusCodes.Status201Created, producto);
    }//Fin del metodo Crear

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Actualizar(int id, ActualizarProductoRequest datos)
    {
        return Ok(await _productoService.ActualizarAsync(id, datos));
    }//Fin del metodo Actualizar

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _productoService.EliminarAsync(id);
        return NoContent();
    }//Fin del metodo Eliminar

}//Fin de la clase ProductosController
