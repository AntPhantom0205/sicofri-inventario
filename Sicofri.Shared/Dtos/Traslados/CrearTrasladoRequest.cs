namespace Sicofri.Shared.Dtos.Traslados;

/// <summary>
/// Datos del formulario "Realizar traslado".
/// La ubicacion de origen no se envia: es la ubicacion actual del lote.
/// El usuario responsable tampoco: se toma del token de la sesion.
/// </summary>
public record CrearTrasladoRequest(int IdProducto, int IdUbicacionDestino, decimal Cantidad);