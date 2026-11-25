using Sicofri.API.Entities;
using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Traslados;

namespace Sicofri.API.Services;

/// <summary>
/// Traslados internos entre las 6 ubicaciones. Un traslado parcial divide el lote:
/// el origen conserva el resto y el destino recibe los kilos trasladados.
/// </summary>
public class TrasladoService : ITrasladoService
{
    private const int TamanoPaginaPorDefecto = 20;
    private const int TamanoPaginaMaximo = 100;

    private readonly ITrasladoRepository _traslados;

    public TrasladoService(ITrasladoRepository traslados)
    {
        _traslados = traslados;
    }

    public async Task<MovimientoDto> TrasladarAsync(CrearTrasladoRequest datos, int idUsuario)
    {
        //1. Validaciones antes de tocar nada

        if (datos.Cantidad <= 0)
            throw new ReglaNegocioException("La cantidad a trasladar debe ser mayor a 0 kg.");

        if (decimal.Round(datos.Cantidad, 2) != datos.Cantidad)
            throw new ReglaNegocioException("La cantidad admite máximo 2 decimales.");

        var origen = await _traslados.ObtenerLoteAsync(datos.IdProducto)
            ?? throw new ReglaNegocioException("El lote seleccionado no existe.", StatusCodes.Status404NotFound);

        var destino = await _traslados.ObtenerUbicacionAsync(datos.IdUbicacionDestino)
            ?? throw new ReglaNegocioException("La ubicación de destino no existe.");

        if (destino.IdUbicacion == origen.IdUbicacion)
            throw new ReglaNegocioException(
                $"La ubicación de destino debe ser distinta a la de origen ({origen.Ubicacion.Nombre}).");

        if (datos.Cantidad > origen.Cantidad)
            throw new ReglaNegocioException(
                $"No hay suficiente producto. Disponible en {origen.Ubicacion.Nombre}: {origen.Cantidad:0.##} kg.");

        string nombreResponsable = await _traslados.ObtenerNombreUsuarioAsync(idUsuario)
            ?? throw new ReglaNegocioException("El usuario de la sesión no existe o está inactivo.",
                StatusCodes.Status401Unauthorized);

        //2. Transaccion: todo o nada

        await using var transaccion = await _traslados.IniciarTransaccionAsync();

        bool descontado = await _traslados.DescontarAsync(origen.IdProducto, datos.Cantidad);
        if (!descontado)
        {
            // Otro usuario modifico el lote entre la validacion y este momento.
            throw new ReglaNegocioException(
                "Otro usuario modificó este lote al mismo tiempo y ya no hay suficiente producto. Actualice la pantalla e intente de nuevo.",
                StatusCodes.Status409Conflict);
        }

        var movimiento = new MovimientoTraslado
        {
            Cantidad = datos.Cantidad,
            FechaHora = DateTime.UtcNow,
            IdProductoOrigen = origen.IdProducto,
            IdUbicacionOrigen = origen.IdUbicacion,
            IdUbicacionDestino = destino.IdUbicacion,
            IdUsuario = idUsuario
        };

        var gemelo = await _traslados.BuscarLoteGemeloAsync(origen, destino.IdUbicacion);

        if (gemelo is not null)
        {
            // Ya existe el mismo lote en el destino: se le suman los kilos.
            await _traslados.SumarAsync(gemelo.IdProducto, datos.Cantidad);
            movimiento.IdProductoDestino = gemelo.IdProducto;
        }
        else
        {
            // Se crea un lote nuevo en el destino, identico al de origen salvo la cantidad.
            var nuevoLote = new Producto
            {
                Nombre = origen.Nombre,
                NombreProveedor = origen.NombreProveedor,
                TipoLote = origen.TipoLote,
                CodigoLote = origen.CodigoLote,
                Cantidad = datos.Cantidad,
                FechaIngreso = origen.FechaIngreso,
                FechaProduccion = origen.FechaProduccion,
                FechaCaducidad = origen.FechaCaducidad,
                IdUbicacion = destino.IdUbicacion
            };

            await _traslados.AgregarLoteAsync(nuevoLote);
            movimiento.ProductoDestino = nuevoLote;
        }

        await _traslados.AgregarMovimientoAsync(movimiento);
        await _traslados.GuardarCambiosAsync();
        await transaccion.CommitAsync();

        return new MovimientoDto(
            movimiento.IdMovimiento,
            HoraCostaRica.ALocal(movimiento.FechaHora),
            origen.Nombre,
            movimiento.Cantidad,
            origen.Ubicacion.Nombre,
            destino.Nombre,
            nombreResponsable,
            movimiento.IdProductoOrigen,
            movimiento.IdProductoDestino);
    }//Fin del metodo TrasladarAsync

    public async Task<ResultadoPaginado<MovimientoDto>> ObtenerBitacoraAsync(
        string? producto, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new ReglaNegocioException("La fecha 'desde' no puede ser posterior a la fecha 'hasta'.");

        pagina = Math.Max(pagina, 1);
        tamanoPagina = tamanoPagina is < 1 or > TamanoPaginaMaximo ? TamanoPaginaPorDefecto : tamanoPagina;

        // "hasta" incluye todo ese día: se filtra hasta la medianoche del día siguiente.
        DateTime? desdeUtc = desde.HasValue ? HoraCostaRica.InicioDelDiaUtc(desde.Value) : null;
        DateTime? hastaUtc = hasta.HasValue ? HoraCostaRica.InicioDelDiaUtc(hasta.Value.AddDays(1)) : null;

        var (movimientos, total) = await _traslados.ObtenerBitacoraAsync(
            producto, desdeUtc, hastaUtc, (pagina - 1) * tamanoPagina, tamanoPagina);

        var elementos = movimientos.Select(m => new MovimientoDto(
            m.IdMovimiento,
            HoraCostaRica.ALocal(m.FechaHora),
            m.ProductoOrigen.Nombre,
            m.Cantidad,
            m.UbicacionOrigen.Nombre,
            m.UbicacionDestino.Nombre,
            m.Usuario.Nombre,
            m.IdProductoOrigen,
            m.IdProductoDestino)).ToList();

        return new ResultadoPaginado<MovimientoDto>(elementos, total, pagina, tamanoPagina);
    }//Fin del metodo ObtenerBitacoraAsync
}//Fin de la clase TrasladoService