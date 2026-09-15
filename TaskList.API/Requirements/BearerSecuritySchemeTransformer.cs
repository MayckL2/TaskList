using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TaskList.API
{
    internal sealed class BearerSecuritySchemeTransformer(
        IAuthenticationSchemeProvider authenticationSchemeProvider
    ) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken
        )
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

            // Só adiciona o esquema se o Bearer estiver configurado
            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                var bearerScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT no formato: Bearer {seu-token}",
                };

                document.AddComponent("Bearer", bearerScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>(),
                };

                if (document.Paths != null)
                {
                    foreach (var pathItem in document.Paths.Values)
                    {
                        if (pathItem.Operations != null)
                        {
                            foreach (var operation in pathItem.Operations)
                            {
                                operation.Value.Security ??= new List<OpenApiSecurityRequirement>();
                                operation.Value.Security.Add(securityRequirement);
                            }
                        }
                    }
                }
            }
        }
    }
}
