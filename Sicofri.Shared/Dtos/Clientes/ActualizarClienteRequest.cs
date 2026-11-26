using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Clientes;

/// <summary>
/// Datos para actualizar un cliente (Jefe o Secretaria).
/// Permite agregar después el telefono o el contacto que no se tenian al registrarlo.
/// </summary>

public record ActualizarClienteRequest(
    string Nombre,
    Provincia Provincia,
    string UbicacionDetalle,
    string? Telefono,
    string? NombreContacto);// Fin de la clase ActualizarClienteRequest