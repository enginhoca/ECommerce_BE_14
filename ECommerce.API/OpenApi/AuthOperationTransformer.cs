using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ECommerce.API.OpenApi;

public class AuthOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var hasAuthorize = context
            .Description
            .ActionDescriptor
            .EndpointMetadata
            .OfType<AuthorizeAttribute>()
            .Any();

        if (!hasAuthorize) return Task.CompletedTask;

        operation.Security = [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer",context.Document)]=[]
            }
        ];

        if (!operation.Responses!.ContainsKey("401"))
            operation.Responses["401"] = new OpenApiResponse
                { Description = "Unauthorized" };

        if (!operation.Responses.ContainsKey("403"))
            operation.Responses["403"] = new OpenApiResponse
                { Description = "Forbidden" };

        return Task.CompletedTask;

    }
}
