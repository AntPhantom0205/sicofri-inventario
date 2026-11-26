using Sicofri.Shared.Enums;

namespace Sicofri.API.Entities;

/// <summary>
/// Hotel o restaurante al que Calimar le entrega pedidos.
/// TelEfono y contacto son opcionales: se pueden completar despues 
/// de crear el cliente
/// </summary>
public class Cliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Provincia Provincia { get; set; }
    public string UbicacionDetalle { get; set; } = string.Empty;

    /// <summary>Telefono del cliente (opcional).</summary>
    public string? Telefono { get; set; }

    /// <summary>Persona de contacto, ej. "Luis Mora, encargado de cocina" (opcional).</summary>
    public string? NombreContacto { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}//Fin de la clase Cliente