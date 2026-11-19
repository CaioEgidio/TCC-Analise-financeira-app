// Descreve a autenticação Bearer no OpenAPI para permitir autorização no Swagger.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FinData.Infrastructure.OpenApi;

public sealed class BearerSecurityTransformer
    : IOpenApiDocumentTransformer
{
    // Acrescenta o esquema Bearer e identifica as operações protegidas no documento.
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                Description = "Cole somente o accessToken, sem aspas."
            };

        // Percorre as rotas descritas e exige Bearer apenas quando há autorização obrigatória.
        foreach (var group in context.DescriptionGroups)
        {
            foreach (var description in group.Items)
            {
                var metadata = description.ActionDescriptor.EndpointMetadata;

                var requiresAuthorization =
                    metadata.OfType<IAuthorizeData>().Any();

                var allowsAnonymous =
                    metadata.OfType<IAllowAnonymous>().Any();

                if (!requiresAuthorization || allowsAnonymous)
                    continue;

                var path = "/" + description.RelativePath?.Split('?')[0];

                if (!document.Paths.TryGetValue(path, out var pathItem))
                    continue;

                if (pathItem.Operations is null)
                    continue;

                foreach (var entry in pathItem.Operations)
                {
                    var matchesMethod = string.Equals(
                        entry.Key.ToString(),
                        description.HttpMethod,
                        StringComparison.OrdinalIgnoreCase);

                    if (!matchesMethod)
                        continue;

                    entry.Value.Security ??= [];

                    // A indicação no Swagger documenta a proteção; a autorização real é aplicada pelo middleware.
                    entry.Value.Security.Add(
                        new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecuritySchemeReference(
                                "Bearer", document)] = []
                        });
                }
            }
        }

        return Task.CompletedTask;
    }
}