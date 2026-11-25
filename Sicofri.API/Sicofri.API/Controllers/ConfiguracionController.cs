using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Configuracion;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Controllers;

/// <summary>
/// Parametros del sistema (pantalla de Configuración).
/// </summary>
/// 
[ApiController]
[Route("api/configuracion")]
[Authorize]
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionService _configuracionService;

    public ConfiguracionController(IConfiguracionService configuracionService)
    {
        _configuracionService = configuracionService;
    }

    /// <summary>
    /// Reglas de caducidad vigentes. Disponible para todos los roles.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ConfiguracionDto>> Obtener()
    {
        return Ok(await _configuracionService.ObtenerAsync());
    }

    /// <summary>Cambia los dias de anticipacion. Solo el Jefe.</summary>
    [HttpPut]
    [Authorize(Roles = nameof(Rol.Jefe))]
    public async Task<ActionResult<ConfiguracionDto>> Actualizar(ActualizarConfiguracionRequest datos)
    {
        return Ok(await _configuracionService.ActualizarAsync(datos));
    }
}//Fin de la clase ConfiguracionController