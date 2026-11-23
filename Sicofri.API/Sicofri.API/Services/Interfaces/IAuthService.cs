using Sicofri.Shared.Dtos.Auth;
using Sicofri.Shared.Dtos.Usuarios;

namespace Sicofri.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> IniciarSesionAsync(LoginRequest datos);

}//Fin de la interfaz