using Sicofri.API.Entities;
using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Pedidos;
using Sicofri.Shared.Dtos.Viajes;
using Sicofri.Shared.Enums;
using Sicofri.Shared.Utilidades;

namespace Sicofri.API.Services;

/// <summary>
/// Viajes de entrega: programarlos (Jefe/Secretaria) y ejecutarlos (encargados).
/// Flujo: Programado → EnCurso → Finalizado (automático al entregar el ultimo pedido).
/// </summary>
public class ViajeService : IViajeService
{
    private const int MinimoEncargados = 1;
    private const int MaximoEncargados = 3;
    private const int TamanoPaginaPorDefecto = 20;
    private const int TamanoPaginaMaximo = 100;

    private readonly IViajeRepository _viajes;

    public ViajeService(IViajeRepository viajes)
    {
        _viajes = viajes;
    }

    //Consultas

    public async Task<ResultadoPaginado<ViajeResumenDto>> BuscarAsync(
        EstadoViaje? estado, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new ReglaNegocioException("La fecha 'desde' no puede ser posterior a la fecha 'hasta'.");

        pagina = Math.Max(pagina, 1);
        tamanoPagina = tamanoPagina is < 1 or > TamanoPaginaMaximo ? TamanoPaginaPorDefecto : tamanoPagina;

        var (viajes, total) = await _viajes.BuscarAsync(
            estado, desde, hasta, idEmpleado: null, (pagina - 1) * tamanoPagina, tamanoPagina);

        var elementos = viajes.Select(ConvertirAResumen).ToList();
        return new ResultadoPaginado<ViajeResumenDto>(elementos, total, pagina, tamanoPagina);
    }//Fin del metodo BuscarAsync

    public async Task<ViajeDto> ObtenerPorIdAsync(int idViaje)
    {
        return ConvertirADto(await ObtenerOFallarAsync(idViaje));
    }//Fin del metodo ObtenerPorIdAsync

    //Programar

    public async Task<ViajeDto> CrearAsync(CrearViajeRequest datos, int idUsuario)
    {
        DateOnly hoy = ReglasCaducidad.HoyEnCostaRica();
        ValidarFechaViaje(datos.Fecha, hoy);

        var encargados = await ValidarEncargadosAsync(datos.IdsEncargados);
        var pedidos = await ValidarPedidosAsync(datos.IdsPedidos, datos.Fecha, idViajeActual: null);

        var viaje = new Viaje
        {
            Fecha = datos.Fecha,
            Estado = EstadoViaje.Programado,
            IdUsuarioCreador = idUsuario
        };

        foreach (var empleado in encargados)
            viaje.Encargados.Add(new ViajeEncargado { IdUsuario = empleado.IdUsuario });

        foreach (var pedido in pedidos)
            viaje.Pedidos.Add(pedido);

        await _viajes.AgregarAsync(viaje);
        await _viajes.GuardarCambiosAsync();

        return ConvertirADto(await ObtenerOFallarAsync(viaje.IdViaje));
    }//Fin del metodo CrearAsync

    public async Task<ViajeDto> ActualizarAsync(int idViaje, ActualizarViajeRequest datos)
    {
        DateOnly hoy = ReglasCaducidad.HoyEnCostaRica();
        var viaje = await ObtenerOFallarAsync(idViaje);

        ValidarEstado(viaje, EstadoViaje.Programado, "modificar");
        ValidarFechaViaje(datos.Fecha, hoy);

        var encargadosNuevos = await ValidarEncargadosAsync(datos.IdsEncargados);
        var pedidosNuevos = await ValidarPedidosAsync(datos.IdsPedidos, datos.Fecha, idViajeActual: idViaje);

        // --- Encargados: se quitan los que ya no van y se agregan los nuevos (sin tocar los que se quedan). ---
        var idsNuevos = encargadosNuevos.Select(e => e.IdUsuario).ToHashSet();
        var idsActuales = viaje.Encargados.Select(e => e.IdUsuario).ToHashSet();

        var aQuitar = viaje.Encargados.Where(e => !idsNuevos.Contains(e.IdUsuario)).ToList();
        _viajes.EliminarEncargados(aQuitar);
        foreach (var encargado in aQuitar)
            viaje.Encargados.Remove(encargado);

        foreach (var idEmpleado in idsNuevos.Where(id => !idsActuales.Contains(id)))
            viaje.Encargados.Add(new ViajeEncargado { IdViaje = idViaje, IdUsuario = idEmpleado });

        // --- Pedidos: los que se quitan quedan libres; los nuevos se asignan. ---
        var idsPedidosNuevos = pedidosNuevos.Select(p => p.IdPedido).ToHashSet();

        foreach (var pedido in viaje.Pedidos.Where(p => !idsPedidosNuevos.Contains(p.IdPedido)).ToList())
        {
            pedido.IdViaje = null;
            viaje.Pedidos.Remove(pedido);
        }

        foreach (var pedido in pedidosNuevos.Where(p => p.IdViaje != idViaje))
            viaje.Pedidos.Add(pedido);

        viaje.Fecha = datos.Fecha;

        await _viajes.GuardarCambiosAsync();

        return ConvertirADto(await ObtenerOFallarAsync(idViaje));
    }//Fin del metodo ActualizarAsync

    public async Task EliminarAsync(int idViaje)
    {
        var viaje = await ObtenerOFallarAsync(idViaje);
        ValidarEstado(viaje, EstadoViaje.Programado, "eliminar");

        // Sus pedidos quedan libres (siguen Armados); sus encargados se borran
        // con el viaje.
        foreach (var pedido in viaje.Pedidos)
            pedido.IdViaje = null;

        _viajes.Eliminar(viaje);
        await _viajes.GuardarCambiosAsync();
    }//Fin del metodo EliminarAsync

    //Ejecutar

    public async Task<ViajeDto> IniciarAsync(int idViaje, int idUsuario, bool esJefe)
    {
        var viaje = await ObtenerOFallarAsync(idViaje);

        ValidarEstado(viaje, EstadoViaje.Programado, "iniciar");
        ValidarEsEncargado(viaje, idUsuario, esJefe);

        if (viaje.Fecha > ReglasCaducidad.HoyEnCostaRica())
        {
            throw new ReglaNegocioException(
                $"El viaje está programado para el {viaje.Fecha:dd/MM/yyyy}; no se puede iniciar antes de esa fecha.");
        }

        if (viaje.Pedidos.Count == 0)
            throw new ReglaNegocioException("El viaje no tiene pedidos asignados.");

        viaje.Estado = EstadoViaje.EnCurso;
        viaje.FechaHoraInicio = DateTime.UtcNow;

        foreach (var pedido in viaje.Pedidos)
            pedido.Estado = EstadoPedido.EnViaje;

        await _viajes.GuardarCambiosAsync();

        return ConvertirADto(viaje);
    }//Fin del metodo IniciarAsync

    public async Task<ViajeDto> EntregarPedidoAsync(int idViaje, int idPedido, int idUsuario, bool esJefe)
    {
        var viaje = await ObtenerOFallarAsync(idViaje);

        ValidarEstado(viaje, EstadoViaje.EnCurso, "registrar entregas en");
        ValidarEsEncargado(viaje, idUsuario, esJefe);

        var pedido = viaje.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido)
            ?? throw new ReglaNegocioException(
                $"El pedido {NumeroPedido.Formatear(idPedido)} no pertenece a este viaje.", StatusCodes.Status404NotFound);

        if (pedido.Estado != EstadoPedido.EnViaje)
        {
            throw new ReglaNegocioException(
                $"El pedido {NumeroPedido.Formatear(idPedido)} ya está en estado {pedido.Estado}.", StatusCodes.Status409Conflict);
        }

        DateTime ahora = DateTime.UtcNow;
        pedido.Estado = EstadoPedido.Entregado;
        pedido.FechaHoraEntregado = ahora;
        pedido.IdUsuarioEntrega = idUsuario;

        // Si era el ultimo pendiente, el viaje termina solo.
        if (viaje.Pedidos.All(p => p.Estado == EstadoPedido.Entregado))
        {
            viaje.Estado = EstadoViaje.Finalizado;
            viaje.FechaHoraFin = ahora;
        }

        await _viajes.GuardarCambiosAsync();

        // Se vuelve a consultar para traer el nombre de quien entrego.
        return ConvertirADto(await ObtenerOFallarAsync(idViaje));
    }//Fin del metodo EntregarPedidoAsync

    //Mis entregas y bitacora 

    /// <summary>
    /// Viajes pendientes del empleado: primero el que esta en curso,
    /// despues los programados, del mas próximo al mas lejano.
    /// </summary>
    public async Task<List<ViajeDto>> ObtenerMisEntregasAsync(int idEmpleado)
    {
        var viajes = await _viajes.ObtenerActivosDeEmpleadoAsync(idEmpleado);

        return viajes
            .OrderBy(v => v.Estado == EstadoViaje.EnCurso ? 0 : 1)
            .ThenBy(v => v.Fecha)
            .ThenBy(v => v.IdViaje)
            .Select(ConvertirADto)
            .ToList();
    }

    public async Task<ResultadoPaginado<BitacoraViajeDto>> ObtenerBitacoraAsync(
        EstadoViaje? estado, DateOnly? desde, DateOnly? hasta, int? idEmpleado, int pagina, int tamanoPagina)
    {
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            throw new ReglaNegocioException("La fecha 'desde' no puede ser posterior a la fecha 'hasta'.");

        pagina = Math.Max(pagina, 1);
        tamanoPagina = tamanoPagina is < 1 or > TamanoPaginaMaximo ? TamanoPaginaPorDefecto : tamanoPagina;

        var (viajes, total) = await _viajes.BuscarParaBitacoraAsync(
            estado, desde, hasta, idEmpleado, (pagina - 1) * tamanoPagina, tamanoPagina);

        var elementos = viajes.Select(ConvertirABitacora).ToList();
        return new ResultadoPaginado<BitacoraViajeDto>(elementos, total, pagina, tamanoPagina);
    }//Fin del metodo ObtenerBitacoraAsync

    //Validaciones

    private static void ValidarFechaViaje(DateOnly fecha, DateOnly hoy)
    {
        if (fecha < hoy)
            throw new ReglaNegocioException("La fecha del viaje no puede ser una fecha pasada.");
    }//Fin del metodo ValidarFechaViaje

    private static void ValidarEstado(Viaje viaje, EstadoViaje estadoRequerido, string accion)
    {
        if (viaje.Estado != estadoRequerido)
        {
            throw new ReglaNegocioException(
                $"No se puede {accion} el viaje {NumeroViaje.Formatear(viaje.IdViaje)}: está {viaje.Estado} (debe estar {estadoRequerido}).",
                StatusCodes.Status409Conflict);
        }
    }//Fin del metodo ValidarEstado

    /// <summary>
    /// Solo los encargados del viaje (o el Jefe) pueden iniciarlo y 
    /// registrar entregas.
    /// </summary>
    private static void ValidarEsEncargado(Viaje viaje, int idUsuario, bool esJefe)
    {
        if (!esJefe && viaje.Encargados.All(e => e.IdUsuario != idUsuario))
        {
            throw new ReglaNegocioException(
                $"Solo los encargados del viaje {NumeroViaje.Formatear(viaje.IdViaje)} pueden realizar esta acción.",
                StatusCodes.Status403Forbidden);
        }
    }//Fin del metodo ValidarEsEncargado

    /// <summary>
    /// De 1 a 3 empleados, sin repetir, activos y con rol Empleado.
    /// </summary>
    private async Task<List<Usuario>> ValidarEncargadosAsync(List<int>? idsEncargados)
    {
        if (idsEncargados is null || idsEncargados.Count < MinimoEncargados || idsEncargados.Count > MaximoEncargados)
            throw new ReglaNegocioException($"El viaje debe tener entre {MinimoEncargados} y {MaximoEncargados} empleados encargados.");

        if (idsEncargados.Distinct().Count() != idsEncargados.Count)
            throw new ReglaNegocioException("Un empleado no puede aparecer dos veces como encargado del mismo viaje.");

        var usuarios = (await _viajes.ObtenerUsuariosAsync(idsEncargados)).ToDictionary(u => u.IdUsuario);

        foreach (int id in idsEncargados)
        {
            if (!usuarios.TryGetValue(id, out var usuario) || !usuario.Activo)
                throw new ReglaNegocioException($"El usuario {id} no existe o está inactivo.");

            if (usuario.Rol != Rol.Empleado)
                throw new ReglaNegocioException($"{usuario.Nombre} no tiene rol Empleado; solo los empleados pueden ser encargados de un viaje.");
        }

        return idsEncargados.Select(id => usuarios[id]).ToList();
    }//Fin del metodo ValidarEncargadosAsync

    /// <summary>
    /// Pedidos Armados, sin otro viaje, y cuya fecha de entrega no sea anterior 
    /// a la del viaje.
    /// idViajeActual: al editar, los pedidos que ya están en ESTE viaje son validos.
    /// </summary>
    private async Task<List<Pedido>> ValidarPedidosAsync(List<int>? idsPedidos, DateOnly fechaViaje, int? idViajeActual)
    {
        if (idsPedidos is null || idsPedidos.Count == 0)
            throw new ReglaNegocioException("El viaje debe llevar al menos un pedido.");

        if (idsPedidos.Distinct().Count() != idsPedidos.Count)
            throw new ReglaNegocioException("Un pedido no puede aparecer dos veces en el mismo viaje.");

        var pedidos = (await _viajes.ObtenerPedidosAsync(idsPedidos)).ToDictionary(p => p.IdPedido);

        foreach (int id in idsPedidos)
        {
            string numero = NumeroPedido.Formatear(id);

            if (!pedidos.TryGetValue(id, out var pedido))
                throw new ReglaNegocioException($"El pedido {numero} no existe.");

            if (pedido.Estado != EstadoPedido.Armado)
                throw new ReglaNegocioException($"El pedido {numero} está {pedido.Estado}; solo se pueden asignar pedidos Armados.");

            if (pedido.IdViaje.HasValue && pedido.IdViaje != idViajeActual)
                throw new ReglaNegocioException($"El pedido {numero} ya está asignado al viaje {NumeroViaje.Formatear(pedido.IdViaje.Value)}.");

            if (pedido.FechaEntrega < fechaViaje)
            {
                throw new ReglaNegocioException(
                    $"El pedido {numero} se entrega el {pedido.FechaEntrega:dd/MM/yyyy}, antes de la fecha del viaje ({fechaViaje:dd/MM/yyyy}).");
            }
        }

        return idsPedidos.Select(id => pedidos[id]).ToList();
    }//Fin del metodo ValidarPedidosAsync

    //Conversion

    private async Task<Viaje> ObtenerOFallarAsync(int idViaje)
    {
        return await _viajes.ObtenerPorIdAsync(idViaje)
            ?? throw new ReglaNegocioException("El viaje no existe.", StatusCodes.Status404NotFound);
    }//Fin del metodo ObtenerOFallarAsync

    private static ViajeResumenDto ConvertirAResumen(Viaje v)
    {
        return new ViajeResumenDto(
            v.IdViaje,
            NumeroViaje.Formatear(v.IdViaje),
            v.Fecha,
            v.Estado,
            v.Encargados.Select(e => e.Usuario.Nombre).OrderBy(n => n).ToList(),
            v.Pedidos.Count,
            v.Pedidos.Count(p => p.Estado == EstadoPedido.Entregado),
            v.Pedidos.SelectMany(p => p.Lineas).Sum(l => l.Cantidad));
    }//Fin del metodo ConvertirResumen

    private static BitacoraViajeDto ConvertirABitacora(Viaje v)
    {
        // La duracion solo existe cuando el viaje ya inicio y termino.
        int? duracion = v.FechaHoraInicio.HasValue && v.FechaHoraFin.HasValue
            ? (int)Math.Round((v.FechaHoraFin.Value - v.FechaHoraInicio.Value).TotalMinutes)
            : null;

        var pedidos = v.Pedidos
            .OrderBy(p => p.IdPedido)
            .Select(p => new BitacoraPedidoDto(
                p.IdPedido,
                NumeroPedido.Formatear(p.IdPedido),
                p.Cliente.Nombre,
                p.Cliente.Provincia.NombreParaMostrar(),
                p.Estado,
                p.Lineas.Sum(l => l.Cantidad),
                p.FechaHoraEntregado.HasValue ? HoraCostaRica.ALocal(p.FechaHoraEntregado.Value) : null,
                p.UsuarioEntrega?.Nombre))
            .ToList();

        return new BitacoraViajeDto(
            v.IdViaje,
            NumeroViaje.Formatear(v.IdViaje),
            v.Fecha,
            v.Estado,
            v.Encargados.Select(e => e.Usuario.Nombre).OrderBy(n => n).ToList(),
            v.UsuarioCreador.Nombre,
            v.FechaHoraInicio.HasValue ? HoraCostaRica.ALocal(v.FechaHoraInicio.Value) : null,
            v.FechaHoraFin.HasValue ? HoraCostaRica.ALocal(v.FechaHoraFin.Value) : null,
            duracion,
            pedidos.Count,
            pedidos.Count(p => p.Estado == EstadoPedido.Entregado),
            pedidos.Sum(p => p.TotalKg),
            pedidos);
    }//Fin del metodo ConvertirBitacora

    private static ViajeDto ConvertirADto(Viaje v)
    {
        var pedidos = v.Pedidos
            .OrderBy(p => p.IdPedido)
            .Select(ConvertirPedido)
            .ToList();

        return new ViajeDto(
            v.IdViaje,
            NumeroViaje.Formatear(v.IdViaje),
            v.Fecha,
            v.Estado,
            v.Encargados.Select(e => new EncargadoDto(e.IdUsuario, e.Usuario.Nombre)).OrderBy(e => e.Nombre).ToList(),
            v.FechaHoraInicio.HasValue ? HoraCostaRica.ALocal(v.FechaHoraInicio.Value) : null,
            v.FechaHoraFin.HasValue ? HoraCostaRica.ALocal(v.FechaHoraFin.Value) : null,
            v.UsuarioCreador.Nombre,
            pedidos.Count,
            pedidos.Count(p => p.Estado == EstadoPedido.Entregado),
            pedidos.Sum(p => p.TotalKg),
            pedidos);
    }//Fin del metodo ConvertirDto

    private static PedidoEnViajeDto ConvertirPedido(Pedido p)
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

        return new PedidoEnViajeDto(
            p.IdPedido,
            NumeroPedido.Formatear(p.IdPedido),
            p.Estado,
            p.Cliente.Nombre,
            p.Cliente.Provincia.NombreParaMostrar(),
            p.Cliente.UbicacionDetalle,
            p.Cliente.Telefono,
            p.Cliente.NombreContacto,
            p.FechaEntrega,
            lineas.Sum(l => l.Cantidad),
            p.FechaHoraEntregado.HasValue ? HoraCostaRica.ALocal(p.FechaHoraEntregado.Value) : null,
            p.UsuarioEntrega?.Nombre,
            lineas);
    }//Fin del metodo ConvertirPedido
}//Fin de la clase ViajeService