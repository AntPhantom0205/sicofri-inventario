using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Clientes;

/// <summary>
/// Datos para registrar un cliente. Telefono y contacto son opcionales.
/// </summary>
public record CrearClienteRequest(
    string Nombre,
    Provincia Provincia,
    string UbicacionDetalle,
    string? Telefono,
    string? NombreContacto);//Fin de la clase CrearClienteRequest