using Sicofri.API.Entities;
using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Pedidos;
using Sicofri.Shared.Enums;
using Sicofri.Shared.Utilidades;

namespace Sicofri.API.Services;

/// <summary>
/// Pedidos a clientes. Armar descuenta inventario; actualizar lo recalcula;
/// cancelar lo devuelve. Las tres operaciones son transaccionales.
/// </summary>
public class PedidoService : IPedidoService
{
    private const int MaximoLineas = 50;
    private const int TamanoPaginaPorDefecto = 20;
    private const int TamanoPaginaMaximo = 100;

    private readonly IPedidoRepository _pedidos;

    public PedidoService(IPedidoRepository pedidos)
    {
        _pedidos = pedidos;
    }

    //Consultas

    public async Task<ResultadoPaginado<PedidoResumenDto>> BuscarAsync(
        EstadoPedido? estado, string? busqueda, int pagina, int tamanoPagina)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = tamanoPagina is < 1 or > TamanoPaginaMaximo ? TamanoPaginaPorDefecto : tamanoPagina;

        // La busqueda puede ser un numero de pedido ("PE-000012" o "12") o
        // parte del nombre del cliente.
        int? idPedido = NumeroPedido.TryObtenerId(busqueda, out int id) ? id : null;
        string? nombreCliente = idPedido is null ? busqueda : null;

        var (pedidos, total) = await _pedidos.BuscarAsync(
            estado, idPedido, nombreCliente, (pagina - 1) * tamanoPagina, tamanoPagina);

        var elementos = pedidos.Select(p => new PedidoResumenDto(
            p.IdPedido,
            NumeroPedido.Formatear(p.IdPedido),
            p.Estado,
            p.Cliente.Nombre,
            p.FechaEntrega,
            p.IdViaje,
            p.Lineas.Sum(l => l.Cantidad),
            p.Lineas.Count)).ToList();

        return new ResultadoPaginado<PedidoResumenDto>(elementos, total, pagina, tamanoPagina);
    }//Fin del metodo BuscarAsync

    public async Task<PedidoDto> ObtenerPorIdAsync(int idPedido)
    {
        return ConvertirADto(await ObtenerOFallarAsync(idPedido));
    }//Fin del metodo ObtenerPorIdAsync

    //Armar

    public async Task<PedidoDto> CrearAsync(CrearPedidoRequest datos, int idUsuario)
    {
        DateOnly hoy = ReglasCaducidad.HoyEnCostaRica();

        await ValidarClienteAsync(datos.IdCliente);
        ValidarFechaEntrega(datos.FechaEntrega, hoy);
        var lineas = ValidarFormatoLineas(datos.Lineas);

        // Al armar no hay kilos "propios" que devolver: se valida contra lo
        // disponible.
        await ValidarDisponibilidadAsync(lineas, datos.FechaEntrega, hoy, kilosQueSeDevuelven: new());

        await using var transaccion = await _pedidos.IniciarTransaccionAsync();

        await DescontarLineasAsync(lineas);

        var pedido = new Pedido
        {
            IdCliente = datos.IdCliente,
            FechaEntrega = datos.FechaEntrega,
            Estado = EstadoPedido.Armado,
            FechaHoraCreacion = DateTime.UtcNow,
            IdUsuarioCreador = idUsuario
        };

        foreach (var linea in lineas)
            pedido.Lineas.Add(new LineaPedido { IdProducto = linea.IdProducto, Cantidad = linea.Cantidad });

        await _pedidos.AgregarAsync(pedido);
        await _pedidos.GuardarCambiosAsync();
        await transaccion.CommitAsync();

        return ConvertirADto(await ObtenerOFallarAsync(pedido.IdPedido));
    }//Fin del metodo CrearAsync

    //Actualizar

    public async Task<PedidoDto> ActualizarAsync(int idPedido, ActualizarPedidoRequest datos)
    {
        DateOnly hoy = ReglasCaducidad.HoyEnCostaRica();
        var pedido = await ObtenerOFallarAsync(idPedido);

        ValidarEstadoArmado(pedido, "modificar");
        await ValidarClienteAsync(datos.IdCliente);
        ValidarFechaEntrega(datos.FechaEntrega, hoy);
        var lineasNuevas = ValidarFormatoLineas(datos.Lineas);

        // Los kilos de las líneas actuales van a regresar a sus lotes:
        // cuentan como disponibles para las lineas nuevas.
        var kilosQueSeDevuelven = pedido.Lineas
            .GroupBy(l => l.IdProducto)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Cantidad));

        await ValidarDisponibilidadAsync(lineasNuevas, datos.FechaEntrega, hoy, kilosQueSeDevuelven);

        await using var transaccion = await _pedidos.IniciarTransaccionAsync();

        // 1. Devolver los kilos de las lineas viejas.
        foreach (var linea in pedido.Lineas)
            await _pedidos.DevolverKilosAsync(linea.IdProducto, linea.Cantidad);

        // 2. Descontar los kilos de las lineas nuevas.
        await DescontarLineasAsync(lineasNuevas);

        // 3. Reemplazar las lineas y el encabezado.
        var lineasViejas = pedido.Lineas.ToList();
        _pedidos.EliminarLineas(lineasViejas);
        pedido.Lineas.Clear();

        foreach (var linea in lineasNuevas)
            pedido.Lineas.Add(new LineaPedido { IdProducto = linea.IdProducto, Cantidad = linea.Cantidad });

        pedido.IdCliente = datos.IdCliente;
        pedido.FechaEntrega = datos.FechaEntrega;

        await _pedidos.GuardarCambiosAsync();
        await transaccion.CommitAsync();

        return ConvertirADto(await ObtenerOFallarAsync(idPedido));
    }//Fin del metodo ActualizarAsync

    //Cancelar

    public async Task<PedidoDto> CancelarAsync(int idPedido, int idUsuario)
    {
        var pedido = await ObtenerOFallarAsync(idPedido);
        ValidarEstadoArmado(pedido, "cancelar");

        await using var transaccion = await _pedidos.IniciarTransaccionAsync();

        // Cada kilo regresa a su lote original.
        foreach (var linea in pedido.Lineas)
            await _pedidos.DevolverKilosAsync(linea.IdProducto, linea.Cantidad);

        pedido.Estado = EstadoPedido.Cancelado;
        pedido.FechaHoraCancelado = DateTime.UtcNow;
        pedido.IdUsuarioCancela = idUsuario;
        pedido.IdViaje = null; // Si estaba asignado a un viaje Programado,
                               // queda libre.

        await _pedidos.GuardarCambiosAsync();
        await transaccion.CommitAsync();

        return ConvertirADto(await ObtenerOFallarAsync(idPedido));
    }//Fin del metodo CancelarAsync

    //Validaciones

    private async Task ValidarClienteAsync(int idCliente)
    {
        if (await _pedidos.ObtenerClienteAsync(idCliente) is null)
            throw new ReglaNegocioException("El cliente seleccionado no existe.");
    }//Fin del metodo ValidarClienteAsync

    private static void ValidarFechaEntrega(DateOnly fechaEntrega, DateOnly hoy)
    {
        if (fechaEntrega < hoy)
            throw new ReglaNegocioException("La fecha de entrega no puede ser una fecha pasada.");
    }//Fin del metodo ValidarFechaEntrega

    private static void ValidarEstadoArmado(Pedido pedido, string accion)
    {
        if (pedido.Estado != EstadoPedido.Armado)
        {
            throw new ReglaNegocioException(
                $"Solo se puede {accion} un pedido en estado Armado. Estado actual: {pedido.Estado}.",
                StatusCodes.Status409Conflict);
        }
    }//Fin del metodo ValidarEstadoArmado

    /// <summary>
    /// Revisa la forma de las lineas: que haya, que no se repitan lotes y
    /// que los kilos sean validos.
    /// </summary>
    private static List<LineaPedidoRequest> ValidarFormatoLineas(List<LineaPedidoRequest>? lineas)
    {
        if (lineas is null || lineas.Count == 0)
            throw new ReglaNegocioException("El pedido debe tener al menos una línea de producto.");

        if (lineas.Count > MaximoLineas)
            throw new ReglaNegocioException($"Un pedido puede tener máximo {MaximoLineas} líneas.");

        var repetido = lineas.GroupBy(l => l.IdProducto).FirstOrDefault(g => g.Count() > 1);
        if (repetido is not null)
        {
            throw new ReglaNegocioException(
                $"El lote {repetido.Key} aparece más de una vez. Use una sola línea y aumente su cantidad.");
        }

        foreach (var linea in lineas)
        {
            if (linea.Cantidad <= 0)
                throw new ReglaNegocioException("La cantidad de cada línea debe ser mayor a 0 kg.");

            if (decimal.Round(linea.Cantidad, 2) != linea.Cantidad)
                throw new ReglaNegocioException("Las cantidades admiten máximo 2 decimales.");
        }

        return lineas;
    }//Fin del metodo ValidarFormatoLineas

    /// <summary>
    /// Revisa cada lote: que exista, que alcance y que no venza antes de la entrega.
    /// kilosQueSeDevuelven: al actualizar, los kilos que el propio pedido ya tenia
    /// apartados de ese lote.
    /// </summary>
    private async Task ValidarDisponibilidadAsync(List<LineaPedidoRequest> lineas, DateOnly fechaEntrega,
        DateOnly hoy, Dictionary<int, decimal> kilosQueSeDevuelven)
    {
        var lotes = (await _pedidos.ObtenerLotesAsync(lineas.Select(l => l.IdProducto)))
            .ToDictionary(p => p.IdProducto);

        foreach (var linea in lineas)
        {
            if (!lotes.TryGetValue(linea.IdProducto, out var lote))
                throw new ReglaNegocioException($"El lote {linea.IdProducto} no existe.");

            decimal disponible = lote.Cantidad + kilosQueSeDevuelven.GetValueOrDefault(lote.IdProducto, 0);
            if (linea.Cantidad > disponible)
            {
                throw new ReglaNegocioException(
                    $"No hay suficiente '{lote.Nombre}' en {lote.Ubicacion.Nombre}. Disponible: {disponible:0.##} kg.");
            }

            if (lote.FechaCaducidad < hoy)
            {
                throw new ReglaNegocioException(
                    $"El lote de '{lote.Nombre}' ({lote.Ubicacion.Nombre}) está vencido desde el {lote.FechaCaducidad:dd/MM/yyyy}.");
            }

            if (lote.FechaCaducidad < fechaEntrega)
            {
                throw new ReglaNegocioException(
                    $"El lote de '{lote.Nombre}' ({lote.Ubicacion.Nombre}) vence el {lote.FechaCaducidad:dd/MM/yyyy}, antes de la fecha de entrega.");
            }
        }
    }//Fin del metodo ValidarDisponibilidadAsync

    /// <summary>
    /// Descuenta cada linea con la proteccion "solo si alcanza". Si una falla,
    /// la transaccion se deshace completa.
    /// </summary>
    private async Task DescontarLineasAsync(List<LineaPedidoRequest> lineas)
    {
        foreach (var linea in lineas)
        {
            if (!await _pedidos.DescontarKilosAsync(linea.IdProducto, linea.Cantidad))
            {
                throw new ReglaNegocioException(
                    $"Otro usuario modificó el lote {linea.IdProducto} al mismo tiempo y ya no alcanza. Actualice e intente de nuevo.",
                    StatusCodes.Status409Conflict);
            }
        }
    }//Fin del metodo DescontarLineasAsync

    //Conversion

    private async Task<Pedido> ObtenerOFallarAsync(int idPedido)
    {
        return await _pedidos.ObtenerPorIdAsync(idPedido)
            ?? throw new ReglaNegocioException("El pedido no existe.", StatusCodes.Status404NotFound);
    }//Fin del metodo ObtenerOFallarAsync

    private static PedidoDto ConvertirADto(Pedido p)
    {
        var lineas = p.Lineas
            .OrderBy(l => l.IdDetalle)
            .Select(l => new LineaPedidoDto(
                l.IdDetalle,
                l.IdProducto,
                l.Producto.Nombre,
                l.Producto.CodigoLote,
                l.Cantidad,
                l.Producto.Ubicacion.Nombre,
                l.Producto.FechaCaducidad))
            .ToList();

        return new PedidoDto(
            p.IdPedido,
            NumeroPedido.Formatear(p.IdPedido),
            p.Estado,
            p.IdCliente,
            p.Cliente.Nombre,
            p.Cliente.Provincia.NombreParaMostrar(),
            p.FechaEntrega,
            p.IdViaje,
            lineas.Sum(l => l.Cantidad),
            HoraCostaRica.ALocal(p.FechaHoraCreacion),
            p.UsuarioCreador.Nombre,
            p.FechaHoraEntregado.HasValue ? HoraCostaRica.ALocal(p.FechaHoraEntregado.Value) : null,
            p.UsuarioEntrega?.Nombre,
            p.FechaHoraCancelado.HasValue ? HoraCostaRica.ALocal(p.FechaHoraCancelado.Value) : null,
            p.UsuarioCancela?.Nombre,
            lineas);
    }//Fin del metodo ConvertirADto
}//Fin de la clase PedidoService