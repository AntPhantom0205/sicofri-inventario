namespace Sicofri.Shared.Enums;

/// <summary>
/// Nombres para mostrar de las provincias, con espacios y tildes.
/// </summary>
public static class ProvinciaExtensiones
{
    public static string NombreParaMostrar(this Provincia provincia)
    {
        return provincia switch
        {
            Provincia.SanJose => "San José",
            Provincia.Alajuela => "Alajuela",
            Provincia.Cartago => "Cartago",
            Provincia.Heredia => "Heredia",
            Provincia.Guanacaste => "Guanacaste",
            Provincia.Puntarenas => "Puntarenas",
            Provincia.Limon => "Limón",
            _ => provincia.ToString()
        };
    }//Fin del metodo NombreParaMostrar
}//Fin de la clase ProvinciaExtensiones