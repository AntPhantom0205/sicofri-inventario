using Sicofri.Shared.Enums;
namespace Sicofri.API.Entities;

/// <summary>
/// Hotel o restaurante al que Calimar le entrega pedidos.
/// </summary>

public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Provincia Provincia { get; set; }
    public string UbicacionDetalle { get; set; } = string.Empty;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

}//Fin de la clase
