using Sicofri.API.Entities;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(int idCliente);
    Task<List<Cliente>> BuscarAsync(string? nombre, Provincia? provincia);
    Task<Dictionary<int, int>> ContarEntregasPorClienteAsync();
    Task<bool> ExisteDuplicadoAsync(string nombre, Provincia provincia, int? excluirIdCliente = null);
    Task<bool> TienePedidosAsync(int idCliente);
    Task AgregarAsync(Cliente cliente);
    void Eliminar(Cliente cliente);
    Task GuardarCambiosAsync();
}//Fin de la interfaz IClienteRepository