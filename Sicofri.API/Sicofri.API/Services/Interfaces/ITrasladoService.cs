using Sicofri.Shared.Dtos;
using Sicofri.Shared.Dtos.Traslados;

namespace Sicofri.API.Services.Interfaces;

public interface ITrasladoService
{
    Task<MovimientoDto> TrasladarAsync(CrearTrasladoRequest datos, int idUsuario);
    Task<ResultadoPaginado<MovimientoDto>> ObtenerBitacoraAsync(
        string? producto, DateOnly? desde, DateOnly? hasta, int pagina, int tamanoPagina);
}//Fin de la interfaz ITrasladoService