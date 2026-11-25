using Microsoft.EntityFrameworkCore.Storage;
using Sicofri.API.Entities;

namespace Sicofri.API.Repositories.Interfaces;

/// <summary>Operaciones de base de datos para traslados y su bitacora.</summary>
public interface ITrasladoRepository
{
    Task<Producto?> ObtenerLoteAsync(int idProducto);
    Task<Ubicacion?> ObtenerUbicacionAsync(int idUbicacion);
    Task<string?> ObtenerNombreUsuarioAsync(int idUsuario);
    Task<Producto?> BuscarLoteGemeloAsync(Producto origen, int idUbicacionDestino);
    Task<bool> DescontarAsync(int idProducto, decimal cantidad);
    Task SumarAsync(int idProducto, decimal cantidad);
    Task AgregarLoteAsync(Producto lote);
    Task AgregarMovimientoAsync(MovimientoTraslado movimiento);
    Task<(List<MovimientoTraslado> Movimientos, int Total)> ObtenerBitacoraAsync(
        string? nombreProducto, DateTime? desdeUtc, DateTime? hastaUtc, int saltar, int tomar);
    Task<IDbContextTransaction> IniciarTransaccionAsync();
    Task GuardarCambiosAsync();
}//Fin de la interfaz ITrasladoRepository