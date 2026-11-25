using Sicofri.Shared.Enums;
namespace Sicofri.API.Entities;

/// <summary>
/// Persona que usa la aplicación: Jefe, Secretaria o Empleado.
/// </summary>

public class Usuario
{

    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string ContrasenaHash { get; set; } = string.Empty;
    public Rol Rol { get; set; }
    public bool Activo { get; set; } = true;

    /// <summary>Token de Firebase del celular, para enviarle notificaciones. 
    /// Solo lo usa el Jefe.
    /// </summary>
    public string? TokenFcm { get; set; }

    public ICollection<ViajeEncargado> ViajesEncargados { get; set; } = new List<ViajeEncargado>();
    public ICollection<MovimientoTraslado> Movimientos { get; set; } = new List<MovimientoTraslado>();

}//Fin de la clase