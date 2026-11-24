using Sicofri.API.Entities;

namespace Sicofri.API.Repositories.Interfaces;

/// <summary>
/// Acceso a lotes de producto y a las ubicaciones donde se almacenan.
/// </summary>

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(int idProducto);
    Task<List<Producto>> BuscarAsync(string? nombre, int? idUbicacion, bool incluirAgotados);
    Task<List<Producto>> ObtenerConCaducidadHastaAsync(DateOnly fechaLimite);
    Task<bool> TieneMovimientosOPedidosAsync(int idProducto);
    Task<Ubicacion?> ObtenerUbicacionAsync(int idUbicacion);
    Task<List<Ubicacion>> ObtenerUbicacionesConExistenciasAsync();
    Task AgregarAsync(Producto producto);
    void Eliminar(Producto producto);
    Task GuardarCambiosAsync();

}//Fin de la interfaz IProductoRepository
