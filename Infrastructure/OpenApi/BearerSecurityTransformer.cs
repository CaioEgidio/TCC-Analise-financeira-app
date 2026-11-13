using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace FinData.Infrastructure.OpenApi;

public sealed class BearerSecurityTransformer
    : IOpenApiDocumentTransformer
{
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