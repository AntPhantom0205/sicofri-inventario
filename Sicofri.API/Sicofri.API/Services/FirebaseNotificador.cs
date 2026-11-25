using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Sicofri.API.Services.Interfaces;

namespace Sicofri.API.Services;

/// <summary>
/// Envia avisos push por Firebase Cloud Messaging.
/// Si la llave de Firebase no esta configurada, la API sigue funcionando normalmente:
/// las notificaciones se guardan en la base, pero no se envia el aviso al celular.
/// </summary>
public class FirebaseNotificador : INotificadorPush
{
    private readonly ILogger<FirebaseNotificador> _logger;
    private readonly FirebaseMessaging? _mensajeria;

    public FirebaseNotificador(IConfiguration configuracion, ILogger<FirebaseNotificador> logger)
    {
        _logger = logger;

        string? ruta = configuracion["Firebase:RutaCredenciales"];

        if (string.IsNullOrWhiteSpace(ruta) || !File.Exists(ruta))
        {
            _logger.LogWarning(
                "Firebase no está configurado: no se encontró la llave en '{Ruta}'. " +
                "Las notificaciones se guardarán, pero no se enviarán avisos push.", ruta);
            return;
        }

        try
        {
            var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(ruta)
            });

            _mensajeria = FirebaseMessaging.GetMessaging(app);
            _logger.LogInformation("Firebase inicializado correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo inicializar Firebase con la llave '{Ruta}'. Revise que el archivo sea válido.", ruta);
        }
    }

    public bool EstaDisponible => _mensajeria is not null;

    public async Task<List<string>> EnviarAsync(
        IReadOnlyList<string> tokens,
        string titulo,
        string cuerpo,
        IReadOnlyDictionary<string, string>? datos = null)
    {
        var tokensInvalidos = new List<string>();

        if (_mensajeria is null || tokens.Count == 0)
            return tokensInvalidos;

        var mensaje = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification { Title = titulo, Body = cuerpo },
            Data = datos,
            Android = new AndroidConfig { Priority = Priority.High }
        };

        try
        {
            BatchResponse respuesta = await _mensajeria.SendEachForMulticastAsync(mensaje);

            for (int i = 0; i < respuesta.Responses.Count; i++)
            {
                var resultado = respuesta.Responses[i];
                if (resultado.IsSuccess)
                    continue;

                _logger.LogWarning("No se pudo enviar el aviso a un dispositivo: {Motivo}", resultado.Exception?.Message);

                // Token que ya no sirve (app desinstalada, token vencido o inválido).
                if (resultado.Exception?.MessagingErrorCode is MessagingErrorCode.Unregistered
                                                            or MessagingErrorCode.InvalidArgument)
                {
                    tokensInvalidos.Add(tokens[i]);
                }
            }
        }
        catch (Exception ex)
        {
            // Un fallo de Firebase (sin internet, etc.) nunca debe tumbar la revisión de caducidades.
            _logger.LogError(ex, "Error al comunicarse con Firebase.");
        }

        return tokensInvalidos;
    }//Fin del metodo FirebaseNotificador
}//Fin de la clase FirebaseNotificador