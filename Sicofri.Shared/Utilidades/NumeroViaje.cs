namespace Sicofri.Shared.Utilidades;

/// <summary>
/// Numero visible de un viaje: 1 → "VJ-000001".
/// </summary>
public static class NumeroViaje
{
    public static string Formatear(int idViaje)
    {
        return $"VJ-{idViaje:D6}";
    }//Fin del metodo Formatear
}//Fin de la clase NumeroViaje