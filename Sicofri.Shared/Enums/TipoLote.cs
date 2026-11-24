namespace Sicofri.Shared.Enums;

/// <summary>
/// Origen del lote. Define cómo se obtiene la fecha de caducidad:
/// - Empacado: viene impresa en la etiqueta del proveedor (se digita).
/// - Fresco: se calcula como fecha de producción + 3 días.
/// - CongeladoPropio: se calcula como fecha de producción + los meses de vida útil 
/// indicados.
/// </summary>
public enum TipoLote
{
    Empacado = 1,
    Fresco = 2,
    CongeladoPropio = 3

}//Fin de la clase
