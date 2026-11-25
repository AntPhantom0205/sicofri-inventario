namespace Sicofri.Shared.Dtos.Auth;

/// <summary>
/// Token de Firebase del celular. La app lo envia al iniciar sesion.
/// Enviar null al cerrar sesion para que ese celular deje de recibir avisos.
/// </summary>
public record RegistrarDispositivoRequest(string? TokenFcm);