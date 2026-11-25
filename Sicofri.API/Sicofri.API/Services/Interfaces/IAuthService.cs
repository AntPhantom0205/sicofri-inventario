using Sicofri.Shared.Dtos.Auth;
using Sicofri.Shared.Dtos.Usuarios;

namespace Sicofri.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> IniciarSesionAsync(LoginRequest datos);
    Task RegistrarDispositivoAsync(int idUsuario, string? tokenFcm);

}//Fin de la interfaz