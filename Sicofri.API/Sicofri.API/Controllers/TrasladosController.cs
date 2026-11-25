using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Security;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Traslados;

namespace Sicofri.API.Controllers;

/// <summary>Traslados internos y bitacora de movimientos. Disponible para todos los roles.</summary>
[ApiController]
[Route("api/traslados")]
[Authorize]
public class TrasladosController : ControllerBase
{
    private readonly ITrasladoService _trasladoService;

    public TrasladosController(ITrasladoService trasladoService)
    {
        _trasladoService = trasladoService;
    }

    /// <summary>Traslada kilos de un lote hacia otra ubicacion.</summary>
    [HttpPost]
    public async Task<ActionResult<MovimientoDto>> Trasladar(CrearTrasladoRequest datos)
    {
        int idUsuario = User.ObtenerIdUsuario();
        var movimiento = await _trasladoService.TrasladarAsync(datos, idUsuario);
        return StatusCode(StatusCodes.Status201Created, movimiento);
    }//Fin del metodo Trasladar

    /// <summary>
    /// Bitacora de traslados, del mas reciente al mas antiguo.
    /// Filtros opcionales: nombre del producto y rango de fechas (formato AAAA-MM-DD).
    /// </summary>
    [HttpGet("bitacora")]
    public async Task<ActionResult<ResultadoPaginado<MovimientoDto>>> ObtenerBitacora(
        [FromQuery] string? producto,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        return Ok(await _trasladoService.ObtenerBitacoraAsync(producto, desde, hasta, pagina, tamanoPagina));
    }//Fin del metodo ObtenerBitacora
}//Fin de la clase TrasladosController