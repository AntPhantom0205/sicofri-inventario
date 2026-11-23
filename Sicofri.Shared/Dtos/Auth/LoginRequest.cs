using System;
using System.Collections.Generic;
using System.Text;

namespace Sicofri.Shared.Dtos.Auth;

/// <summary>Datos que envía la app para iniciar sesión.</summary>
public record LoginRequest(string NombreUsuario, string Contrasena);