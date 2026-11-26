using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Security;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Notificaciones;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Controllers;

/// <summary>
/// Notificaciones de caducidad. Las consultan y atienden el Jefe y la Secretaria;
/// el aviso push solo le llega al Jefe.
/// </summary>
[ApiController]
[Route("api/notificaciones")]
[Authorize(Roles = $"{nameof(Rol.Jefe)},{nameof(Rol.Secretaria)}")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificacionService;

    public NotificacionesController(INotificacionService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    /// <summary>
    /// Bitacora de notificaciones: pendientes primero. Filtro opcional 
    /// atendida=true/false.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<NotificacionDto>>> Buscar(
        [FromQuery] bool? atendida,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20)
    {
        return Ok(await _notificacionService.BuscarAsync(atendida, pagina, tamanoPagina));
    }//Fin del metodo Buscar

    /// <summary>Cantidad de pendientes: el numero rojo de la campanita.</summary>
    [HttpGet("pendientes/cantidad")]
    public async Task<ActionResult<int>> ContarPendientes()
    {
        return Ok(await _notificacionService.ContarPendientesAsync());
    }//Fin del metodo ContarPendientes

    /// <summary>Marca la notificacion como atendida y registra quien y cuando.</summary>
    [HttpPut("{id:int}/atender")]
    public async Task<ActionResult<NotificacionDto>> Atender(int id)
    {
        return Ok(await _notificacionService.AtenderAsync(id, User.ObtenerIdUsuario()));
    }//Fin del metodo Atender

    /// <summary>Ejecuta la revision de caducidades en este momento. Solo el Jefe.</summary>
    [HttpPost("revisar")]
    [Authorize(Roles = nameof(Rol.Jefe))]
    public async Task<ActionResult<ResultadoRevisionDto>> Revisar()
    {
        return Ok(await _notificacionService.RevisarAsync());
    }//Fin del metodo Revisar
}//Fin de la clase NotificacionesController