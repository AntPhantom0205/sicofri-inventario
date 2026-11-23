using Microsoft.EntityFrameworkCore;
using Sicofri.API.Data;

var builder = WebApplication.CreateBuilder(args);


//------ Servicios -------

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var cadenaConexion = builder.Configuration.GetConnectionString("SicofriDb");
if (string.IsNullOrWhiteSpace(cadenaConexion))
{
    throw new InvalidOperationException(
        "Falta la cadena de conexión 'SicofriDb'. Agréguela en los secretos de usuario.");
}

builder.Services.AddDbContext<SicofriDbContext>(opciones =>
    opciones.UseNpgsql(cadenaConexion));

// ---------- Aplicación ----------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opciones =>
        opciones.SwaggerEndpoint("/openapi/v1.json", "Sicofri API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();