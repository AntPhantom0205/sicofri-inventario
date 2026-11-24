using Sicofri.API.Entities;
using Sicofri.API.Exceptions;
using Sicofri.API.Repositories.Interfaces;
using Sicofri.API.Services.Interfaces;
using Sicofri.Shared.Dtos.Inventario;
using Sicofri.Shared.Dtos.Productos;
using Sicofri.Shared.Enums;

namespace Sicofri.API.Services;

/// <summary>
/// Reglas de negocio de los lotes de producto y del inventario por ubicacion.
/// </summary>

public class ProductoService : IProductoService
{
    //Cantidad maxima de dias antes de la caducidad para que un lote se considere proximo a caducar.
    private const int LargoMaximoTexto = 60;
    private const int LargoMaximoCodigoLote = 40;
    private const int MesesVidaUtilMaximo = 24;
    private const decimal CantidadMaxima = 99_999_999.99m;

    private readonly IProductoRepository _productos;
    private readonly IConfiguracionRepository _configuracion;

    public ProductoService(IProductoRepository productos, IConfiguracionRepository configuracion)
    {
        _productos = productos;
        _configuracion = configuracion;
    }

    // Consultas de productos y ubicaciones

    public async Task<List<ProductoDto>> BuscarAsync(string? nombre, int? idUbicacion, bool incluirAgotados)
    {
        var (hoy, dias) = await ObtenerContextoCaducidadAsync();
        var lotes = await _productos.BuscarAsync(nombre, idUbicacion, incluirAgotados);
        return lotes.Select(p => ConvertirADto(p, hoy, dias)).ToList();
    }//Fin del metodo BuscarAsync

    public async Task<ProductoDto> ObtenerPorIdAsync(int idProducto)
    {
        var producto = await ObtenerOFallarAsync(idProducto);
        var (hoy, dias) = await ObtenerContextoCaducidadAsync();
        return ConvertirADto(producto, hoy, dias);
    }//Fin del metodo ObtenerPorIdAsync

    public async Task<List<ProductoDto>> ObtenerProximosACaducarAsync()
    {
        var (hoy, dias) = await ObtenerContextoCaducidadAsync();

        // Se piden a la base los que vencen dentro de la ventana mas amplia,
        // y luego se aplica la regla exacta de cada tipo (fresco = 1 dia, los demas = configurable).
        int ventanaMaxima = Math.Max(dias, ReglasCaducidad.DiasAvisoFresco);
        var candidatos = await _productos.ObtenerConCaducidadHastaAsync(hoy.AddDays(ventanaMaxima));

        return candidatos
            .Where(p => ReglasCaducidad.EstaProximoACaducar(p, hoy, dias))
            .Select(p => ConvertirADto(p, hoy, dias))
            .ToList();
    }//Fin del metodo ObtenerProximosACaducarAsync

    public async Task<List<ResumenUbicacionDto>> ObtenerResumenInventarioAsync()
    {
        var (hoy, dias) = await ObtenerContextoCaducidadAsync();
        var ubicaciones = await _productos.ObtenerUbicacionesConExistenciasAsync();

        return ubicaciones.Select(u => new ResumenUbicacionDto(
            u.IdUbicacion,
            u.Nombre,
            u.Zona,
            u.Productos.Count,
            u.Productos.Sum(p => p.Cantidad),
            u.Productos.Count(p => ReglasCaducidad.EstaProximoACaducar(p, hoy, dias))
        )).ToList();
    }//Fin del metodo ObtenerResumenInventarioAsync

    // Registro y edicion (Metodos CRUD de productos) 

    public async Task<ProductoDto> CrearAsync(CrearProductoRequest datos)
    {
        var (hoy, dias) = await ObtenerContextoCaducidadAsync();

        var ubicacion = await _productos.ObtenerUbicacionAsync(datos.IdUbicacion)
            ?? throw new ReglaNegocioException("La ubicación seleccionada no existe.");

        if (datos.Cantidad <= 0)
            throw new ReglaNegocioException("La cantidad debe ser mayor a 0 kg.");

        var producto = new Producto
        {
            IdUbicacion = ubicacion.IdUbicacion,
            Ubicacion = ubicacion
        };

        ValidarYAplicar(producto, datos.Nombre, datos.NombreProveedor, datos.TipoLote, datos.CodigoLote,
            datos.Cantidad, datos.FechaIngreso, datos.FechaProduccion, datos.FechaCaducidad, datos.MesesVidaUtil, hoy);

        await _productos.AgregarAsync(producto);
        await _productos.GuardarCambiosAsync();

        return ConvertirADto(producto, hoy, dias);
    }// Fin del metodo CrearAsync

    public async Task<ProductoDto> ActualizarAsync(int idProducto, ActualizarProductoRequest datos)
    {
        var (hoy, dias) = await ObtenerContextoCaducidadAsync();
        var producto = await ObtenerOFallarAsync(idProducto);

        ValidarYAplicar(producto, datos.Nombre, datos.NombreProveedor, datos.TipoLote, datos.CodigoLote,
            datos.Cantidad, datos.FechaIngreso, datos.FechaProduccion, datos.FechaCaducidad, datos.MesesVidaUtil, hoy);

        await _productos.GuardarCambiosAsync();

        return ConvertirADto(producto, hoy, dias);
    }//Fin del metodo ActualizarAsync

    public async Task EliminarAsync(int idProducto)
    {
        var producto = await ObtenerOFallarAsync(idProducto);

        if (await _productos.TieneMovimientosOPedidosAsync(idProducto))
        {
            throw new ReglaNegocioException(
                "No se puede eliminar un lote con traslados o pedidos registrados. Si ya no hay producto, ajuste su cantidad a 0.",
                StatusCodes.Status409Conflict);
        }

        _productos.Eliminar(producto);
        await _productos.GuardarCambiosAsync();
    }//Fin del metodo EliminarAsync

    // Reglas internas de la app

    /// <summary>
    /// Valida todos los datos del lote y, si son correctos, los copia a la entidad.
    /// Lo usan tanto el registro como la edicion, para que las reglas sean identicas.
    /// </summary>
    private static void ValidarYAplicar(Producto producto, string? nombre, string? proveedor, TipoLote tipo,
        string? codigoLote, decimal cantidad, DateOnly fechaIngreso, DateOnly fechaProduccion,
        DateOnly? fechaCaducidad, int? mesesVidaUtil, DateOnly hoy)
    {
        string nombreLimpio = ValidarTexto(nombre, "El nombre del producto");
        string proveedorLimpio = ValidarTexto(proveedor, "El nombre del proveedor");

        if (!Enum.IsDefined(tipo))
            throw new ReglaNegocioException("El tipo de lote no es válido. Debe ser Empacado, Fresco o CongeladoPropio.");

        string? codigo = string.IsNullOrWhiteSpace(codigoLote) ? null : codigoLote.Trim();

        if (tipo == TipoLote.Empacado && codigo is null)
            throw new ReglaNegocioException("Para productos empacados debe digitar el código de lote que trae la etiqueta.");

        if (codigo is not null && codigo.Length > LargoMaximoCodigoLote)
            throw new ReglaNegocioException($"El código de lote debe tener máximo {LargoMaximoCodigoLote} caracteres.");

        if (cantidad < 0 || cantidad > CantidadMaxima)
            throw new ReglaNegocioException("La cantidad en kilogramos no es válida.");

        if (decimal.Round(cantidad, 2) != cantidad)
            throw new ReglaNegocioException("La cantidad admite máximo 2 decimales.");

        if (fechaIngreso > hoy)
            throw new ReglaNegocioException("La fecha de ingreso no puede ser una fecha futura.");

        if (fechaProduccion > fechaIngreso)
            throw new ReglaNegocioException("La fecha de producción no puede ser posterior a la fecha de ingreso.");

        DateOnly caducidad = CalcularFechaCaducidad(tipo, fechaProduccion, fechaCaducidad, mesesVidaUtil);

        if (caducidad <= fechaProduccion)
            throw new ReglaNegocioException("La fecha de caducidad debe ser posterior a la fecha de producción.");

        if (caducidad < fechaIngreso)
            throw new ReglaNegocioException("El lote ya estaba vencido en su fecha de ingreso.");

        producto.Nombre = nombreLimpio;
        producto.NombreProveedor = proveedorLimpio;
        producto.TipoLote = tipo;
        producto.CodigoLote = codigo;
        producto.Cantidad = cantidad;
        producto.FechaIngreso = fechaIngreso;
        producto.FechaProduccion = fechaProduccion;
        producto.FechaCaducidad = caducidad;
    }//Fin del metodo ValidarYAplicar

    /// <summary>
    /// Las tres formas de obtener la caducidad, segun lo que explico la empresa Calimar.
    /// </summary>
    private static DateOnly CalcularFechaCaducidad(TipoLote tipo, DateOnly fechaProduccion,
        DateOnly? fechaCaducidadEscrita, int? mesesVidaUtil)
    {
        switch (tipo)
        {
            case TipoLote.Empacado:
                return fechaCaducidadEscrita
                    ?? throw new ReglaNegocioException("Para productos empacados debe digitar la fecha de caducidad de la etiqueta.");

            case TipoLote.Fresco:
                return fechaProduccion.AddDays(ReglasCaducidad.DiasVidaFresco);

            case TipoLote.CongeladoPropio:
                if (mesesVidaUtil is null || mesesVidaUtil < 1 || mesesVidaUtil > MesesVidaUtilMaximo)
                    throw new ReglaNegocioException($"Indique la vida útil en meses (entre 1 y {MesesVidaUtilMaximo}).");
                return fechaProduccion.AddMonths(mesesVidaUtil.Value);

            default:
                throw new ReglaNegocioException("El tipo de lote no es valido.");
        }
    }//Fin del metodo CalcularFechaCaducidad

    private static string ValidarTexto(string? valor, string campo)
    {
        string limpio = valor?.Trim() ?? string.Empty;

        if (limpio.Length == 0 || limpio.Length > LargoMaximoTexto)
            throw new ReglaNegocioException($"{campo} es obligatorio y debe tener maximo {LargoMaximoTexto} caracteres.");

        return limpio;
    }//Fin del metodo ValidarTexto

    private async Task<Producto> ObtenerOFallarAsync(int idProducto)
    {
        return await _productos.ObtenerPorIdAsync(idProducto)
            ?? throw new ReglaNegocioException("El lote no existe.", StatusCodes.Status404NotFound);
    }//Fin del metodo ObtenerOFallarAsync

    private async Task<(DateOnly Hoy, int Dias)> ObtenerContextoCaducidadAsync()
    {
        var configuracion = await _configuracion.ObtenerAsync();
        return (ReglasCaducidad.HoyEnCostaRica(), configuracion.DiasAnticipacion);
    }//Fin del metodo ObtenerContextoCaducidadAsync

    private static ProductoDto ConvertirADto(Producto p, DateOnly hoy, int diasConfigurados)
    {
        return new ProductoDto(
            p.IdProducto,
            p.Nombre,
            p.NombreProveedor,
            p.TipoLote,
            p.CodigoLote,
            p.Cantidad,
            p.IdUbicacion,
            p.Ubicacion.Nombre,
            p.FechaIngreso,
            p.FechaProduccion,
            p.FechaCaducidad,
            p.FechaCaducidad.DayNumber - hoy.DayNumber,
            ReglasCaducidad.EstaProximoACaducar(p, hoy, diasConfigurados));
    }//Fin del metodo ConvertirADto

}//Fin de la clase ProductoService
