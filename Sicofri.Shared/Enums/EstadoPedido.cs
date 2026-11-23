using System;
using System.Collections.Generic;
using System.Text;

namespace Sicofri.Shared.Enums;

/// <summary>
/// Estados de un pedido. Solo avanza en este orden: Armado → EnViaje → Entregado.
/// </summary>

public enum EstadoPedido
{
     Armado = 1,
    EnViaje = 2,
    Entregado = 3

}//Fin de la clase