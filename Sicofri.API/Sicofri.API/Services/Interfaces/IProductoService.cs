using Sicofri.Shared.Dtos.Inventario;
using Sicofri.Shared.Dtos.Productos;

namespace Sicofri.API.Services.Interfaces;

public interface IProductoService
{
    Task<List<ProductoDto>> BuscarAsync(string? nombre, int? idUbicacion, bool incluirAgotados);
    Task<ProductoDto> ObtenerPorIdAsync(int idProducto);
    Task<List<ProductoDto>> ObtenerProximosACaducarAsync();
    Task<List<ResumenUbicacionDto>> ObtenerResumenInventarioAsync();
    Task<ProductoDto> CrearAsync(CrearProductoRequest datos);
    Task<ProductoDto> ActualizarAsync(int idProducto, ActualizarProductoRequest datos);
    Task EliminarAsync(int idProducto);

}//Fin de la Interfaz IProductoService