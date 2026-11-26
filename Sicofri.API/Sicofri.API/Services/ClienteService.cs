using System.Text.RegularExpressions;
using Sicofri.API.Entities;
using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Clientes;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Services;

/// <summary>Reglas de negocio de los clientes (hoteles y restaurantes).</summary>
public class ClienteService : IClienteService
{
    private const int LargoMaximoNombre = 60;
    private const int LargoMaximoUbicacion = 150;
    private const int LargoMaximoContacto = 60;

    /// <summary>
    /// Teléfono: entre 8 y 20 caracteres, solo digitos, espacios, guiones, parentesis
    /// y "+".
    /// Acepta "88887777", "8888-7777" y "+506 8888 7777".
    /// </summary>
    private static readonly Regex FormatoTelefono = new(@"^[0-9+\-\s()]{8,20}$");

    private readonly IClienteRepository _clientes;

    public ClienteService(IClienteRepository clientes)
    {
        _clientes = clientes;
    }

    //Consultas

    public async Task<List<ClienteDto>> BuscarAsync(string? nombre, Provincia? provincia)
    {
        var clientes = await _clientes.BuscarAsync(nombre, provincia);
        var entregas = await _clientes.ContarEntregasPorClienteAsync();

        return clientes.Select(c => ConvertirADto(c, entregas)).ToList();
    }//Fin del metodo BuscarAsync

    public async Task<ClienteDto> ObtenerPorIdAsync(int idCliente)
    {
        var cliente = await ObtenerOFallarAsync(idCliente);
        var entregas = await _clientes.ContarEntregasPorClienteAsync();

        return ConvertirADto(cliente, entregas);
    }//Fin del metodo ObtenerPorIdAsync

    /// <summary>
    /// Datos para el mapa: las 7 provincias, cada una con sus clientes.
    /// Las provincias sin clientes tambien aparecen (con 0), para que el mapa 
    /// siempre las dibuje.
    /// </summary>
    public async Task<List<ProvinciaClientesDto>> ObtenerPorProvinciaAsync()
    {
        var clientes = await _clientes.BuscarAsync(nombre: null, provincia: null);
        var entregas = await _clientes.ContarEntregasPorClienteAsync();

        return Enum.GetValues<Provincia>()
            .Select(provincia =>
            {
                var clientesDeProvincia = clientes
                    .Where(c => c.Provincia == provincia)
                    .Select(c => ConvertirADto(c, entregas))
                    .ToList();

                return new ProvinciaClientesDto(
                    provincia,
                    provincia.NombreParaMostrar(),
                    clientesDeProvincia.Count,
                    clientesDeProvincia.Sum(c => c.EntregasRecibidas),
                    clientesDeProvincia);
            })
            .ToList();
    }//Fin del metodo ObtenerPorProvinciaAsync

    //Registro y edicion de clientes

    public async Task<ClienteDto> CrearAsync(CrearClienteRequest datos)
    {
        var cliente = new Cliente();
        ValidarYAplicar(cliente, datos.Nombre, datos.Provincia, datos.UbicacionDetalle, datos.Telefono, datos.NombreContacto);

        if (await _clientes.ExisteDuplicadoAsync(cliente.Nombre, cliente.Provincia))
        {
            throw new ReglaNegocioException(
                $"Ya existe un cliente llamado '{cliente.Nombre}' en {cliente.Provincia.NombreParaMostrar()}.",
                StatusCodes.Status409Conflict);
        }

        await _clientes.AgregarAsync(cliente);
        await _clientes.GuardarCambiosAsync();

        // Un cliente recien creado todavia no tiene entregas.
        return ConvertirADto(cliente, new Dictionary<int, int>());
    }//Fin del metodo CrearAsync

    public async Task<ClienteDto> ActualizarAsync(int idCliente, ActualizarClienteRequest datos)
    {
        var cliente = await ObtenerOFallarAsync(idCliente);
        ValidarYAplicar(cliente, datos.Nombre, datos.Provincia, datos.UbicacionDetalle, datos.Telefono, datos.NombreContacto);

        if (await _clientes.ExisteDuplicadoAsync(cliente.Nombre, cliente.Provincia, excluirIdCliente: idCliente))
        {
            throw new ReglaNegocioException(
                $"Ya existe otro cliente llamado '{cliente.Nombre}' en {cliente.Provincia.NombreParaMostrar()}.",
                StatusCodes.Status409Conflict);
        }

        await _clientes.GuardarCambiosAsync();

        var entregas = await _clientes.ContarEntregasPorClienteAsync();
        return ConvertirADto(cliente, entregas);
    }//Fin del metodo ActualizarAsync

    public async Task EliminarAsync(int idCliente)
    {
        var cliente = await ObtenerOFallarAsync(idCliente);

        if (await _clientes.TienePedidosAsync(idCliente))
        {
            throw new ReglaNegocioException(
                "No se puede eliminar un cliente que tiene pedidos registrados.",
                StatusCodes.Status409Conflict);
        }

        _clientes.Eliminar(cliente);
        await _clientes.GuardarCambiosAsync();
    }//Fin del metodo EliminarAsync

    //Reglas internas

    private static void ValidarYAplicar(Cliente cliente, string? nombre, Provincia provincia,
        string? ubicacionDetalle, string? telefono, string? nombreContacto)
    {
        string nombreLimpio = nombre?.Trim() ?? string.Empty;
        if (nombreLimpio.Length == 0 || nombreLimpio.Length > LargoMaximoNombre)
            throw new ReglaNegocioException($"El nombre del cliente es obligatorio y debe tener máximo {LargoMaximoNombre} caracteres.");

        if (!Enum.IsDefined(provincia))
            throw new ReglaNegocioException("La provincia no es válida.");

        string ubicacionLimpia = ubicacionDetalle?.Trim() ?? string.Empty;
        if (ubicacionLimpia.Length == 0 || ubicacionLimpia.Length > LargoMaximoUbicacion)
            throw new ReglaNegocioException($"La ubicación es obligatoria y debe tener máximo {LargoMaximoUbicacion} caracteres.");

        // Opcionales: si vienen vacíos se guardan como null; si vienen, se validan.
        string? telefonoLimpio = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        if (telefonoLimpio is not null && !FormatoTelefono.IsMatch(telefonoLimpio))
            throw new ReglaNegocioException("El teléfono no es válido. Use solo números (ej. 8888-7777 o +506 8888 7777).");

        string? contactoLimpio = string.IsNullOrWhiteSpace(nombreContacto) ? null : nombreContacto.Trim();
        if (contactoLimpio is { Length: > LargoMaximoContacto })
            throw new ReglaNegocioException($"El nombre del contacto debe tener máximo {LargoMaximoContacto} caracteres.");

        cliente.Nombre = nombreLimpio;
        cliente.Provincia = provincia;
        cliente.UbicacionDetalle = ubicacionLimpia;
        cliente.Telefono = telefonoLimpio;
        cliente.NombreContacto = contactoLimpio;
    }//Fin del metodo ValidarYAplicar

    private async Task<Cliente> ObtenerOFallarAsync(int idCliente)
    {
        return await _clientes.ObtenerPorIdAsync(idCliente)
            ?? throw new ReglaNegocioException("El cliente no existe.", StatusCodes.Status404NotFound);
    }//Fin del metodo ObtenerOFallarAsync

    private static ClienteDto ConvertirADto(Cliente c, Dictionary<int, int> entregas)
    {
        return new ClienteDto(
            c.IdCliente,
            c.Nombre,
            c.Provincia,
            c.Provincia.NombreParaMostrar(),
            c.UbicacionDetalle,
            c.Telefono,
            c.NombreContacto,
            entregas.GetValueOrDefault(c.IdCliente, 0));
    }//Fin del metodo ConvertirADto
}//Fin de la clase ClienteService