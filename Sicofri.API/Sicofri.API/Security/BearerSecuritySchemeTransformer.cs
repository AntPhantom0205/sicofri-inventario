using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Sicofri.API.Security;

/// <summary>
/// Agrega a la documentación de Swagger el esquema de seguridad "Bearer" (JWT),
/// para poder probar los servicios protegidos desde el navegador.
/// </summary>

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _proveedorEsquemas;

    public BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider proveedorEsquemas)
    {
        _proveedorEsquemas = proveedorEsquemas;
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var esquemas = await _proveedorEsquemas.GetAllSchemesAsync();
        if (!esquemas.Any(e => e.Name == "Bearer"))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Description = "Pegue aquí el token obtenido en /api/auth/login"
            }
        };

        foreach (var operacion in document.Paths.Values.SelectMany(ruta => ruta.Operations!))
        {
            operacion.Value.Security ??= [];
            operacion.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }
    }//Fin del metodo TransformAsync
}//Fin de la clase
