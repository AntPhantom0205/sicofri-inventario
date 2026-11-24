using System;
using System.Collections.Generic;
using System.Text;

namespace Sicofri.Shared.Dtos;
/// <summary>
/// Forma estandar de todos los errores de la API.
/// - Mensaje: texto para mostrar al usuario.
/// - Errores: problemas por campo (ej. "fechaIngreso": ["El valor no es valido..."]).
/// - Detalle: información tecnica, solo para los desarrolladores.
/// </summary>
public record ErrorResponse(
    string Mensaje,
    Dictionary<string, string[]>? Errores = null,
    string? Detalle = null);