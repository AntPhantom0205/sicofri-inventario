namespace Sicofri.Shared.Utilidades;

/// <summary>
/// Convierte entre el id de un pedido y su numero visible (1 ↔ "PE-000001").
/// Esta en Shared porque la app tambien lo usa para su funcionamiento.
/// </summary>
public static class NumeroPedido
{
    private const string Prefijo = "PE-";

    public static string Formatear(int idPedido)
    {
        return $"{Prefijo}{idPedido:D6}";
    }

    /// <summary>Acepta "PE-000012", "pe-12" o simplemente "12".</summary>
    public static bool TryObtenerId(string? texto, out int idPedido)
    {
        idPedido = 0;
        if (string.IsNullOrWhiteSpace(texto))
            return false;

        string limpio = texto.Trim();
        if (limpio.StartsWith(Prefijo, StringComparison.OrdinalIgnoreCase))
            limpio = limpio[Prefijo.Length..];

        return int.TryParse(limpio, out idPedido) && idPedido > 0;
    }
}//Fin de la clase NumeroPedido