using Sicofri.API.Entities;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Repositories.Interfaces;

/// <summary>Acceso a viajes, sus encargados y la asignacion de pedidos.</summary>
public interface IViajeRepository
{
    Task<Viaje?> ObtenerPorIdAsync(int idViaje);
    Task<(List<Viaje> Viajes, int Total)> BuscarAsync(
        EstadoViaje? estado, DateOnly? desde, DateOnly? hasta, int? idEmpleado, int saltar, int tomar);
    Task<List<Usuario>> ObtenerUsuariosAsync(IEnumerable<int> idsUsuarios);
    Task<List<Pedido>> ObtenerPedidosAsync(IEnumerable<int> idsPedidos);
    Task AgregarAsync(Viaje viaje);
    void Eliminar(Viaje viaje);
    void EliminarEncargados(IEnumerable<ViajeEncargado> encargados);
    Task GuardarCambiosAsync();
}//Fin de la interfaz IViajeRepository