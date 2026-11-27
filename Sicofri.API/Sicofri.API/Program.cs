using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sicofri.API.Data;
using Sicofri.API.Data.Seed;
using Sicofri.API.Middleware;
using Sicofri.API.Repositories;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Security;
using Sicofri.API.Services;
using Sicofri.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Controladores y documentacion

builder.Services.AddControllers()
    // Los enums viajan como texto ("Jefe") en lugar de numero (1).
    .AddJsonOptions(opciones =>
        opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    // Errores de formato del JSON: respuesta en español indicando cada campo.
    .ConfigureApiBehaviorOptions(opciones =>
        opciones.InvalidModelStateResponseFactory = RespuestaValidacion.Crear);

// La documentación de Swagger tambien debe mostrar los enums como texto.
builder.Services.ConfigureHttpJsonOptions(opciones =>
    opciones.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi(opciones =>
    opciones.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

// 2. Base de datos

var cadenaConexion = builder.Configuration.GetConnectionString("SicofriDb");
if (string.IsNullOrWhiteSpace(cadenaConexion))
{
    throw new InvalidOperationException(
        "Falta la cadena de conexion 'SicofriDb'. Agreguela en los secretos de usuario.");
}

builder.Services.AddDbContext<SicofriDbContext>(opciones =>
    opciones.UseNpgsql(cadenaConexion));

// 3. Autenticacion con JWT

var jwt = builder.Configuration.GetSection(JwtSettings.Seccion).Get<JwtSettings>() ?? new JwtSettings();
if (Encoding.UTF8.GetByteCount(jwt.Clave) < 32)
{
    throw new InvalidOperationException(
        "La clave 'Jwt:Clave' falta o es muy corta (mínimo 32 caracteres). Revise los secretos de usuario.");
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.Seccion));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emisor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // Respuestas claras en español para los errores 401 y 403.
        opciones.Events = EventosJwt.Crear();
    });

builder.Services.AddAuthorization();

// 4. Inyeccion de dependencias (nuestras clases)

// Herramientas sin estado: una sola instancia para toda la aplicacion.
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtService>();

// Repositorios y servicios: una instancia nueva por cada peticion.
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

//Repositorios y servicios: una instancia nueva por cada peticion.
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

//Repositorios y servicios: una instancia nueva por cada peticion.
builder.Services.AddScoped<ITrasladoRepository, TrasladoRepository>();
builder.Services.AddScoped<ITrasladoService, TrasladoService>();

// Envío de avisos push: una sola instancia, porque Firebase se inicializa una vez.
builder.Services.AddSingleton<INotificadorPush, FirebaseNotificador>();

//Repositorios y Servicios: una instancia nueva por cada peticion.
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();

//Repositorios y Servicios: una instancia nueva por cada peticion.
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();

// El despertador: revisa caducidades al arrancar y todos los dias a las 6:00 a. m.
builder.Services.AddHostedService<RevisorCaducidadWorker>();

// Repositorios y Servicios: una instancia nueva por cada peticion.
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();

//Repositorios y Servicios: una instancia nueva por cada peticion
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

//Repositorios y Servicios: una instancia nueva por cada peticion
builder.Services.AddScoped<IViajeRepository, ViajeRepository>();
builder.Services.AddScoped<IViajeService, ViajeService>();

// 5. Construir y configurar la aplicacion

var app = builder.Build();

await DatosIniciales.CrearJefeInicialAsync(app.Services, app.Configuration);

// Inicializa Firebase al arrancar, para detectar de inmediato si la llave esta mal
// configurada.
app.Services.GetRequiredService<INotificadorPush>();

// Debe ir primero, para atrapar errores de todo lo que viene despues.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opciones =>
        opciones.SwaggerEndpoint("/openapi/v1.json", "Sicofri API v1"));
}

app.UseHttpsRedirection();

// El orden importa: primero se define quien eres (autenticacion), y despues ver
// lo que se puede hacer. (autorizacion).
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();