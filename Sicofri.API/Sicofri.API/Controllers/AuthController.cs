using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Auth;

namespace Sicofri.API.Controllers;

/// <summary>Inicio de sesión.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Valida usuario y contraseña y devuelve un token JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest datos)
    {
        var respuesta = await _authService.IniciarSesionAsync(datos);
        return Ok(respuesta);
    }//Fin del método Login

}//Fin de la clase