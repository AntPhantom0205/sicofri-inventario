using Sicofri.API.Services.Interfaces;

namespace Sicofri.API.Services;

/// <summary>
/// Proceso en segundo plano que revisa las caducidades al arrancar la API
/// y luego todos los dias a las 6:00 a. m. (hora de Costa Rica).
/// </summary>
public class RevisorCaducidadWorker : BackgroundService
{
    private static readonly TimeOnly HoraRevision = new(6, 0);
    private static readonly TimeSpan EsperaAlArrancar = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _fabricaAlcances;
    private readonly ILogger<RevisorCaducidadWorker> _logger;

    public RevisorCaducidadWorker(IServiceScopeFactory fabricaAlcances, ILogger<RevisorCaducidadWorker> logger)
    {
        _fabricaAlcances = fabricaAlcances;
        _logger = logger;
    }//Fin del constructor

    protected override async Task ExecuteAsync(CancellationToken detener)
    {
        try
        {
            // Pequeña espera para que la API termine de arrancar antes de la primera
            // revision.
            await Task.Delay(EsperaAlArrancar, detener);
            await EjecutarRevisionAsync("al arrancar");

            while (!detener.IsCancellationRequested)
            {
                TimeSpan espera = HoraCostaRica.TiempoHasta(HoraRevision);
                _logger.LogInformation("Próxima revisión de caducidades en {Horas:0.0} horas (6:00 a. m.).", espera.TotalHours);

                await Task.Delay(espera, detener);
                await EjecutarRevisionAsync("programada");
            }
        }
        catch (OperationCanceledException)
        {
            // La API se esta apagando: se sale sin error.
        }
    }//Fin del metodo ExecuteAsync

    private async Task EjecutarRevisionAsync(string motivo)
    {
        try
        {
            // Los servicios que usan la base de datos son "Scoped": hay que crear
            // un alcance propio.
            using var alcance = _fabricaAlcances.CreateScope();
            var servicio = alcance.ServiceProvider.GetRequiredService<INotificacionService>();

            var resultado = await servicio.RevisarAsync();

            _logger.LogInformation(
                "Revisión de caducidades ({Motivo}): {Nuevas} nueva(s), {Cerradas} cerrada(s) por lote agotado, {Avisos} aviso(s) push enviado(s).",
                motivo, resultado.NotificacionesNuevas, resultado.CerradasPorLoteAgotado, resultado.AvisosPushEnviados);
        }
        catch (Exception ex)
        {
            // Un error en una revision no debe detener al despertador: se intentara
            // de nuevo mañana.
            _logger.LogError(ex, "Falló la revisión de caducidades ({Motivo}).", motivo);
        }
    }//Fin del metodo EjecutarRevisionAsync
}//Fin de la clase RevisorCaducidadWorker