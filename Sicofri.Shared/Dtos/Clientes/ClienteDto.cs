using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Clientes;

/// <summary>
/// Un cliente tal como lo ve la app, con la cantidad de entregas que ha recibido.
/// </summary>

public record ClienteDto(
    int IdCliente,
    string Nombre,
    Provincia Provincia,
    string NombreProvincia,
    string UbicacionDetalle,
    string? Telefono,
    string? NombreContacto,
    int EntregasRecibidas);//Fin de la clase ClienteDto