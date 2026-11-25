namespace Sicofri.Shared.Dtos;

/// <summary>
/// Una "pagina" de resultados. Sirve para listas largas como la bitacora,
/// para no enviar miles de registros de una sola vez.
/// </summary>
public record class ResultadoPaginado<T>(List<T> Elementos, int Total, int Pagina, int TamanoPagina)
{
    public int TotalPaginas => TamanoPagina == 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanoPagina);

}//Fin de la clase ResultadoPaginado