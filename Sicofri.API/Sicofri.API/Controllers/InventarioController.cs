using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Inventario;
using Sicofri.Shared.Dtos.Productos;

namespace Sicofri.API.Controllers;

/// <summary>
/// Inventario por ubicacion (los 5 congeladores y la camara de San José).
/// </summary>
[ApiController]
[Route("api/inventario")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly IProductoService _productoService;

    public InventarioController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    /// <summary>Resumen de cada ubicacion: lotes, kilos totales y lotes en riesgo.</summary>
    [HttpGet("ubicaciones")]
    public async Task<ActionResult<List<ResumenUbicacionDto>>> ObtenerResumen()
    {
        return Ok(await _productoService.ObtenerResumenInventarioAsync());
    }//Fin del metodo ObtenerResumen

    /// <summary>Lotes con existencia en una ubicacion especifica.</summary>
    [HttpGet("ubicaciones/{idUbicacion:int}/productos")]
    public async Task<ActionResult<List<ProductoDto>>> ObtenerProductosDeUbicacion(int idUbicacion)
    {
        return Ok(await _productoService.BuscarAsync(null, idUbicacion, incluirAgotados: false));
    }//Fin del metodo ObtenerProductosDeUbicacion
}//Fin del metodo InventarioController
