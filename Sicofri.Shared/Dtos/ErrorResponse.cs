using System;
using System.Collections.Generic;
using System.Text;

namespace Sicofri.Shared.Dtos;
/// <summary>Forma estándar de todos los errores que devuelve la API.</summary>
public record ErrorResponse(string Mensaje);