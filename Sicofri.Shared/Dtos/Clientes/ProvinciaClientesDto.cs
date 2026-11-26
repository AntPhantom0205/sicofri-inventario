using Sicofri.Shared.Enums;

namespace Sicofri.Shared.Dtos.Clientes;

/// <summary>
/// Un punto del mapa: una provincia con sus clientes.
/// Se devuelven siempre las 7 provincias, aunque alguna no tenga clientes.
/// </summary>
public record ProvinciaClientesDto(
    Provincia Provincia,
    string NombreProvincia,
    int CantidadClientes,
    int TotalEntregas,
    List<ClienteDto> Clientes); //Fin de la clase ProvinciaClientesDto