using Sicofri.Shared.Dtos.Clientes;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Services.Interfaces;

public interface IClienteService
{
    Task<List<ClienteDto>> BuscarAsync(string? nombre, Provincia? provincia);
    Task<ClienteDto> ObtenerPorIdAsync(int idCliente);
    Task<List<ProvinciaClientesDto>> ObtenerPorProvinciaAsync();
    Task<ClienteDto> CrearAsync(CrearClienteRequest datos);
    Task<ClienteDto> ActualizarAsync(int idCliente, ActualizarClienteRequest datos);
    Task EliminarAsync(int idCliente);
}//Fin de la interfaz IClienteService