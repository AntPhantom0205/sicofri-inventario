using Sicofri.API.Entities;
using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Notificaciones;

namespace Sicofri.API.Services;

/// <summary>
/// Notificaciones de caducidad: la revision (automática o manual), la consulta y 
/// la atencion.
/// </summary>
public class NotificacionService : INotificacionService
{
    private const int TamanoPaginaPorDefecto = 20;
    private const int TamanoPaginaMaximo = 100;
    private const int MaximoNombresEnAviso = 3;

    /// <summary>
    /// Evita que dos revisiones corran al mismo tiempo (por ejemplo, la automatica 
    /// de las 6:00
    /// y una manual del Jefe en el mismo segundo), lo que podria intentar 
    /// duplicar notificaciones.
    /// </summary>
    private static readonly SemaphoreSlim Candado = new(1, 1);

    private readonly INotificacionRepository _notificaciones;
    private readonly IProductoRepository _productos;
    private readonly IConfiguracionRepository _configuracion;
    private readonly INotificadorPush _notificador;
    private readonly ILogger<NotificacionService> _logger;

    public NotificacionService(
        INotificacionRepository notificaciones,
        IProductoRepository productos,
        IConfiguracionRepository configuracion,
        INotificadorPush notificador,
        ILogger<NotificacionService> logger)
    {
        _notificaciones = notificaciones;
        _productos = productos;
        _configuracion = configuracion;
        _notificador = notificador;
        _logger = logger;
    }
//Revision de caducidad: se ejecuta automaticamente cada 6 horas, y también puede ejecutarla el Jefe.

    public async Task<ResultadoRevisionDto> RevisarAsync()
    {
        await Candado.WaitAsync();
        try
        {
            DateOnly hoy = ReglasCaducidad.HoyEnCostaRica();
            int diasConfigurados = (await _configuracion.ObtenerAsync()).DiasAnticipacion;

            // 1. Las pendientes cuyo lote ya se agotó se cierran solas (sin usuario).
            var agotadas = await _notificaciones.ObtenerPendientesDeLotesAgotadosAsync();
            DateTime ahora = DateTime.UtcNow;
            foreach (var notificacion in agotadas)
            {
                notificacion.Atendida = true;
                notificacion.FechaHoraAtendida = ahora;
            }

            // 2. Lotes en riesgo que todavía no tienen notificación.
            int ventanaMaxima = Math.Max(diasConfigurados, ReglasCaducidad.DiasAvisoFresco);
            var candidatos = await _productos.ObtenerConCaducidadHastaAsync(hoy.AddDays(ventanaMaxima));
            var yaNotificados = await _notificaciones.ObtenerIdsProductosNotificadosAsync();

            var lotesNuevos = candidatos
                .Where(p => ReglasCaducidad.EstaProximoACaducar(p, hoy, diasConfigurados))
                .Where(p => !yaNotificados.Contains(p.IdProducto))
                .ToList();

            foreach (var lote in lotesNuevos)
            {
                await _notificaciones.AgregarAsync(new Notificacion
                {
                    IdProducto = lote.IdProducto,
                    FechaGenerada = hoy
                });
            }

            await _notificaciones.GuardarCambiosAsync();

            // 3. Aviso push al Jefe, solo si hay notificaciones nuevas.
            int avisosEnviados = lotesNuevos.Count > 0
                ? await AvisarAlJefeAsync(lotesNuevos, hoy)
                : 0;

            return new ResultadoRevisionDto(lotesNuevos.Count, agotadas.Count, avisosEnviados, _notificador.EstaDisponible);
        }
        finally
        {
            Candado.Release();
        }
    }//Fin del metodo RevisarAsync

    /// <summary>
    /// Envía un solo aviso, aunque haya varios lotes:
    /// - 1 lote:  "Producto proximo a caducar" · "Filete de tilapia · Congelador 2 · 
    /// vence 30/11/2026"
    /// - Varios:  "3 productos proximos a caducar" · "Filete de tilapia, Pargo entero, Camarón jumbo"
    /// </summary>
    private async Task<int> AvisarAlJefeAsync(List<Producto> lotes, DateOnly hoy)
    {
        var tokens = await _notificaciones.ObtenerTokensDelJefeAsync();
        if (tokens.Count == 0 || !_notificador.EstaDisponible)
            return 0;

        string titulo;
        string cuerpo;
        var datos = new Dictionary<string, string> { ["tipo"] = "caducidad" };

        if (lotes.Count == 1)
        {
            var lote = lotes[0];
            string vence = lote.FechaCaducidad < hoy ? "venció el" : "vence";
            titulo = "Producto próximo a caducar";
            cuerpo = $"{lote.Nombre} · {lote.Ubicacion.Nombre} · {vence} {lote.FechaCaducidad:dd/MM/yyyy}";
            datos["idProducto"] = lote.IdProducto.ToString();
        }
        else
        {
            var nombres = lotes.Select(l => l.Nombre).Distinct().ToList();
            titulo = $"{lotes.Count} productos próximos a caducar";
            cuerpo = string.Join(", ", nombres.Take(MaximoNombresEnAviso));
            if (nombres.Count > MaximoNombresEnAviso)
                cuerpo += $" y {nombres.Count - MaximoNombresEnAviso} más";
        }

        var tokensInvalidos = await _notificador.EnviarAsync(tokens, titulo, cuerpo, datos);

        if (tokensInvalidos.Count > 0)
        {
            await _notificaciones.QuitarTokensAsync(tokensInvalidos);
            _logger.LogInformation("Se borraron {Cantidad} token(s) inválido(s) de Firebase.", tokensInvalidos.Count);
        }

        return tokens.Count - tokensInvalidos.Count;
    }//Fin del metodo AvisarAlJefeAsync

    //Consulta y atencion de notificaciones

    public async Task<ResultadoPaginado<NotificacionDto>> BuscarAsync(bool? atendida, int pagina, int tamanoPagina)
    {
        pagina = Math.Max(pagina, 1);
        tamanoPagina = tamanoPagina is < 1 or > TamanoPaginaMaximo ? TamanoPaginaPorDefecto : tamanoPagina;

        DateOnly hoy = ReglasCaducidad.HoyEnCostaRica();
        var (notificaciones, total) = await _notificaciones.BuscarAsync(atendida, (pagina - 1) * tamanoPagina, tamanoPagina);

        var elementos = notificaciones.Select(n => ConvertirADto(n, hoy)).ToList();
        return new ResultadoPaginado<NotificacionDto>(elementos, total, pagina, tamanoPagina);
    }//Fin del metodo BuscarAsync

    public async Task<int> ContarPendientesAsync()
    {
        return await _notificaciones.ContarPendientesAsync();
    }

    public async Task<NotificacionDto> AtenderAsync(int idNotificacion, int idUsuario)
    {
        var notificacion = await _notificaciones.ObtenerPorIdAsync(idNotificacion)
            ?? throw new ReglaNegocioException("La notificación no existe.", StatusCodes.Status404NotFound);

        if (notificacion.Atendida)
            throw new ReglaNegocioException("Esta notificación ya fue atendida.", StatusCodes.Status409Conflict);

        notificacion.Atendida = true;
        notificacion.FechaHoraAtendida = DateTime.UtcNow;
        notificacion.IdUsuarioAtiende = idUsuario;

        await _notificaciones.GuardarCambiosAsync();

        // Se vuelve a consultar para traer el nombre de quien la atendió.
        var actualizada = await _notificaciones.ObtenerPorIdAsync(idNotificacion);
        return ConvertirADto(actualizada!, ReglasCaducidad.HoyEnCostaRica());
    }//Fin de la función AtenderAsync

    private static NotificacionDto ConvertirADto(Notificacion n, DateOnly hoy)
    {
        string? atendidaPor = n.UsuarioAtiende?.Nombre
            ?? (n.Atendida ? "Sistema (lote agotado)" : null);

        return new NotificacionDto(
            n.IdNotificacion,
            n.FechaGenerada,
            n.Atendida,
            n.FechaHoraAtendida.HasValue ? HoraCostaRica.ALocal(n.FechaHoraAtendida.Value) : null,
            atendidaPor,
            n.IdProducto,
            n.Producto.Nombre,
            n.Producto.TipoLote,
            n.Producto.Ubicacion.Nombre,
            n.Producto.Cantidad,
            n.Producto.FechaCaducidad,
            n.Producto.FechaCaducidad.DayNumber - hoy.DayNumber);
    }//Fin del metodo ConvertirADto
}//Fin de la clase NotificacionService